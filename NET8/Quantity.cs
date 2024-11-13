using System;
using JA.Expressions;

namespace JA
{
    /// <summary>
    /// Represents the return type of an expression evaluation, <see cref="Expressions.Expr.Eval((string sym, double val)[])"/>. 
    /// It might be a scalar, a vector or a matrix.
    /// </summary>    
    public interface IQuantity : IFormattableExpr
    {
        public double Value { get; }
        public double[] Array { get; }
        public double[][] JaggedArray { get; }
        public bool IsScalar { get => Rank==0; }
        public bool IsArray { get => Rank==1; }
        public bool IsMatrix { get => Rank==2; }
        public int Rank { get; }
        public static IQuantity Scalar(double value) => new Scalar(value);
        public static IQuantity Vector(params double[] elements) => new LinearAlgebra.Vector(elements);
        public static IQuantity Jagged(double[][] elements) => new LinearAlgebra.JaggedMatrix(elements);
    }

}
