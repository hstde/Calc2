using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace CalcLang.Interpreter
{
    public class OperatorInfo
    {
        public string Symbol;
        public ExpressionType ExpressionType;
        public int Precedence;
        public Associativity Associativity;
    }

    [Serializable]
    public class OperatorInfoDictionary : Dictionary<string, OperatorInfo>
    {
        public void Add(string symbol, ExpressionType expressionType, int precedence, Associativity associativity = Associativity.Left)
        {
            var info = new OperatorInfo()
            {
                Symbol = symbol,
                ExpressionType = expressionType,
                Precedence = precedence,
                Associativity = associativity
            };
            this[symbol] = info;
        }
    }

    public class OperatorHandler
    {
        private readonly OperatorInfoDictionary registeredOperators;

        public OperatorHandler()
        {
            registeredOperators = new OperatorInfoDictionary();
            BuildDefaultOperatorMappings();
        }

        public ExpressionType GetOperatorExpressionType(string symbol)
        {
            OperatorInfo opInfo;
            if (registeredOperators.TryGetValue(symbol, out opInfo))
                return opInfo.ExpressionType;
            return (ExpressionType)(-1);
        }

        public ExpressionType GetUnaryOperatorExpressionType(string symbol)
        {
            switch(symbol.ToLowerInvariant())
            {
                case "+": return ExpressionType.UnaryPlus;
                case "-": return ExpressionType.Negate;
                case "!": return ExpressionType.Not;
                case "~": return ExpressionType.Not;
                default:
                    return (ExpressionType)(-1);
            }
        }

        public virtual ExpressionType GetBinaryOperatorForAugmented(ExpressionType augmented)
        {
            switch (augmented)
            {
                case ExpressionType.AddAssign:
                case ExpressionType.AddAssignChecked:
                    return ExpressionType.AddChecked;
                case ExpressionType.AndAssign:
                    return ExpressionType.And;
                case ExpressionType.Decrement:
                    return ExpressionType.SubtractChecked;
                case ExpressionType.DivideAssign:
                    return ExpressionType.Divide;
                case ExpressionType.ExclusiveOrAssign:
                    return ExpressionType.ExclusiveOr;
                case ExpressionType.LeftShiftAssign:
                    return ExpressionType.LeftShift;
                case ExpressionType.ModuloAssign:
                    return ExpressionType.Modulo;
                case ExpressionType.MultiplyAssign:
                case ExpressionType.MultiplyAssignChecked:
                    return ExpressionType.MultiplyChecked;
                case ExpressionType.OrAssign:
                    return ExpressionType.Or;
                case ExpressionType.RightShiftAssign:
                    return ExpressionType.RightShift;
                case ExpressionType.SubtractAssign:
                case ExpressionType.SubtractAssignChecked:
                    return ExpressionType.SubtractChecked;
                case ExpressionType.PowerAssign:
                    return ExpressionType.Power;
                default:
                    return (ExpressionType)(-1);
            }
        }

        private OperatorInfoDictionary BuildDefaultOperatorMappings()
        {
            var dict = registeredOperators;
            dict.Clear();
            int p = 0; //precedence

            p += 10;
            dict.Add("=", ExpressionType.Assign, p);
            dict.Add("+=", ExpressionType.AddAssignChecked, p);
            dict.Add("-=", ExpressionType.SubtractAssignChecked, p);
            dict.Add("*=", ExpressionType.MultiplyAssignChecked, p);
            dict.Add("/=", ExpressionType.DivideAssign, p);
            dict.Add("%=", ExpressionType.ModuloAssign, p);
            dict.Add("|=", ExpressionType.OrAssign, p);
            dict.Add("&=", ExpressionType.AndAssign, p);
            dict.Add("^=", ExpressionType.ExclusiveOrAssign, p);
            dict.Add("<<=", ExpressionType.LeftShiftAssign, p);
            dict.Add(">>=", ExpressionType.RightShiftAssign, p);
            dict.Add("**=", ExpressionType.PowerAssign, p);

            p += 10;
            dict.Add("==", ExpressionType.Equal, p);
            dict.Add("!=", ExpressionType.NotEqual, p);
            dict.Add("<>", ExpressionType.NotEqual, p);

            p += 10;
            dict.Add("<", ExpressionType.LessThan, p);
            dict.Add("<=", ExpressionType.LessThanOrEqual, p);
            dict.Add(">", ExpressionType.GreaterThan, p);
            dict.Add(">=", ExpressionType.GreaterThanOrEqual, p);

            p += 10;
            dict.Add("|", ExpressionType.Or, p);
            dict.Add("||", ExpressionType.OrElse, p);
            dict.Add("^", ExpressionType.ExclusiveOr, p);

            p += 10;
            dict.Add("&", ExpressionType.And, p);
            dict.Add("&&", ExpressionType.AndAlso, p);

            p += 10;
            dict.Add("!", ExpressionType.Not, p);

            p += 10;
            dict.Add("<<", ExpressionType.LeftShift, p);
            dict.Add(">>", ExpressionType.RightShift, p);

            p += 10;
            dict.Add("+", ExpressionType.AddChecked, p);
            dict.Add("-", ExpressionType.SubtractChecked, p);

            p += 10;
            dict.Add("*", ExpressionType.MultiplyChecked, p);
            dict.Add("/", ExpressionType.Divide, p);
            dict.Add("%", ExpressionType.Modulo, p);
            dict.Add("**", ExpressionType.Power, p);

            p += 10;
            dict.Add("??", ExpressionType.Coalesce, p);
            dict.Add("?", ExpressionType.Conditional, p);

            return dict;
        }
    }
}
