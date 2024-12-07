using System;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

using JA.Expressions;

namespace JA.LinearAlgebra
{
    public static class Factory
    {
        public static T[] CreateVector<T>(int size) where T : IAdditiveIdentity<T, T>
        {
            var result = new T[size];
            System.Array.Fill(result, T.AdditiveIdentity);
            return result;
        }
        public static T[] CreateVector<T>(int size, T value)
        {
            var result = new T[size];
            System.Array.Fill(result, value);
            return result;
        }
        public static T[] CreateVector<T>(int size, Func<int, T> initializer)
        {
            var result = new T[size];
            if (initializer!=null)
            {
                for (int i = 0; i<result.Length; i++)
                {
                    result[i]=initializer(i);
                }
            }
            return result;
        }
        public static T[] CreateVector<T>(int size, Func<int, string> initializer)
            where T : IParsable<T>
        {
            var result = new T[size];
            if (initializer!=null)
            {
                for (int i = 0; i<result.Length; i++)
                {
                    result[i]=T.Parse(initializer(i), CultureInfo.CurrentCulture.NumberFormat);
                }
            }
            return result;
        }
        public static T[,] CreateMatrix<T>(int rows, int columns) where T : IAdditiveIdentity<T, T>
        {
            var result = new T[rows, columns];
            for (int i = 0; i<rows; i++)
            {
                for (int j = 0; j<columns; j++)
                {
                    result[i, j]=T.AdditiveIdentity;
                }
            }
            return result;
        }
        public static T[,] CreateMatrix<T>(int rows, int columns, T value)
        {
            var result = new T[rows, columns];
            for (int i = 0; i<rows; i++)
            {
                for (int j = 0; j<columns; j++)
                {
                    result[i, j]=value;
                }
            }
            return result;
        }
        public static T[,] CreateMatrix<T>(int rows, int columns, Func<int, int, string> initializer)
            where T : IParsable<T>
        {
            if (initializer==null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }
            var result = new T[rows, columns];
            for (int i = 0; i<rows; i++)
            {
                for (int j = 0; j<columns; j++)
                {
                    result[i, j]=T.Parse(initializer(i, j), CultureInfo.CurrentCulture.NumberFormat);
                }
            }
            return result;
        }
        public static T[,] CreateMatrix<T>(int rows, int columns, Func<int, int, T> initializer)
        {
            if (initializer==null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }
            var result = new T[rows,columns];
            for (int i = 0; i<rows; i++)
            {
                for (int j = 0; j<columns; j++)
                {
                    result[i, j]=initializer(i, j);
                }
            }
            return result;
        }
        public static T[][] CreateJagged<T>(int rows, int columns) where T : IAdditiveIdentity<T, T>
        {
            var result = new T[rows][];
            for (int i = 0; i<rows; i++)
            {
                var row = new T[columns];
                System.Array.Fill(row, T.AdditiveIdentity);
                result[i]=row;
            }
            return result;
        }
        public static T[][] CreateJagged<T>(int rows, int columns, T value)
        {
            var result = new T[rows][];
            for (int i = 0; i<rows; i++)
            {
                var row = new T[columns];
                System.Array.Fill(row, value);
                result[i]=row;
            }
            return result;
        }
        public static T[][] CreateJagged<T>(int rows, int columns, Func<int, int, string> initializer)
            where T : IParsable<T>
        {
            if (initializer==null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }
            var result = new T[rows][];
            for (int i = 0; i<rows; i++)
            {
                var row = new T[columns];
                for (int j = 0; j<row.Length; j++)
                {
                    row[j]=T.Parse(initializer(i, j), null);
                }
                result[i]=row;
            }
            return result;
        }
        public static T[][] CreateJagged<T>(int rows, int columns, Func<int, int, T> initializer)
        {
            if (initializer==null)
            {
                throw new ArgumentNullException(nameof(initializer));
            }
            var result = new T[rows][];
            for (int i = 0; i<rows; i++)
            {
                var row = new T[columns];
                for (int j = 0; j<columns; j++)
                {
                    row[j] = initializer(i, j);
                }
                result[i]=row;
            }
            return result;
        }

        public static T[][] ZerosJegged<T>(int rows, int columns)
            where T : IAdditiveIdentity<T, T>
            => CreateJagged<T>(rows, columns);
        public static T[][] IdentityJagged<T>(int size)
            where T : IAdditiveIdentity<T, T>, IMultiplicativeIdentity<T, T>
            => DiagonalJagged(size, T.MultiplicativeIdentity);
        public static T[][] DiagonalJagged<T>(int size, T value)
            where T : IAdditiveIdentity<T, T>
        {
            //return CreateJagged(size, size, (i, j) => i==j ? value : T.AdditiveIdentity);
            var result = CreateJagged<T>(size,size, T.AdditiveIdentity);
            for (int i = 0; i<size; i++)
            {
                result[i][i]=value;
            }
            return result;
        }

        public static T[][] DiagonalJagged<T>(params T[] diagonals)
            where T : IAdditiveIdentity<T, T>
            => CreateJagged(diagonals.Length, diagonals.Length, (i, j) => i==j ? diagonals[i] : T.AdditiveIdentity);
        public static T[,] ZerosMatrix<T>(int rows, int columns)
            where T : IAdditiveIdentity<T, T>
            => CreateMatrix<T>(rows, columns);
        public static T[,] IdentityMatrix<T>(int size)
            where T : IAdditiveIdentity<T, T>, IMultiplicativeIdentity<T, T>
            => DiagonalMatrix(size, T.MultiplicativeIdentity);
        public static T[,] DiagonalMatrix<T>(int size, T value)
            where T : IAdditiveIdentity<T, T>
            => CreateMatrix(size, size, (i, j) => i==j ? value : T.AdditiveIdentity);
        public static T[,] DiagonalMatrix<T>(params T[] diagonals)
            where T : IAdditiveIdentity<T, T>
            => CreateMatrix(diagonals.Length, diagonals.Length, (i, j) => i==j ? diagonals[i] : T.AdditiveIdentity);

    }

}
