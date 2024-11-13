using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Linq;
using JA.Expressions;
using System.Text.Json.Serialization;
using JA.LinearAlgebra;

namespace JA
{

    static class Program
    {
        static int testIndex = 0;
        static void Main(string[] args)
        {                                    
            ParseExpressionDemo();
            CompileExpressionDemo();
            SimplifyExpressionDemo();
            AssignExpressionDemo();
            SystemOfEquationsDemo();
            CalculusExprDemo();
            CalculusDerivativeDemo();
            CalculusSolveDemo();
            CompileArrayDemo();
            CompileMatrixDemo();
            DoublePendulumDemo();
                        
        }


        private static void CalculusExprDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            VariableExpr x = "x", y="y";

            var f_input = "((x-1)*(x+1))/((x)^2+1)";
            Console.WriteLine($"input: {f_input}");
            var f = Expr.Parse(f_input).GetFunction("f");
            Console.WriteLine(f);
            var fx = f.CompileArg1();
            Console.WriteLine($"f(0.5)={fx(0.5)}");

            Console.WriteLine("Partial Derivative");
            var df = f.PartialDerivative(x);
            Console.WriteLine($"df/dx: {df}");

            Console.WriteLine("Total Derivative");
            var fp = f.TotalDerivative();
            Console.WriteLine(fp);

            Console.WriteLine();
            var w_input = "x^2 + 2*x*y + x/y";
            Console.WriteLine($"input: {w_input}");
            var w = Expr.Parse(w_input).GetFunction("w", "x", "y");
            Console.WriteLine(w);

            Console.WriteLine("Patial Derivatives");
            var wx = w.PartialDerivative(x);
            Console.WriteLine($"dw/dx: {wx}");
            var wy = w.PartialDerivative(y);
            Console.WriteLine($"dw/dy: {wy}");

            Console.WriteLine("Total Derivative");
            Console.WriteLine($"Set xp=v, yp=3");
            var wp = w.TotalDerivative("wp", ("x", "v"), ("y", 3.0));
            Console.WriteLine(wp);
            Console.WriteLine();
        }

        static void ParseExpressionDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            Console.WriteLine("Evaluate Expression");
            var text = "1/t^2*(1-exp(-pi*t))/(1-t^2)";
            var expr = Expr.Parse(text);
            Console.WriteLine(expr);
            Console.WriteLine($"Symbols: {string.Join(",", expr.GetSymbols())}");
            Console.WriteLine($"Rank:{expr.Rank}");
            Console.WriteLine($"{"t",-12} {"expr",-12}");
            for (int i = 1; i <= 10; i++)
            {
                var t = 0.10 * i;
                var x = expr.Eval(("t", t));
                Console.WriteLine($"{t,-12:g4} {x,-12:g4}");
            }
            Console.WriteLine();
        }
        static void CompileExpressionDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            Console.WriteLine("Compile Expression");
            var text = "1/t^2*(1-exp(-pi*t))/(1-t^2)";
            var expr = Expr.Parse(text);
            var fun = new Function("f", expr, "t");
            Console.WriteLine(fun);
            Console.WriteLine($"Rank:{fun.Rank}");
            var fq = fun.Compile<FArg1>();
            Console.WriteLine($"{"t",-12} {"f(t)",-12}");
            for (int i = 1; i <= 10; i++)
            {
                var t = 0.10 * i;
                var x = fq(t);
                Console.WriteLine($"{t,-12:g4} {x,-12:g4}");
            }
            Console.WriteLine();
        }
        static void SimplifyExpressionDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            VariableExpr x = "x", y = "y";
            //double a = 3, b = 0.25;
            Expr a = "a=3", b = "b=0.25";

            Console.WriteLine($"{a}={(double)a}, {b}={(double)b}, {x}, {y}");

            Console.WriteLine();
            int index = 0;
            Console.WriteLine($"{index,3}.     definition = result");
            Console.WriteLine($"{++index,3}. ({a}+x)+({b}+y) = {(a+x)+(b+y)}");
            Console.WriteLine($"{++index,3}. ({a}+x)-({b}+y) = {(a+x)-(b+y)}");
            Console.WriteLine($"{++index,3}. ({a}-x)+({b}+y) = {(a-x)+(b+y)}");
            Console.WriteLine($"{++index,3}. ({a}+x)-({b}-y) = {(a+x)-(b-y)}");
            Console.WriteLine($"{++index,3}. ({a}-x)+({b}-y) = {(a-x)+(b-y)}");
            Console.WriteLine($"{++index,3}. ({a}-x)-({b}-y) = {(a-x)-(b-y)}");
            Console.WriteLine($"{++index,3}. ({a}*x)+({b}*y) = {(a*x)+(b*y)}");
            Console.WriteLine($"{++index,3}. ({a}*x)-({b}*y) = {(a*x)-(b*y)}");
            Console.WriteLine($"{++index,3}. ({a}/x)+({b}*y) = {(a/x)+(b*y)}");
            Console.WriteLine($"{++index,3}. ({a}*x)-({b}/y) = {(a*x)-(b/y)}");
            Console.WriteLine($"{++index,3}. ({a}/x)+({b}/y) = {(a/x)+(b/y)}");
            Console.WriteLine($"{++index,3}. ({a}/x)-({b}/y) = {(a/x)-(b/y)}");

            Console.WriteLine($"{++index,3}. ({a}+x)*({b}+y) = {(a+x)*(b+y)}");
            Console.WriteLine($"{++index,3}. ({a}+x)/({b}+y) = {(a+x)/(b+y)}");
            Console.WriteLine($"{++index,3}. ({a}-x)*({b}+y) = {(a-x)*(b+y)}");
            Console.WriteLine($"{++index,3}. ({a}+x)/({b}-y) = {(a+x)/(b-y)}");
            Console.WriteLine($"{++index,3}. ({a}-x)*({b}-y) = {(a-x)*(b-y)}");
            Console.WriteLine($"{++index,3}. ({a}-x)/({b}-y) = {(a-x)/(b-y)}");
            Console.WriteLine($"{++index,3}. ({a}*x)*({b}*y) = {(a*x)*(b*y)}");
            Console.WriteLine($"{++index,3}. ({a}*x)/({b}*y) = {(a*x)/(b*y)}");
            Console.WriteLine($"{++index,3}. ({a}/x)*({b}*y) = {(a/x)*(b*y)}");
            Console.WriteLine($"{++index,3}. ({a}*x)/({b}/y) = {(a*x)/(b/y)}");
            Console.WriteLine($"{++index,3}. ({a}/x)*({b}/y) = {(a/x)*(b/y)}");
            Console.WriteLine($"{++index,3}. ({a}/x)/({b}/y) = {(a/x)/(b/y)}");
            Console.WriteLine();
        }
        static void CompileArrayDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            Console.WriteLine("Array Expression");
            var input = "abs([(1-t)^3,3*(1-t)^2*t,3*t^2*(1-t),t^3])";
            var expr = Expr.Parse(input);
            Console.WriteLine($"input: {input}");
            var y = expr.Eval(("t", 0.5));
            var fun = new Function("f", expr, "t");
            Console.WriteLine(fun);
            var f = fun.Compile<QArg1>();
            Console.WriteLine($"{"t",-12} {"f(t)",-12}");
            for (int i = 0; i <= 10; i++)
            {
                var t = 0.10 * i;
                var x = f(t);
                Console.WriteLine($"{t,-12:g4} {x,-18:g4}");
            }

            // TODO: How to implement vector functions?

            input = "dot([x,2*y],[4-x,-1+y])";
            Console.WriteLine($"input: {input}");
            expr = Expr.Parse(input);
            var f2 = new Function("f", expr, "x", "y");
            Console.WriteLine(f2);

            input = "dot(r_, r_) - outer(r_, r_)";
            Console.WriteLine($"f(r_) = {input}");
            Console.WriteLine("r_=[x,y,z]");
            input = input.Replace("r_", "[x,y,z]");
            expr = Expr.Parse(input);
            var f3 = new Function("f", expr, "x", "y", "z");
            Console.WriteLine(f3);

            Console.WriteLine();
        }

        static void CompileMatrixDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            Console.WriteLine("Matrix Expression");
            var tt = Expr.Variable("t");
            var expr = Expr.Jagged( new Expr[][] {
                [1/(1+tt^2), 1-(tt^2)/(1+tt^2)],
                [tt/(1+tt^2), -1/(1+tt^2)]});

            var fexpr = new Function("f", expr, "t");
            Console.WriteLine(fexpr);
            var f = fexpr.Compile<QArg1>();
            Console.WriteLine($"{"t",-12} {"f(t)"}");
            JaggedMatrix.ShowAsTable=false;
            for (int i = 0; i <= 10; i++)
            {
                var t = 0.10 * i;
                var y = f(t);
                Console.Write($"{t,-12:g4}");
                Console.ForegroundColor=ConsoleColor.Cyan;
                Console.WriteLine($" {y:g4}");
                Console.ForegroundColor=ConsoleColor.Gray;
            }
            JaggedMatrix.ShowAsTable=true;
            var fp = fexpr.PartialDerivative(tt);
            Console.WriteLine(fp);

            Console.WriteLine("Solve a 2×2 system of equations.");
            var A = Expr.Parse("[[7,t],[-t,3]]");
            var b = Expr.Parse("[4,-1]");
            Console.WriteLine($"Coefficient Matrix, A={A}");
            Console.WriteLine($"Constant Vector, b={b}");
            var x = Expr.Solve(A, b);
            Console.WriteLine($"Solution Vector, x={x}");
            var r = b - A*x;
            Console.ForegroundColor=ConsoleColor.DarkYellow;
            Console.WriteLine($"Residual Vector, b-A*x={r}");
            Console.ForegroundColor=ConsoleColor.Gray;
            Console.WriteLine("Check residual for t=0..1");
            Console.WriteLine($"{"t",-12} {"residual"}");
            JaggedMatrix.ShowAsTable=false;
            for (int i = 0; i <= 10; i++)
            {
                var t = 0.10 * i;
                var rval = r.Eval(("t",t));
                Console.Write($"{t,-12:g4}");
                Console.ForegroundColor=ConsoleColor.DarkYellow;
                Console.WriteLine($" {rval:g8}");
                Console.ForegroundColor=ConsoleColor.Gray;
            }
            JaggedMatrix.ShowAsTable=true;
            Console.ForegroundColor=ConsoleColor.Gray;
            var DM = Expr.Matrix(2,2, 1.0, Expr.Pi*tt ,  0.0, 1-(tt^2) );

            Console.WriteLine($"Matrix = {DM}");

            var f_DM = new Function("f", DM, "t");
            Console.WriteLine(f_DM);
            var ft = f_DM.Compile<QArg1>();
            Console.WriteLine("f(0)=");
            Console.WriteLine($"{ft(1/double.Pi):g4}");

            Console.WriteLine();
        }
        static void CalculusDerivativeDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            VariableExpr x = "x";
            foreach (var op in KnownUnaryDictionary.Defined)
            {
                var f = Expr.Unary(op, x);
                var fp = f.PartialDerivative(x);
                Console.WriteLine($"d/dx({f}) = {fp}");
            }
            Console.WriteLine();
        }
        static void CalculusSolveDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            Console.WriteLine("Define a function and solve using Newton-Raphon.");

            Expr a = 7, b= 3;
            Expr x = "x";

            Function f = ( a*Expr.Sin(x)-b*x ).GetFunction("f");
            Console.WriteLine(f);

            Console.WriteLine("Find solution, such that f(x)=0");

            Scalar init = 3.0;
            Console.WriteLine($"Initial guess, x={init}");
            Scalar sol = (Scalar)f.NewtonRaphson(init, 0.0);

            var fx = f.CompileArg1();
            Console.WriteLine($"x={sol}, f(x)={fx(sol)}");

            Console.WriteLine();
        }

        static void AssignExpressionDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            Expr.ClearParameters();

            var input = "a+b = (2*a+2*b)/2";
            var ex_1 = Expr.Parse(input);
            Console.WriteLine(ex_1);
            var e_1 = ex_1.Eval(("a",1), ("b",3) );
            Console.WriteLine($"Eval = {e_1}");

            Console.WriteLine("Use = for assignment of constants.");

            VariableExpr a = "a", b= "b", c="c";
            Expr lhs = Expr.Array(a,b,c);
            Expr rhs = "[1,2,3]";
            Console.WriteLine($"{lhs}={rhs}");
            Expr ex_2 = Expr.Assign(lhs, rhs);
            Console.WriteLine(ex_2);

            input = "(a+b)*x-c";
            var f = Expr.Parse(input).GetFunction("f", "x");
            Console.WriteLine(f);
            var fx = f.CompileArg1();
            foreach (var x in new[] { -1.0, 0.0, 1.0 })
            {
                Console.WriteLine($"f({x})={fx(x)}");
            }

            Console.WriteLine();
        }
        static void SystemOfEquationsDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            Expr.ClearParameters();

            Console.WriteLine("Define a 3×3 system of equations.");
            var system = Expr.Parse("[2*x-y+3*z=15, x + 3*z/2 = 3, x+3*y = 1]");
            var vars = new[] { "x", "y", "z" };

            Console.WriteLine(system);
            Console.WriteLine();
            if (system.ExtractLinearSystem(vars, out JaggedMatrix A, out Vector b))
            {
                Console.WriteLine($"Unknowns: {string.Join(",", vars)}");
                Console.WriteLine();
                Console.WriteLine($"Coefficient Matrix A={A}");

                Console.WriteLine();
                Console.WriteLine($"Constant Vector b={b}");

                var x = A.Solve(b);
                Console.WriteLine($"Solution Vector x={x}");
                Console.ForegroundColor=ConsoleColor.DarkYellow;
                var r = b-A*x;
                Console.WriteLine($"Residual Vector r={r}");
                Console.ForegroundColor=ConsoleColor.Gray;
            }
            else
            {
                Console.WriteLine("Something went wrong, could not extract linear system from equations above.");
            }

            Console.WriteLine();
        }

        static void DoublePendulumDemo()
        {
            Console.ForegroundColor=ConsoleColor.Yellow;
            Console.WriteLine($"*** DEMO [{++testIndex}] : {GetMethodName()} ***");
            Console.ForegroundColor=ConsoleColor.Gray;
            Expr.ClearParameters();

            VariableExpr L = "L", m="m", q_1="q_1", qp_1="qp_1", q_2="q_2", qp_2="qp_2";

            Expr x_1 = L*Expr.Sin(q_1), y_1 = -L*Expr.Cos(q_1);
            Expr x_2 = x_1 + L*Expr.Sin(q_1 + q_2), y_2 = y_1 -L*Expr.Cos(q_1+q_2);

            var pos_1 = Expr.Vec3(x_1, y_1, Expr.Zero);
            var pos_2 = Expr.Vec3(x_2, y_2, Expr.Zero);

            Console.WriteLine($"pos_1 = {pos_1}");
            Console.WriteLine($"pos_2 = {pos_2}");

            var vec_1 = pos_1.PartialDerivative(q_1)*qp_1 + pos_1.PartialDerivative(q_2)*qp_2;
            var vec_2 = pos_2.PartialDerivative(q_1)*qp_1 + pos_2.PartialDerivative(q_2)*qp_2;

            Console.WriteLine($"vec_1 = {vec_1}");
            Console.WriteLine($"vec_2 = {vec_2}");
        }

        static string GetMethodName([CallerMemberName] string name = null)
        {
            return name;
        }
    }
}
