using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using JA.Expressions;

namespace JA.LinearAlgebra.Mechanics
{
    public record Vec3Expr(Expr X, Expr Y, Expr Z) : ArrayExpr([X, Y, Z])
    {        
        public static Vec3Expr Elemental(int index, Expr value)
        {
            return index switch
            {
                0 => new Vec3Expr(value, Zero, Zero),
                1 => new Vec3Expr(Zero, value, Zero),
                2 => new Vec3Expr(Zero, Zero, value),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        }
        public static Vec3Expr Zeros { get; } = new Vec3Expr(Zero, Zero, Zero);
        public static Vec3Expr UX { get; } = new Vec3Expr(One, Zero, Zero);
        public static Vec3Expr UY { get; } = new Vec3Expr(Zero, One, Zero);
        public static Vec3Expr UZ { get; } = new Vec3Expr(Zero, Zero, One);
        public static Expr Dot(Vec3Expr a, Vec3Expr b)
            => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static Vec3Expr Cross(Vec3Expr a, Vec3Expr b)
            => new Vec3Expr(
                a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X);
        public static Mat3Expr Outer(Vec3Expr a, Vec3Expr b)
            => new Mat3Expr(
                a.X*b.X, a.X*b.Y, a.X*b.Z,
                a.Y*b.X, a.Y*b.Y, a.Y*b.Z,
                a.Z*b.X, a.Z*b.Y, a.Z*b.Z);

        public Mat3Expr Cross()        
            => new Mat3Expr(
                Zero, -Z, Y,
                Z, Zero, -X,
                -Y, X, Zero);
        
    }

    public record Mat3Expr(Vec3Expr Row1, Vec3Expr Row2, Vec3Expr Row3) : ArrayExpr([Row1, Row2, Row3])
    {
        public static Mat3Expr FromRows(Vec3Expr Row1, Vec3Expr Row2, Vec3Expr Row3)
            => new Mat3Expr(Row1, Row2, Row3);

        public static Mat3Expr FromColumns(Vec3Expr Column1, Vec3Expr Column2, Vec3Expr Column3)
            => new Mat3Expr(Column1, Column2, Column3).Transpose();

        public Mat3Expr(
            Expr a11, Expr a12, Expr a13,
            Expr a21, Expr a22, Expr a23,
            Expr a31, Expr a32, Expr a33)
            : this(
                new Vec3Expr(a11, a12, a13),
                new Vec3Expr(a21, a22, a23),
                new Vec3Expr(a31, a32, a33))
        { }
        public static Mat3Expr Zeros { get; } = new Mat3Expr(
            Vec3Expr.Zeros, 
            Vec3Expr.Zeros, 
            Vec3Expr.Zeros);
        public static Mat3Expr Identity { get; } = new Mat3Expr(
            Vec3Expr.UX, 
            Vec3Expr.UY, 
            Vec3Expr.UZ);
        public static Mat3Expr Diagnonal(Expr d_1, Expr d_2, Expr d_3)
        {
            return new Mat3Expr(
                Vec3Expr.Elemental(0, d_1),
                Vec3Expr.Elemental(1, d_2),
                Vec3Expr.Elemental(2, d_3));
        }
        public static Mat3Expr Scalar(Expr value) => Diagnonal(value, value, value);
        public Vec3Expr Col1 { get; } = new Vec3Expr( Row1.X, Row2.X, Row3.X );
        public Vec3Expr Col2 { get; } = new Vec3Expr( Row1.Y, Row2.Y, Row3.Y );
        public Vec3Expr Col3 { get; } = new Vec3Expr( Row1.Z, Row2.Z, Row3.Z );

        public Expr A11 { get; } = Row1.X;
        public Expr A12 { get; } = Row1.Y;
        public Expr A13 { get; } = Row1.Z;
        public Expr A21 { get; } = Row2.X;
        public Expr A22 { get; } = Row2.Y;
        public Expr A23 { get; } = Row2.Z;
        public Expr A31 { get; } = Row3.X;
        public Expr A32 { get; } = Row3.Y;
        public Expr A33 { get; } = Row3.Z;
        public Mat3Expr Transpose()
        {
            return new Mat3Expr(Col1, Col2, Col3);
        }
        public Expr Determinant() => Dot(Row1, Cross(Row2, Row3));

        public static Vec3Expr Product(Mat3Expr A, Vec3Expr b) 
            => new Vec3Expr(
                Vec3Expr.Dot(A.Row1, b),
                Vec3Expr.Dot(A.Row2, b),
                Vec3Expr.Dot(A.Row3, b));

        public static Mat3Expr Product(Mat3Expr A, Mat3Expr B)
            => new Mat3Expr(
                Vec3Expr.Dot(A.Row1, B.Col1), Vec3Expr.Dot(A.Row1, B.Col2), Vec3Expr.Dot(A.Row1, B.Col3),
                Vec3Expr.Dot(A.Row2, B.Col1), Vec3Expr.Dot(A.Row2, B.Col2), Vec3Expr.Dot(A.Row2, B.Col3),
                Vec3Expr.Dot(A.Row3, B.Col1), Vec3Expr.Dot(A.Row3, B.Col2), Vec3Expr.Dot(A.Row3, B.Col3));

        public Mat3Expr Inverse()
        {
            var D = Determinant();
            var row1 = new Vec3Expr(
                Row2.Y*Row3.Z - Row2.Z*Row3.Y,
                Row3.Y*Row1.Z - Row3.Z*Row1.Y,
                Row1.Y*Row2.Z - Row1.Z*Row2.Y);
            var row2 = new Vec3Expr(
                Row2.Z*Row3.X - Row2.X*Row3.Z,
                Row3.Z*Row1.X - Row3.X*Row1.Z,
                Row1.Z*Row2.X - Row1.X*Row2.Z);
            var row3 = new Vec3Expr(
                Row2.X*Row3.Y - Row2.Y*Row3.X,
                Row3.X*Row1.Y - Row3.Y*Row1.X,
                Row1.X*Row2.Y - Row1.Y*Row2.X);

            return new Mat3Expr(row1, row2, row3)/D as Mat3Expr;
        }

        public Vec3Expr Solve(Vec3Expr b)
        {
            return Inverse()*b;
        }
        public Mat3Expr Solve(Mat3Expr B)
        {
            return Inverse()*B;
        }

        public static Vec3Expr operator *(Mat3Expr A, Vec3Expr b) => Product(A, b);
        public static Mat3Expr operator *(Mat3Expr A, Mat3Expr B) => Product(A, B);

        public void Decompose(out Mat3Expr L, out Mat3Expr U, out Mat3Expr D)
        {
            var d11 = A11;
            var U12 = A12/d11;
            var U13 = A13/d11;
            var L21 = A21/d11;
            var L31 = A31/d11;
            var d22 = A22 - L21*A11*U12;
            var U23 = (A23 - L21*A11*U13)/d22;
            var L32 = (A32 - L31*A11*U12)/d22;
            var d33 = A33 - L32*d22*U23 - L31*d11*U13;

            L=new Mat3Expr(
                One, Zero, Zero,
                L21, One, Zero,
                L31, L32, One);
            U=new Mat3Expr(
                One, U12, U13,
                Zero, One, U23,
                Zero, Zero, One);
            D=new Mat3Expr(
                d11, Zero, Zero,
                Zero, d22, Zero,
                Zero, Zero, d33);
        }
    }
}
