using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using JA.LinearAlgebra;

namespace JA.Expressions
{
    public partial record Expr
    {
        static Expr()
        {
            AdditiveIdentity=Zero;
            MultiplicativeIdentity=One;
        }
        public static Expr AdditiveIdentity { get; } 
        public static Expr MultiplicativeIdentity { get; } 

        #region Constants        
        public static NamedConstExpr Pi { get; } = new("pi");
        public static NamedConstExpr E { get; }  = new("e");
        public static NamedConstExpr Φ { get; }  = new("Φ");

        public static ConstExpr Zero { get; } = 0.0;
        public static ConstExpr One  { get; } = 1.0;
        public static Expr Deg       { get; } = Pi/180;
        public static Expr Rad       { get; } = 180/Pi;
        public static Expr Rpm       { get; } = Pi/30;
        #endregion

        #region Factory
        public static implicit operator Expr(string expr) => Parse(expr);
        public static implicit operator Expr(double value) => Const(value);
        public static implicit operator Expr(Vector vector) => Vector(vector.Elements);
        public static implicit operator Expr(JaggedMatrix matrix) => Jagged(matrix.Elements);

        public static explicit operator string(Expr expression)
        {
            if (expression.IsSymbol(out string symbol))
            {
                return symbol;
            }
            throw new ArgumentException("Argument must be a "+nameof(VariableExpr)+" type.", nameof(expression));
        }
        public static explicit operator double(Expr expression)
        {
            if (expression.IsConstant(out double value, true))
            {
                return value;
            }
            throw new ArgumentException("Argument must be a "+nameof(ConstExpr)+" type.", nameof(expression));
        }

        public static ConstExpr Const(double value)
        {
#pragma warning disable IDE0011 // Add braces
            if (value==0) return Zero;
            if (value==1) return One;
            if (value==Math.PI) return Pi;
            if (value==Math.E) return E;
            return new ConstExpr(value);
#pragma warning restore IDE0011 // Add braces
        }
        public static Expr ConstOrVariable(string symbol)
        {
            if (ConstOp.IsConst(symbol, out var op))
            {
                return Const(op);
            }
            return Variable(symbol);

        }
        public static NamedConstExpr Const(ConstOp op) => new(op);
        public static NamedConstExpr Const(string symbol, double value)
        {
            if (ConstOp.IsConst(symbol, out var op))
            {
                if (op.Value!=value)
                {
                    throw new ArgumentException($"Cannot change the value of the constant {symbol}");
                }
                return Const(op);
            }
            if (parameters.TryAdd(symbol, value))
            {
                //parameters[symbol]=value;
            }
            return new NamedConstExpr(symbol, value);
        }

        public static VariableExpr Variable(string name) => new(name);

        public static Expr Unary(UnaryOp Op, Expr Argument)
        {
            if (Argument.IsConstant(out double value))
            {
                return Op.Function(value);
            }

            if (Argument.IsAssign(out var leftAsgn, out var rightAsgn))
            {
                return Assign(Unary(Op, leftAsgn), Unary(Op, rightAsgn));
            }

            if (Argument.IsArray(out var arrayExpr))
            {
                return Array(UnaryVectorOp(Op, arrayExpr));
            }

            if (Argument.IsUnary(out string argOp, out Expr argArg))
            {
#pragma warning disable IDE0011 // Add braces
                if (Op.Identifier=="-"&&argOp=="-") return argArg;
                if (Op.Identifier=="inv"&&argOp=="inv") return argArg;
                if (Op.Identifier=="ln"&&argOp=="exp") return argArg;
                if (Op.Identifier=="exp"&&argOp=="ln") return argArg;
                if (Op.Identifier=="sqrt"&&argOp=="sqr") return Abs(argArg);
                if (Op.Identifier=="cbrt"&&argOp=="cub") return argArg;
                if (Op.Identifier=="cub"&&argOp=="cbrt") return argArg;
                if (Op.Identifier=="sin"&&argOp=="asin") return argArg;
                if (Op.Identifier=="cos"&&argOp=="acos") return argArg;
                if (Op.Identifier=="tan"&&argOp=="atan") return argArg;
                if (Op.Identifier=="asin"&&argOp=="sin") return argArg;
                if (Op.Identifier=="acos"&&argOp=="cos") return argArg;
                if (Op.Identifier=="atan"&&argOp=="tan") return argArg;
                if (Op.Identifier=="sinh"&&argOp=="asinh") return argArg;
                if (Op.Identifier=="cosh"&&argOp=="acosh") return argArg;
                if (Op.Identifier=="tanh"&&argOp=="atanh") return argArg;
                if (Op.Identifier=="asinh"&&argOp=="sinh") return argArg;
                if (Op.Identifier=="acosh"&&argOp=="cosh") return argArg;
                if (Op.Identifier=="atanh"&&argOp=="tanh") return argArg;
#pragma warning restore IDE0011 // Add braces
            }

            //if (Op.Identifier=="+") return Argument;
            return Op.Identifier switch
            {
                "+"     => Argument,
                "-"     => Negate(Argument),
                "inv"   => 1/Argument,
                "ln"    => Ln(Argument),
                "exp"   => Exp(Argument),
                "sqrt"  => Sqrt(Argument),
                "sqr"   => Sqr(Argument),
                "sin"   => Sin(Argument),
                "cos"   => Cos(Argument),
                "tan"   => Tan(Argument),
                "sind"  => Sin(Deg*Argument),
                "cosd"  => Cos(Deg*Argument),
                "tand"  => Tan(Deg*Argument),
                "sinh"  => Sinh(Argument),
                "cosh"  => Cosh(Argument),
                "tanh"  => Tanh(Argument),
                "asin"  => Asin(Argument),
                "acos"  => Acos(Argument),
                "atan"  => Atan(Argument),
                "asinh" => Asinh(Argument),
                "acosh" => Acosh(Argument),
                "atanh" => Atanh(Argument),
                _       => new UnaryExpr(Op, Argument),
            };
        }
        public static Expr Binary(BinaryOp Op, Expr Left, Expr Right)
        {
            if (Left.IsScalar()&&Right.IsJaggedMatrix(out var rightMatrix))
            {
                Left=Jagged(Factory.DiagonalJagged(rightMatrix.Length, Left));

                return Binary(Op, Left, Right);
            }
            if (Left.IsJaggedMatrix(out var leftMatrix)&&Right.IsScalar())
            {
                Right=Jagged(Factory.DiagonalJagged(leftMatrix.Length, Right));

                return Binary(Op, Left, Right);
            }
            if (IsVectorizable(Left, Right, out var leftArray, out var rightArray, true))
            {
                return Array(BinaryVectorOp(Op, leftArray, rightArray));
            }

            switch (Op.Identifier)
            {
                case "=": return Assign(Left, Right);
                case "+": return Add(Left, Right);
                case "-": return Subtract(Left, Right);
                case "*": return Multiply(Left, Right);
                case "/": return Divide(Left, Right);
                case "^": return Power(Left, Right);
                case "log" when Right.IsConstant(out var newBase): return Log(Left, newBase);
            }

            if (Left.IsConstant(out var leftValue)&&Right.IsConstant(out var rightValue))
            {
                return Op.Function(leftValue, rightValue);
            }

            if (Left.IsAssign(out var leftAsgnLeft, out var leftAsgnRight)
                &&Right.IsAssign(out var rightAsgnLeft, out var rightAsgnRight))
            {
                // (a=b) + (c=d) => (a+c) = (b+d)
                return Assign(
                    Binary(Op, leftAsgnLeft, rightAsgnLeft),
                    Binary(Op, leftAsgnRight, rightAsgnRight));
            }
            if (Left.IsAssign(out leftAsgnLeft, out leftAsgnRight))
            {
                // (a=b) + c => (a+c) = (b+c)
                return Assign(
                    Binary(Op, leftAsgnLeft, Right),
                    Binary(Op, leftAsgnRight, Right));
            }
            if (Right.IsAssign(out rightAsgnLeft, out rightAsgnRight))
            {
                // (a) + (c=d) => (a+c) = (a+d)
                return Assign(
                    Binary(Op, Left, rightAsgnLeft),
                    Binary(Op, Left, rightAsgnRight));
            }

            return new BinaryExpr(Op, Left, Right);
        }


        #endregion

    }
}
