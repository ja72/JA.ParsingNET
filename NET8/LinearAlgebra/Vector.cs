using System;
using System.Linq;
using System.Text;

namespace JA.LinearAlgebra
{
    public record Vector(params double[] Elements) : IQuantity
    {
        public static Vector Empty { get; } = Array.Empty<double>();
        public static implicit operator Vector(double[] array) => new(array);
        public static implicit operator double[](Vector vector) => vector.Elements;

        #region Factory
        public Vector(int size) : this(CreateVector(size)) { }
        public Vector(int size, Func<int, double> initializer) : this(CreateVector(size, initializer)) { }
        public static Vector Zero(int size) => new(size);
        public static Vector Elemental(int size, int oneIndex)
            => new(size, (i) => i==oneIndex ? 1 : 0);
        public static Vector Append(Vector a, double b)
        {
            var result = new double[a.Size + 1];
            Array.Copy(a.Elements, result, a.Size);
            result[^1]=b;
            return new Vector(result);
        }
        public static Vector Append(Vector a, Vector b)
        {
            var result = new double[a.Size + b.Size];
            Array.Copy(a.Elements, 0, result, 0, a.Size);
            Array.Copy(b.Elements, 0, result, a.Size, b.Size);            
            return new Vector(result);
        }
        static double[] CreateVector(int size, Func<int, double> initializer = null)
        {
            var result = new double[size];
            if (initializer!=null)
            {
                for (int i = 0; i<result.Length; i++)
                {
                    result[i]=initializer(i);
                }
            }
            return result;
        }
        #endregion

        #region Properties
        public bool SplitLast(out Vector vector, out double scalar)
        {
            if (Size>1)
            {
                vector=Elements[..^1];
                scalar=Elements[^1];
                return true;
            }
            vector=null;
            scalar=0;
            return false;
        }
        public int Rank { get; } = 1; 
        public int Size { get; } = Elements.Length; 
        double IQuantity.Value { get; } = 0; 
        double[] IQuantity.Array { get; } = Elements; 
        double[][] IQuantity.JaggedArray { get; } = new[] { Elements }; 
        public ref double this[Index index] => ref Elements[index];
        public double[] this[Range range] => Elements[range];
        public ReadOnlySpan<double> AsSpan() => AsSpan(Range.All);
        public ReadOnlySpan<double> AsSpan(Range range)
        {
            var (offset, length)=range.GetOffsetAndLength(Elements.Length);
            return new ReadOnlySpan<double>(Elements, offset, length);
        }


        public double[] Slice(int start, int length)
        {
            var slice = new double[length];
            Array.Copy(Elements, start, slice, 0, length);
            return slice;
        } 
        #endregion

        #region Formatting
        public static bool ShowAsTable { get; set; } = false;
        public string DefaultFormatting { get; } = "g6";
        public override string ToString() => ToString(DefaultFormatting);
        public string ToString(string formatting) => ToString(formatting, null);
        public string ToString(string formatting, IFormatProvider formatProvider)
        {
            if (ShowAsTable)
            {
                int n = Elements.GetLength(0);
                var width = 3;
                var lines = new string[n];
                for (int i = 0; i<lines.Length; i++)
                {
                    lines[i]=Elements[i].ToString(formatting, formatProvider);
                    width=Math.Max(width, lines[i].Length);
                }
                var sb = new StringBuilder();
                sb.AppendLine();
                for (int i = 0; i<n; i++)
                {
                    var row = lines[i];
                    sb.Append("|");
                    sb.Append($" {row.PadLeft(width)} |");
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            else
            {
                return $"[{string.Join(",", 
                    Elements.Select(x=> x.ToString(formatting, formatProvider)))}]";
            }
        }
        #endregion

        #region Algebra
        public static Vector Add(Vector A, Vector B)
        {
            var result = new double[A.Elements.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = A.Elements[i] + B.Elements[i];
            }
            return new Vector(result);
        }
        public static Vector Subtract(Vector A, Vector B)
        {
            var result = new double[A.Elements.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = A.Elements[i] - B.Elements[i];
            }
            return new Vector(result);
        }

        public static Vector Scale(double factor, Vector A)
        {
            var result = new double[A.Elements.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = factor * A.Elements[i];
            }
            return new Vector(result);
        }
        public static double Dot(Vector A, Vector B)
        {
            double sum = 0;
            for (int i = 0; i < A.Elements.Length; i++)
            {
                sum += A.Elements[i] * B.Elements[i];
            }
            return sum;
        }

        public static JaggedMatrix Outer(Vector A, Vector B) => new(A.Size, B.Size, (i, j)
            => A.Elements[i] * B.Elements[j]);

        #endregion

        #region Operators
        public static Vector operator +(Vector a, Vector b) => Add(a, b);
        public static Vector operator -(Vector a) => Scale(-1, a);
        public static Vector operator -(Vector a, Vector b) => Subtract(a, b);
        public static Vector operator *(double a, Vector b) => Scale(a, b);
        public static Vector operator *(Vector a, double b) => Scale(b, a);
        public static Vector operator /(Vector a, double b) => Scale(1 / b, a);
        public static double operator *(Vector a, Vector b) => Dot(a, b);
        #endregion
    }
}
