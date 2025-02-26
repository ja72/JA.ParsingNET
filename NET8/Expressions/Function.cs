﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JA.LinearAlgebra;

namespace JA.Expressions
{

    public class Function : IFormattable
    {
        public Function(string name, Expr body)
            : this(name, body, body.GetSymbols().ToArray())
        { 
        }
        internal Function(string name, Expr[] elements, params string[] arguments)
            : this(name, Expr.Array(elements), arguments)
        {
        }

        internal Function(string name, Expr body, params string[] arguments)
        {
            var sym = body.GetSymbols();
            var missing = sym.Except(arguments).ToArray();
            if (missing.Length>0)
            {
                throw new ArgumentOutOfRangeException(nameof(arguments),
                    $"Missing {string.Join(",", missing)} from parameter list.");
            }
            Name = name;
            Body = body;
            Arguments = arguments;
        }

        #region Properties
        public string Name { get; }
        public Expr Body { get; }
        public string[] Arguments { get; }
        public int Rank => Body.Rank;        
        #endregion

        #region Calculus
        public Function TotalDerivative() => TotalDerivative($"{Name}'");
        public Function TotalDerivative(string name)
        {
            var paramsAndDots = new List<string>(Arguments);
            var body = Body.TotalDerivative(ref paramsAndDots);
            return new Function(name, body, paramsAndDots.ToArray());
        }
        public Function TotalDerivative(string name, params (string sym, Expr expr)[] parameters)
        {
            var paramsAndDots = new List<string>(Arguments);
            var body = Body.TotalDerivative(parameters, ref paramsAndDots);
            return new Function(name, body, paramsAndDots.ToArray());
        }
        public Function PartialDerivative(string variable)
        {
            var body = Body.PartialDerivative(variable);
            return new Function($"{Name}_{variable}", body, Arguments);
        }

        public Expr Jacobian() => Body.Jacobian(Arguments);

        public IQuantity NewtonRaphson(IQuantity init, double target = 0, double tolerance = 1e-11, int maxIter = 100)
        {
            if (init.IsScalar && Arguments.Length != 1)
            {
                throw new ArgumentException("Missing parameters from inital values.", nameof(init));
            }
            if (init.IsArray && init.Array.Length != Arguments.Length)
            {
                throw new ArgumentException("Missing parameters from inital values.", nameof(init));
            }
            if (Rank!=0)
            {
                throw new NotSupportedException($"Rank:{Rank} is not supported.");
            }
            if (init.IsScalar &&  Arguments.Length==1)
            {
                var fx = CompileArg1();
                var fx_x = PartialDerivative(Arguments[0]).CompileArg1();
                var x = init.Value;
                var f = fx(x);
                var e = Math.Abs(f-target);
                int iter=0;
                while (e>tolerance && iter<maxIter)
                {
                    iter++;
                    var dx = -f/fx_x(x);
                    var lambda = 1.0;
                    var e_old = e;
                    f = fx(x + lambda* dx);
                    e = Math.Abs(f-target);
                    while (e>=e_old && lambda>1/maxIter)
                    {
                        lambda /= 2;
                        f = fx(x + lambda* dx);
                        e = Math.Abs(f-target);
                    }
                    x += lambda * dx;
                    f = fx(x);
                }
                return (Scalar)x;
            }
            else if (init.IsArray && Arguments.Length==2)
            {
                var fxy = CompileArg2();
                var fxy_x = PartialDerivative(Arguments[0]).CompileArg2();
                var fxy_y = PartialDerivative(Arguments[1]).CompileArg2();
                var x = init.Array[0];
                var y = init.Array[1];
                var f = fxy(x,y);
                var e = Math.Abs(f-target);
                int iter=0;
                Debug.WriteLine($"iter={iter}, f={f}, e={e}");
                while (e>tolerance && iter<maxIter)
                {
                    iter++;
                    var dx = -f/fxy_x(x, y);
                    var dy = -f/fxy_y(x, y);
                    var lambda = 1.0;
                    var e_old = e;
                    f = fxy(x + lambda* dx, y + lambda*dy);
                    e = Math.Abs(f-target);
                    Debug.WriteLine($"iter={iter}, f={f}, e={e}");
                    while (e>=e_old && lambda>1/maxIter)
                    {
                        lambda /= 2;
                        f = fxy(x + lambda* dx, y + lambda*dy);
                        e = Math.Abs(f-target);
                        Debug.WriteLine($"ratio={lambda}, f={f}, e={e}");
                    }

                    x += lambda*dx;
                    y += lambda*dy;
                    f = fxy(x,y);
                }
                return new Vector(x, y);
            }
            // TODO: Consider implementing the following in general
            // https://people.duke.edu/~kh269/teaching/b553/newtons_method.pdf
            throw new NotSupportedException($"{Arguments.Length} parameters not supported.");
        }
        #endregion

        #region Algebra
        public Function Substitute(string name, params (string symbol, double values)[] parameters)
        {
            List<string> arg = new(Arguments);
            Expr expr = Body.Substitute(parameters);
            foreach (var (symbol, _) in parameters)
            {
                arg.Remove(symbol);
            }
            return new Function(name, expr, arg.ToArray());
        }
        public Function Substitute(string name, params (string symbol, Expr subExpression)[] parameters)
        {
            List<string> arg = new(Arguments);
            Expr expr = Body.Substitute(parameters);
            foreach (var (symbol, _) in parameters)
            {
                arg.Remove(symbol);
            }
            return new Function(name, expr, arg.ToArray());
        }
        #endregion

        #region Compiling
        public static explicit operator FArg0(Function function) => function.CompileArg0();
        public static explicit operator FArg1(Function function) => function.CompileArg1();
        public static explicit operator FArg2(Function function) => function.CompileArg2();
        public static explicit operator FArg3(Function function) => function.CompileArg3();
        public static explicit operator FArg4(Function function) => function.CompileArg4();
        public static explicit operator VArg0(Function function) => function.CompileVArg0();
        public static explicit operator VArg1(Function function) => function.CompileVArg1();
        public static explicit operator VArg2(Function function) => function.CompileVArg2();

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public FArg0 CompileArg0() => Compile<FArg0>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public FArg1 CompileArg1() => Compile<FArg1>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public FArg2 CompileArg2() => Compile<FArg2>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public FArg3 CompileArg3() => Compile<FArg3>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public FArg4 CompileArg4() => Compile<FArg4>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public VArg0 CompileVArg0() => Compile<VArg0>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public VArg1 CompileVArg1() => Compile<VArg1>();
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public VArg2 CompileVArg2() => Compile<VArg2>();

        internal T Compile<T>()
        {
            var asm = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName("LCG"),
                    AssemblyBuilderAccess.RunAndCollect);

            var mod = asm.DefineDynamicModule("Lightweight");

            var tpe = mod.DefineType("Code");
            Type t_delegate = typeof(T);
            MethodInfo m_invoke = t_delegate.GetMethod("Invoke");
            // Return type
            var ret = m_invoke.ReturnType;
            //var arg_types = Enumerable.Repeat(typeof(double), Arguments.Length).ToArray();
            var args = m_invoke.GetParameters();
            // Get types for all arguments
            var arg_types = args.Select(a=> a.ParameterType).ToArray();
            if (arg_types.Length!=Arguments.Length)
            {
                throw new InvalidCastException($"Expecting delegate type with {Arguments.Length} arguments.");
            }
            var mtd = tpe.DefineMethod("Generation",
                MethodAttributes.Public | MethodAttributes.Static,
                ret,
                arg_types
            );

            // Establish environment; we could also use a List<T> and use IndexOf
            // in the recursive compilation code to find the parameter's index.
            var env = new Dictionary<string, (int index, Type type)>();
            for (int index = 0; index<Arguments.Length; index++)
            {
                env[ Arguments[index] ] = (index, arg_types[index]);
            }

            // Compilation is requested for the Body, and a final "ret" instruction
            // is added to the IL generator.
            var gen = mtd.GetILGenerator();

            Body.Compile(gen, env);

            if (ret.IsAssignableTo(typeof(IQuantity)))
            {
                switch (Body.Rank)
                {
                    case 0:
                        var fsinfo = typeof(Scalar).GetMethod("op_Implicit", new[] { typeof(double) });
                        gen.EmitCall(OpCodes.Call, fsinfo, null);
                        break;
                    case 1:
                        var fvinfo = typeof(Vector).GetMethod("op_Implicit", new[] { typeof(double[]) });
                        gen.EmitCall(OpCodes.Call, fvinfo, null);
                        break;
                    case 2:
                        var fminfo = typeof(JaggedMatrix).GetMethod("op_Implicit", new[] { typeof(double[][]) });
                        gen.EmitCall(OpCodes.Call, fminfo, null);
                        break;
                    default:
                        throw new NotSupportedException($"Rank:{Rank} is not supported.");
                }
            }
            gen.Emit(OpCodes.Ret);

            var res = tpe.CreateType();

            // Create a delegate of the specified type, referring to the generated
            // method which we always call "Generation" (see the DefineMethod call).
            return (T)(object)Delegate.CreateDelegate(
                t_delegate,
                res.GetMethod("Generation"));
        }

        #endregion

        #region Formatting
        public static bool ShowAsTable { get; set; } = true;
        public override string ToString() => ToString("g");
        public string ToString(string formatting) => ToString(formatting, null);
        public string ToString(string format, IFormatProvider provider)
        {
            var label = $"{Name}({string.Join(",", Arguments)}) = ";
            if (ShowAsTable)
            {
                if (Rank==1)
                {
                    return Body.ToArray().ToTableVector(format, label);
                }
            }
            string expr_str = Body.ToString(format, provider);
            return $"{Name}({string.Join(",", Arguments)})={expr_str}";
        }
        #endregion

    }
}
