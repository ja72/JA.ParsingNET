﻿using System;
using System.Collections.Generic;

namespace JA.Expressions.Parsing
{
    //public interface IParsable<T> where T : IParsable<T>
    //{
    //    abstract static T Parse(string description);
    //}

    internal class ExprParser(Tokenizer tokenizer)
    {
        readonly Tokenizer tokenizer=tokenizer;

        public bool TryParse(out Expr expr)
        {
            try
            {
                expr=ParseExpression();
                return true;
            }
            catch (SyntaxException)
            {
                expr=default(Expr);
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // Parse an entire expression and check EOF was reached
        public Expr ParseExpression()
        {
            // For the moment, all we understand is add and subtract
            var expr = ParseArray();

            // Check everything was consumed
            if (tokenizer.Current.Token!=Token.EOF)
            {
                throw new SyntaxException("Unexpected characters at end of expression");
            }

            return expr;
        }

        Expr ParseArray()
        {
            var token = tokenizer.Current.Token;
            switch (token)
            {
                case Token.OpenBracket:
                // Skip '['
                tokenizer.MoveNext();

                // Parse arguments
                var arguments = new List<Expr>();
                while (true)
                {
                    var lhs = ParseAssignment();
                    // Parse argument and add to list
                    // TODO: Parse arrays as elements first
                    arguments.Add(lhs);

                    // Is there another argument?
                    if (tokenizer.Current.Token==Token.Comma)
                    {
                        tokenizer.MoveNext();
                        continue;
                    }

                    // Get out
                    break;
                }

                // Check and skip ')'
                if (tokenizer.Current.Token!=Token.CloseBracket)
                {
                    throw new SyntaxException("Missing close bracket");
                }
                tokenizer.MoveNext();

                // Create the vector expression
                if (arguments.Count==0)
                {
                    return Expr.Array([]);
                }
                // INFO: Do I want to cast an array of one as a scalar?
                // if (arguments.Count==1) return arguments[0];
                return Expr.Array(arguments);
            }
            return ParseAssignment();
        }

        Expr ParseAssignment()
        {
            var lhs = ParseAddSubtract();

            var token = tokenizer.Current.Token;

            if (token==Token.Assign)
            {
                var op = token.GetOperand();
                // Skip the operator
                tokenizer.MoveNext();

                // Parse the right hand side of the expression
                var rhs = ParseAddSubtract();

                return Expr.Assign(lhs, rhs);
            }

            return lhs;
        }

        // Parse a sequence of add/subtract operators
        Expr ParseAddSubtract()
        {
            // Parse the left hand side
            var lhs = ParseMultiplyDivide();

            while (true)
            {
                var token = tokenizer.Current.Token;
                switch (token)
                {
                    case Token.Add:
                    case Token.Subtract:
                    var op = token.GetOperand();
                    // Skip the operator
                    tokenizer.MoveNext();

                    // Parse the right hand side of the expression
                    var rhs = ParseMultiplyDivide();

                    // Create a binary node and use it as the left-hand side from now on
                    lhs=Expr.Binary(op, lhs, rhs);
                    break;
                    default:
                    return lhs;
                }
            }
        }

        // Parse a sequence of multiply/divide operators
        Expr ParseMultiplyDivide()
        {
            // Parse the left hand side
            var lhs = ParsePower();

            while (true)
            {
                var token = tokenizer.Current.Token;
                switch (token)
                {
                    case Token.Multiply:
                    case Token.Divide:
                    {
                        var op = token.GetOperand();
                        // Skip the operator
                        tokenizer.MoveNext();

                        // Parse the right hand side of the expression
                        var rhs = ParsePower();
                        // Create a binary node and use it as the left-hand side from now on
                        lhs=Expr.Binary(op, lhs, rhs);
                    }
                    break;
                    default:
                    return lhs;
                }
            }
        }
        Expr ParsePower()
        {
            // Parse the left hand side
            var lhs = ParseUnary();

            while (true)
            {
                var token = tokenizer.Current.Token;
                switch (token)
                {
                    case Token.Caret:
                    {
                        var op = token.GetOperand();
                        // Skip the operator
                        tokenizer.MoveNext();

                        // Parse the right hand side of the expression
                        var rhs = ParseUnary();
                        // Create a binary node and use it as the left-hand side from now on
                        lhs=Expr.Binary(op, lhs, rhs);
                    }
                    break;
                    default:
                    return lhs;
                }
            }
        }

        // Parse a unary operator (eg: negative/positive)
        Expr ParseUnary()
        {
            while (true)
            {
                var token = tokenizer.Current.Token;
                var op = token.GetOperand();
                switch (token)
                {
                    case Token.Add:
                    tokenizer.MoveNext();
                    continue;
                    case Token.Subtract:
                    // Skip
                    tokenizer.MoveNext();

                    // Parse RHS 
                    // Note this recurses to self to support negative of a negative
                    var rhs = ParseUnary();

                    // Create unary node
                    return Expr.Negate(rhs);
                    default:
                    return ParseLeaf();
                }
            }
        }

        // Parse a leaf node
        // (For the moment this is just a number)
        Expr ParseLeaf()
        {
            Expr node;
            var token = tokenizer.Current.Token;
            switch (token)
            {
                case Token.OpenParens:      // Parenthesis?
                                            // Skip '('
                tokenizer.MoveNext();

                // Parse a top-level expression
                node=ParseAddSubtract();

                // Check and skip ')'
                if (tokenizer.Current.Token!=Token.CloseParens)
                {
                    throw new SyntaxException("Missing close parenthesis");
                }

                tokenizer.MoveNext();

                // Return
                return node;
                case Token.Identifier:      // Variable or Function
                                            // Capture the name and skip it
                var name = tokenizer.Current.Identifier;
                tokenizer.MoveNext();

                return ParseIdentifier(name);
                case Token.Number:          // Is it a number?
                node=Expr.Const(tokenizer.Current.Number);
                tokenizer.MoveNext();
                return node;

                case Token.OpenBracket:
                // Skip '['
                tokenizer.MoveNext();

                // Parse arguments
                var arguments = new List<Expr>();
                while (true)
                {
                    // Parse argument and add to list
                    // TODO: Parse arrays as elements first
                    arguments.Add(ParseAssignment());

                    // Is there another argument?
                    if (tokenizer.Current.Token==Token.Comma)
                    {
                        tokenizer.MoveNext();
                        continue;
                    }

                    // Get out
                    break;
                }

                // Check and skip ')'
                if (tokenizer.Current.Token!=Token.CloseBracket)
                {
                    throw new SyntaxException("Missing close bracket");
                }
                tokenizer.MoveNext();

                // Create the vector expression
                if (arguments.Count==0)
                {
                    return Expr.Array([]);
                }
                // INFO: Do I want to cast an array of one as a scalar?
                // if (arguments.Count==1) return arguments[0];
                return Expr.Array(arguments);

                default:
                throw new SyntaxException($"Unexpect token: {tokenizer.Current.Token}");
            }
        }

        public Expr ParseIdentifier(string name)
        {
            switch (tokenizer.Current.Token)
            {
                case Token.OpenParens:
                {
                    // Function call

                    // Skip parens
                    tokenizer.MoveNext();

                    // Parse arguments
                    var arguments = new List<Expr>();
                    while (true)
                    {
                        // Parse argument and add to list
                        arguments.Add(ParseAddSubtract());

                        // Is there another argument?
                        if (tokenizer.Current.Token==Token.Comma)
                        {
                            tokenizer.MoveNext();
                            continue;
                        }

                        // Get out
                        break;
                    }

                    // Check and skip ')'
                    if (tokenizer.Current.Token!=Token.CloseParens)
                    {
                        throw new SyntaxException("Missing close parenthesis");
                    }
                    tokenizer.MoveNext();

                    // catch special functions here
#pragma warning disable IDE0305 // Simplify collection initialization
                    return name switch
                    {
                        "sum" => Expr.Sum(arguments.ToArray()),
                        "dot" when arguments.Count==2 => Expr.Dot(arguments[0], arguments[1]),
                        "outer" when arguments.Count==2 => Expr.Outer(arguments[0], arguments[1]),
                        "hypot" when arguments.Count==2 => Expr.Hypot(arguments[0], arguments[1]),
                        "norm" => Expr.Norm(arguments.ToArray()),
                        "cross" when arguments.Count==2 => Expr.Cross(arguments[0], arguments[1]),
                        // Create the function call node
                        _ => arguments.Count switch
                        {
                            1 => Expr.Unary(name, arguments[0]),
                            2 => Expr.Binary(name, arguments[0], arguments[1]),
                            _ => throw new SyntaxException("Invalid number of arguments"),
                        },
                    };
#pragma warning restore IDE0305 // Simplify collection initialization
                }
                case Token.OpenBracket:
                {
                    // Array Element

                    // Skip bracket
                    tokenizer.MoveNext();

                    if (tokenizer.Current.Token!=Token.Number)
                    {
                        throw new SyntaxException("Expect numeric index in brackets.");
                    }

                    int index = (int)tokenizer.Current.Number;

                    // Check and skip ])'
                    if (tokenizer.Current.Token!=Token.CloseBracket)
                    {
                        throw new SyntaxException("Missing close bracket");
                    }
                    tokenizer.MoveNext();

                    throw new NotSupportedException("Array Elements not supported yet.");
                }
                default:
                {
                    // Variable
                    return Expr.ConstOrVariable(name);
                }
            }
        }
    }

    // Exception for syntax errors
    public class SyntaxException(string message) : Exception(message)
    {
    }
}