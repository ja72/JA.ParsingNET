using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JA.LinearAlgebra
{
    public static class StringExtensions
    {
        public static string ToListString<T>(this IEnumerable<T> vector, string formatting, string label = null, string startDelimiter = "[", string endDelimiter = "]", string separator = ",")
            where T : IFormattable
        {
            return ToListString(vector.Select(item=> item.ToString(formatting,null)), label, startDelimiter, endDelimiter, separator);
        }
        public static string ToListString<T>(this IEnumerable<T> vector, Func<T, string> convert, string label = null, string startDelimiter = "[", string endDelimiter = "]", string separator = ",")
        {
            return ToListString(vector.Select(convert), label, startDelimiter, endDelimiter, separator);
        }
        public static string ToListString(this IEnumerable<string> vector, string label = null, string startDelimiter = "[", string endDelimiter = "]", string separator = ",")
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(label))
            {
                sb.AppendLine(label);
            }
            sb.Append(startDelimiter);
            sb.Append(string.Join(separator, vector));
            sb.Append(endDelimiter);
            return sb.ToString();
        }
        public static string ToTableVector<T>(this T[] vector, string formatting, string label = null)
            where T : IFormattable
        {
            string[] lines = new string[vector.Length];
            int width = 0;
            for (int i = 0; i<lines.Length; i++)
            {
                lines[i]=vector[i].ToString(formatting, null);
                width=Math.Max(width, lines[i].Length);
            }
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(label))
            {
                sb.AppendLine(label);
            }
            for (int i = 0; i<lines.Length; i++)
            {
                sb.Append("| ");
                sb.Append(lines[i].PadLeft(width));
                sb.Append(" |");
                sb.AppendLine();
            }
            return sb.ToString();
        }
        public static string ToTableMatrix<T>(this T[][] matrix, string formatting, string label = null)
            where T : IFormattable
        {
            int n = matrix.Length;
            int m = matrix.Max((row)=>row.Length);
            string[][] lines = new string[n][];
            int[] widths = new int[m];
            for (int i = 0; i<n; i++)
            {
                var vector = matrix[i];
                var row = new string[m];
                for (int j = 0; j<m; j++)
                {
                    if (j<vector.Length)
                    {
                        row[j]=vector[j].ToString(formatting, null);
                    }
                    else
                    {
                        row[j]=string.Empty;
                    }
                    widths[j]=Math.Max(widths[j], row[j].Length);
                }
                lines[i]=row;
            }
            return ToTableMatrix(lines, widths, label);
        }
        public static string ToTableMatrix<T>(this T[][] matrix, int[] widths, string formatting, string label = null)
            where T : IFormattable
        {
            int n = matrix.Length;
            int m = matrix.Max((row)=>row.Length);
            string[][] lines = new string[n][];
            for (int i = 0; i<n; i++)
            {
                var vector = matrix[i];
                var row = new string[m];
                for (int j = 0; j<m; j++)
                {
                    if (j<vector.Length)
                    {
                        row[j]=vector[j].ToString(formatting, null);
                    }
                    else
                    {
                        row[j]=string.Empty;
                    }
                }
                lines[i]=row;
            }
            return ToTableMatrix(lines, widths, label);
        }
        public static string ToTableMatrix<T>(this T[][] matrix, int[] widths, Func<T, string> convert, string label = null)
        {
            int n = matrix.Length;
            int m = matrix.Max((row)=>row.Length);
            string[][] lines = new string[n][];
            for (int i = 0; i<n; i++)
            {
                var vector = matrix[i];
                var row = new string[m];
                for (int j = 0; j<m; j++)
                {
                    if (j<vector.Length)
                    {
                        row[j]= convert(vector[j]);
                    }
                    else
                    {
                        row[j]=string.Empty;
                    }
                }
                lines[i]=row;
            }
            return ToTableMatrix(lines, widths, label);
        }
        public static string ToTableMatrix(this string[][] lines, int[] widths, string label = null)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(label))
            {
                sb.AppendLine(label);
            }
            for (int i = 0; i<lines.Length; i++)
            {
                var row = lines[i];
                sb.ToTableRowInternal(row, widths);
                sb.AppendLine();
            }
            return sb.ToString();
        }
        public static string ToTableRow<T>(this IEnumerable<T> values, int[] width, string formatting, string columnSeparator = "|")
            where T: IFormattable
        {            
            StringBuilder sb = new StringBuilder();
            var row = values.Select((item)=>item.ToString(formatting,null));
            sb.ToTableRowInternal(row, width, columnSeparator);
            return sb.ToString();
        }
        public static string ToTableRow<T>(this IEnumerable<T> values, int[] width, Func<T, string> convert, string columnSeparator = "|")
        {            
            var row = values.Select( convert ).ToArray();
            return ToTableRow(row, width, columnSeparator);
        }
        public static string ToTableRow(this IEnumerable<string> row, int[] width, string columnSeparator = "|")
        {            
            StringBuilder sb = new StringBuilder();
            sb.ToTableRowInternal(row, width, columnSeparator);
            return sb.ToString();
        }
        static void ToTableRowInternal(this StringBuilder sb, IEnumerable<string> row, int[] width, string columnSeparator = "|")
        {
            int index = 0;
            sb.Append(columnSeparator);
            foreach (var item in row)
            {
                index = index % width.Length;
                sb.Append(" ");
                sb.Append(item.PadLeft(width[index]));
                sb.Append(" ");
                sb.Append(columnSeparator);
                index++;
            }
        }
    }

}
