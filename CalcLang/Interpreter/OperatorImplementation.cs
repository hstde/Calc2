using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public class OperatorImplementation
    {
        public readonly OperatorDispatchKey Key;
        public readonly Type CommonType;
        public readonly BinaryOperatorMethod BaseBinaryMethod;

        internal UnaryOperatorMethod Arg1Converter;
        internal UnaryOperatorMethod Arg2Converter;
        internal UnaryOperatorMethod ResultConverter;

        public BinaryOperatorMethod EvaluateBinary;
        public OperatorImplementation OverflowHandler;


        public OperatorImplementation(OperatorDispatchKey key, Type resultType, BinaryOperatorMethod baseBinaryMethod,
            UnaryOperatorMethod arg1Converter, UnaryOperatorMethod arg2Converter, UnaryOperatorMethod resultConverter)
        {
            Key = key;
            CommonType = resultType;
            Arg1Converter = arg1Converter;
            Arg2Converter = arg2Converter;
            ResultConverter = resultConverter;
            BaseBinaryMethod = baseBinaryMethod;
            SetupEvaluationMethod();
        }

        public OperatorImplementation(OperatorDispatchKey key, Type type, UnaryOperatorMethod method)
        {
            Key = key;
            CommonType = type;
            Arg1Converter = method;
            Arg2Converter = null;
            ResultConverter = null;
            BaseBinaryMethod = null;
        }

        public override string ToString() => "[OpImpl for " + Key.ToString() + "]";

        public void SetupEvaluationMethod()
        {
            if (BaseBinaryMethod == null)
                return;
            // Binary operator
            if (ResultConverter == null)
            {
                //without ResultConverter
                if (Arg1Converter == null && Arg2Converter == null)
                    EvaluateBinary = EvaluateConvNone;
                else if (Arg1Converter != null && Arg2Converter == null)
                    EvaluateBinary = EvaluateConvLeft;
                else if (Arg1Converter == null && Arg2Converter != null)
                    EvaluateBinary = EvaluateConvRight;
                else // if (Arg1Converter != null && arg2Converter != null)
                    EvaluateBinary = EvaluateConvBoth;
            }
            else
            {
                //with result converter
                if (Arg1Converter == null && Arg2Converter == null)
                    EvaluateBinary = EvaluateConvNoneConvResult;
                else if (Arg1Converter != null && Arg2Converter == null)
                    EvaluateBinary = EvaluateConvLeftConvResult;
                else if (Arg1Converter == null && Arg2Converter != null)
                    EvaluateBinary = EvaluateConvRightConvResult;
                else // if (Arg1Converter != null && Arg2Converter != null)
                    EvaluateBinary = EvaluateConvBothConvResult;
            }
        }

        private object EvaluateConvNone(object arg1, object arg2) => BaseBinaryMethod(arg1, arg2);
        private object EvaluateConvLeft(object arg1, object arg2) => BaseBinaryMethod(Arg1Converter(arg1), arg2);
        private object EvaluateConvRight(object arg1, object arg2) => BaseBinaryMethod(arg1, Arg2Converter(arg2));
        private object EvaluateConvBoth(object arg1, object arg2) => BaseBinaryMethod(Arg1Converter(arg1), Arg2Converter(arg2));

        private object EvaluateConvNoneConvResult(object arg1, object arg2) => ResultConverter(BaseBinaryMethod(arg1, arg2));
        private object EvaluateConvLeftConvResult(object arg1, object arg2) => ResultConverter(BaseBinaryMethod(Arg1Converter(arg1), arg2));
        private object EvaluateConvRightConvResult(object arg1, object arg2) => ResultConverter(BaseBinaryMethod(arg1, Arg2Converter(arg2)));
        private object EvaluateConvBothConvResult(object arg1, object arg2) => ResultConverter(BaseBinaryMethod(Arg1Converter(arg1), Arg2Converter(arg2)));
    }
}
