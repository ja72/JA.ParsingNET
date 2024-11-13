using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using JA.LinearAlgebra;

namespace JA.LinearAlgebra
{
    public record JaggedMatrix(double[][] Elements) : IQuantity
    {
        public static readonly JaggedMatrix Empty = Array.Empty<double[]>();
        public static implicit operator JaggedMatrix(double[][] matrix) => new(matrix);        
        public static implicit operator double[][](JaggedMatrix matrix) => matrix.Elements;

        #region Factory
        public JaggedMatrix(Vector[] matrix) :
            this(matrix.Select(row => row.Elements).ToArray())
        { }
        public JaggedMatrix(int rows, int columns)
            : this(Factory.CreateJagged<double>(rows, columns))
        { }
        public JaggedMatrix(int rows, int columns, Func<int, int, double> initializer)
            : this(Factory.CreateJagged(rows, columns, initializer))
        { }
        //static double[][] CreateJagged(int rows, int columns, Func<int, int, double> initializer = null)
        //{
        //    var result = new double[rows][];
        //    for (int i = 0; i < rows; i++)
        //    {
        //        var row = new double[columns];
        //        if (initializer!=null)
        //        {
        //            for (int j = 0; j < row.Length; j++)
        //            {
        //                row[j] = initializer(i, j);
        //            }
        //        }
        //        result[i] = row;
        //    }
        //    return result;
        //}

        public static JaggedMatrix Block(JaggedMatrix A, Vector b, Vector c, double d)
        {
            var result = Factory.CreateJagged<double>(A.Rows+1, A.Columns+1);
            for (int i = 0; i < A.Rows; i++)
            {
                Array.Copy(A.Elements[i], result[i], A.Columns);
                result[i][^1] = b.Elements[i];
            }
            Array.Copy(c.Elements, result[^0], A.Columns);
            result[^1][^1] = d;

            return new JaggedMatrix(result);
        }

        public bool SplitLast(out JaggedMatrix A, out Vector b, out Vector c, out double d)
        {
            if (Rows>1 && Columns>1)
            {
                A = this[..^1, ..^1];
                b = this[..^1, ^1];
                c = this[^1, ..^1];
                d = this[^1, ^1];
                return true;
            }
            A = null;
            b = null;
            c = null;
            d = 0;
            return false;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JaggedMatrix Zeros(int rows, int columns)
        {
            return new JaggedMatrix(rows, columns);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JaggedMatrix Identity(int size) => Diagonal(size, 1.0);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JaggedMatrix Diagonal(int size, double value)
            => new(Factory.CreateJagged(size, size, (i, j) => i==j ? value : 0));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static JaggedMatrix Diagonal(Vector vector)
            => new(Factory.CreateJagged(vector.Size, vector.Size, (i, j) => i==j ? vector[i] : 0));
        #endregion

        #region Properties
        public int Rank { get; } = 2;
        public int Rows { get; } = Elements.Length;
        public int Columns { get; } = Elements.Length>0 ? Elements[0].Length : 0;
        public int Count { get; } = Elements.Sum((r) => r.Length);
        double IQuantity.Value { get; } = 0;
        double[] IQuantity.Array { get; } = null; 
        double[][] IQuantity.JaggedArray { get; } = Elements; 

        /// <summary>
        /// Reference an element of the matrix
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="column">The column index.</param>
        public ref double this[Index row, Index column]
            => ref Elements[row][column];
        /// <summary>
        /// Extract a sub-matrix.
        /// </summary>
        /// <param name="rows">The range of rows.</param>
        /// <param name="columns">The range of columns.</param>
        public double[][] this[Range rows, Range columns]
        {
            get
            {
                var slice = Elements[rows];
                for (int i = 0; i<slice.Length; i++)
                {
                    slice[i]=slice[i][columns];
                }
                return slice;
            }
        }

        /// <summary>
        /// Extract a row sub-vector.
        /// </summary>
        /// <param name="row">The row index.</param>
        /// <param name="columns">The range of columns.</param>
        public double[] this[Index row, Range columns]
        {
            get
            {
                var slice = Elements[row];
                var result = slice[columns];
                return result;
            }
        }

        /// <summary>
        /// Extract a column sub-vector.
        /// </summary>
        /// <param name="rows">The range of rows.</param>
        /// <param name="column">The column index.</param>
        public double[] this[Range rows, Index column]
        {
            get
            {
                var slice = Elements[rows];
                var result = new double[slice.Length];
                for (int i = 0; i<slice.Length; i++)
                {
                    result[i]=slice[i][column];
                }
                return result;
            }
        }

        public ReadOnlySpan<double[]> AsSpan() => AsSpan(Range.All);
        public ReadOnlySpan<double[]> AsSpan(Range rows)
        {
            var (offset, length)=rows.GetOffsetAndLength(Elements.Length);
            return new ReadOnlySpan<double[]>(Elements, offset, length);
        }

        #endregion

        #region Formatting
        public static bool ShowAsTable { get; set; } = true;
        public string DefaultFormatting { get; } = "g6";
        public override string ToString() => ToString(DefaultFormatting);
        public string ToString(string formatting) => ToString(formatting, null);
        public string ToString(string formatting, IFormatProvider formatProvider)
        {
            if (ShowAsTable)
            {
                int n = Elements.Length;
                int m = n > 0 ? Elements[0].Length : 0;
                var width = new int[m];
                var lines = new string[n][];
                for (int i = 0; i<lines.Length; i++)
                {
                    var row = new string[ m ];
                    double[] current = Elements[i];
                    for (int j = 0; j<row.Length; j++)
                    {
                        if (i==0) width[j]=0;
                        row[j]=current[j].ToString(formatting, formatProvider);
                        width[j]=Math.Max(width[j], row[j].Length);
                    }
                    lines[i]=row;
                }
                var sb = new StringBuilder();
                sb.AppendLine();
                for (int i = 0; i<n; i++)
                {
                    var row = lines[i];
                    sb.Append("|");
                    for (int j = 0; j<m; j++)
                    {
                        sb.Append($" {row[j].PadLeft(width[j])}");
                    }
                    sb.Append(" |");
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            else
            {
                return $"[{string.Join(",",
                    Elements.Select(row => 
                        $"[{string.Join(",", 
                            row.Select(x => x.ToString(formatting, formatProvider))
                            )}]"
                        )
                    )}]";
            }
        }
        #endregion

        #region Algebra

        public static JaggedMatrix Add(JaggedMatrix A, JaggedMatrix B)
        {
            var result = new double[A.Rows][];
            for (int i = 0; i < result.Length; i++)
            {
                var row = new double[A.Columns];
                var Arow = A.Elements[i];
                var Brow = B.Elements[i];
                for (int j = 0; j < row.Length; j++)
                {
                    row[j] = Arow[j] + Brow[j];
                }
                result[i] = row;
            }
            return new JaggedMatrix(result);
        }

        public static JaggedMatrix Subtract(JaggedMatrix A, JaggedMatrix B)
        {
            var result = new double[A.Rows][];
            for (int i = 0; i < result.Length; i++)
            {
                var row = new double[A.Columns];
                var Arow = A.Elements[i];
                var Brow = B.Elements[i];
                for (int j = 0; j < row.Length; j++)
                {
                    row[j] = Arow[j] - Brow[j];
                }
                result[i] = row;
            }
            return new JaggedMatrix(result);
        }

        public static JaggedMatrix Scale(double factor, JaggedMatrix A)
        {
            var result = new double[A.Rows][];
            for (int i = 0; i < result.Length; i++)
            {
                var row = new double[A.Columns];
                var Arow = A.Elements[i];
                for (int j = 0; j < row.Length; j++)
                {
                    row[j] = factor * Arow[j];
                }
                result[i] = row;
            }
            return new JaggedMatrix(result);
        }

        public static Vector Product(Vector a, JaggedMatrix B) => Product(B.Transpose(), a);
        public static Vector Product(JaggedMatrix A, Vector b)
        {
            var result = new double[A.Rows];
            for (int i = 0; i < result.Length; i++)
            {
                double sum = 0;
                var Arow = A.Elements[i];
                for (int k = 0; k < b.Elements.Length; k++)
                {
                    sum += Arow[k] * b.Elements[k];
                }
                result[i] = sum;
            }
            return new Vector(result);
        }

        public static JaggedMatrix Product(JaggedMatrix A, JaggedMatrix B)
        {
            var result = new double[A.Rows][];
            for (int i = 0; i < result.Length; i++)
            {
                var row = new double[B.Columns];
                var Arow = A.Elements[i];
                for (int j = 0; j < row.Length; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < Arow.Length; k++)
                    {
                        sum += Arow[k] * B.Elements[k][j];
                    }
                    row[j] = sum;
                }
                result[i] = row;
            }
            return new JaggedMatrix(result);
        }

        public JaggedMatrix Transpose()
        {
            int n = Rows;
            int m = Columns;
            var result = new double[m][];
            for (int i = 0; i < result.Length; i++)
            {
                var row = new double[n];
                for (int j = 0; j < row.Length; j++)
                {
                    row[j] = Elements[j][i];
                }
                result[i] = row;
            }
            return result;
        }

        public JaggedMatrix Inverse() => Solve(Identity(Rows));
        public Vector Solve(Vector vector)
        {
            if (Rows != vector.Size)
            {
                throw new ArgumentOutOfRangeException(nameof(vector), "Mismatch between matrix rows and vector size.");
            }

            if (Rows==1 && Columns==1 && vector.Size==1)
            {
                return new Vector(vector.Elements[0]/ Elements[0][0]);
            }
            if (SplitLast(out var A, out var b, out var c, out var d))
            {
                if (vector.SplitLast(out var u, out var y))
                {
                    var Au = A.Solve(u);
                    var Ab = A.Solve(b);

                    double x = (y - Vector.Dot(c, Au))/(d - Vector.Dot(c,Ab));
                    Vector v = Au - x*Ab;

                    var result = new double[Rows];
                    Array.Copy(v.Elements, result, result.Length-1);
                    result[^1] = x;

                    return result;
                }
            }
            throw new ArgumentException("Invalid inputs.", nameof(vector));
        }

        public JaggedMatrix Solve(JaggedMatrix matrix)
        {
            if (Rows != matrix.Rows)
            {
                throw new ArgumentOutOfRangeException(nameof(matrix), "Mismatch between matrix rows.");
            }

            if (Rows==1 && Columns==1 && matrix.Rows==1 && matrix.Columns==1)
            {
                var result = Factory.CreateJagged<double>(1,1);
                result[0][0] = matrix.Elements[0][0]/this.Elements[0][0];
                return new JaggedMatrix(result);
            }

            if (SplitLast(out var A, out var b, out var c, out var d))
            {
                if (matrix.SplitLast(out var U, out var u, out var h, out var y))
                {
                    var Au = A.Solve(u);
                    var Ab = A.Solve(b);
                    double x = (y - Vector.Dot(c, Au))/(d - Vector.Dot(c,Ab));

                    JaggedMatrix Abc = A - Vector.Outer(b, c);
                    JaggedMatrix V = Abc.Solve(d*U - Vector.Outer(b,h));
                    Vector v = A.Solve( u - x*b);
                    Vector g = (h - c*V)/d;

                    return Block(V, v, g, x);
                }
            }
            throw new ArgumentException("Invalid inputs.", nameof(matrix));
        }
        #endregion

        #region Operators
        public static JaggedMatrix operator +(JaggedMatrix A, JaggedMatrix B) => Add(A, B);
        public static JaggedMatrix operator -(JaggedMatrix A, JaggedMatrix B) => Subtract(A, B);
        public static JaggedMatrix operator -(JaggedMatrix A) => Scale(-1, A);
        public static JaggedMatrix operator *(double factor, JaggedMatrix B) => Scale(factor, B);
        public static JaggedMatrix operator *(JaggedMatrix A, double factor) => Scale(factor, A);
        public static Vector operator *(JaggedMatrix A, Vector b) => Product(A, b);
        public static JaggedMatrix operator *(JaggedMatrix A, JaggedMatrix B) => Product(A, B);
        public static Vector operator *(Vector a, JaggedMatrix B) => Product(a, B);
        public static JaggedMatrix operator /(JaggedMatrix A, double divisor) => Scale(1/divisor, A);
        public static JaggedMatrix operator ~(JaggedMatrix A) => A.Transpose();
        public static JaggedMatrix operator !(JaggedMatrix A) => A.Inverse();
        public static Vector operator /(Vector b, JaggedMatrix A) => A.Solve(b);
        public static JaggedMatrix operator /(JaggedMatrix B, JaggedMatrix A) => A.Solve(B);
        #endregion

    }

}
