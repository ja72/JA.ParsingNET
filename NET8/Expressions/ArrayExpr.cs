using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Xml;

using JA.LinearAlgebra;

namespace JA.Expressions
{
    public record ArrayExpr(Expr[] Elements) : Expr
    {
        public override int Rank { get; } = Elements.Max((item) => item.Rank)+1;
        public int Count { get; } = Elements.Length;
        public int Size { get; } = Elements.Length;

        #region Methods
        public override IQuantity Eval(params (string sym, double val)[] parameters)
        {
            return Rank switch
            {
                1 => (Vector)( Elements.Select(item => item.Eval(parameters).Value).ToArray() ),
                2 => (JaggedMatrix)( Elements.Select((item) => item.ToArray().Select(col => col.Eval(parameters).Value).ToArray()).ToArray() ),
                _ => throw new NotSupportedException("Ranks more than 2 are not supported"),
            };
        }

        protected internal override void Compile(ILGenerator generator, Dictionary<string, int> envirnoment)
        {
            if (Rank==1)
            {
                generator.Emit(OpCodes.Ldc_I4, Size);
                generator.Emit(OpCodes.Newarr, typeof(double));
                for (int i = 0; i<Elements.Length; i++)
                {
                    generator.Emit(OpCodes.Dup);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    Elements[i].Compile(generator, envirnoment);
                    generator.Emit(OpCodes.Stelem_R8);
                }
            }
            else if (Rank==2)
            {
                generator.Emit(OpCodes.Ldc_I4, Size);
                generator.Emit(OpCodes.Newarr, typeof(double[]));
                for (int i = 0; i<Elements.Length; i++)
                {
                    generator.Emit(OpCodes.Dup);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    Elements[i].Compile(generator, envirnoment);
                    generator.Emit(OpCodes.Stelem_Ref);
                }
            }
            else
            {
                throw new NotSupportedException("Ranks more than 2 are not supported");
            }
        }


        protected internal override Expr Substitute(Expr variable, Expr value)
        {
            return new ArrayExpr(Elements.Select((item) => item.Substitute(variable, value)).ToArray());
        }

        public override Expr PartialDerivative(VariableExpr symbol)
        {
            return new ArrayExpr(Elements.Select((item) => item.PartialDerivative(symbol)).ToArray());
        }

        protected internal override void FillSymbols(ref List<string> variables)
        {
            for (int i = 0; i<Elements.Length; i++)
            {
                Elements[i].FillSymbols(ref variables);
            }
        }
        protected internal override void FillValues(ref List<double> values)
        {
            for (int i = 0; i<Elements.Length; i++)
            {
                Elements[i].FillValues(ref values);
            }
        }

        #endregion

        #region Formatting
        public static bool ShowAsTable { get; set; } = false;
        public override string ToString(string formatting, IFormatProvider formatProvider)
        {
            if (ShowAsTable && Rank == 1)
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
                    sb.Append("| ");
                    sb.Append(row.PadLeft(width));
                    sb.Append(" |");
                    sb.AppendLine();
                }

                return sb.ToString();
            }
            else
            {
                return $"[{string.Join(",", Elements.Select(x=> x.ToString(formatting, formatProvider)))}]";
            }
        }
        #endregion

    }
}
