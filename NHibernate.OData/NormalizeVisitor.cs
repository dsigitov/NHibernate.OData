using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System;

namespace NHibernate.OData
{
    internal class NormalizeVisitor : IVisitor<Expression>
    {
        public Expression LiteralExpression(LiteralExpression expression)
        {
            return expression;
        }

        public virtual Expression MemberExpression(MemberExpression expression)
        {
            return expression;
        }

        public Expression ParenExpression(ParenExpression expression)
        {
            return expression.Expression.Visit(this);
        }

        public Expression BoolUnaryExpression(BoolUnaryExpression expression)
        {
            var result = expression.Expression.Visit(this);

            var literal = result as LiteralExpression;

            if (literal != null)
                return ResolveBoolUnaryLiteral(expression, literal);

            return new BoolUnaryExpression(expression.Operator, result);
        }


        private Expression ResolveBoolUnaryLiteral(BoolUnaryExpression expression, LiteralExpression literal)
        {
            if (expression.Operator != Operator.Not)
                throw new NotSupportedException();

            // CoerceBoolExpression takes care of the type of the literal.

            Debug.Assert(literal.LiteralType == LiteralType.Boolean);

            return new LiteralExpression(!(bool)literal.Value, LiteralType.Boolean);
        }

        public Expression ArithmeticUnaryExpression(ArithmeticUnaryExpression expression)
        {
            var result = expression.Expression.Visit(this);

            var literal = result as LiteralExpression;

            if (literal != null)
                return ResolveArithmeticUnaryLiteral(expression, literal);

            return new ArithmeticUnaryExpression(Operator.Negative, result);
        }

        private Expression ResolveArithmeticUnaryLiteral(ArithmeticUnaryExpression expression, LiteralExpression literal)
        {
            if (expression.Operator != Operator.Negative)
                throw new NotSupportedException();

            object value = literal.Value;

            switch (literal.LiteralType)
            {
                case LiteralType.Decimal:
                    value = -((decimal)value);
                    break;

                case LiteralType.Double:
                    value = -((double)value);
                    break;

                case LiteralType.Duration:
                    value = -((XmlTimeSpan)value);
                    break;

                case LiteralType.Int:
                    value = -((int)value);
                    break;

                case LiteralType.Long:
                    value = -((long)value);
                    break;

                case LiteralType.Single:
                    value = -((float)value);
                    break;

                default:
                    throw new ODataException(String.Format(
                        ErrorMessages.Expression_CannotNegate, literal.LiteralType
                        ));
            }

            return new LiteralExpression(value, literal.LiteralType);
        }

        public Expression LogicalExpression(LogicalExpression expression)
        {
            var left = expression.Left.Visit(this);
            var right = expression.Right.Visit(this);

            var leftLiteral = left as LiteralExpression;
            var rightLiteral = right as LiteralExpression;

            if (leftLiteral != null && rightLiteral != null)
                return NormalizeLogicalLiterals(expression, leftLiteral, rightLiteral);

            var anyLiteral = leftLiteral ?? rightLiteral;

            if (anyLiteral != null)
            {
                if (expression.Operator == Operator.And && (bool)anyLiteral.Value == false)
                    return new LiteralExpression(false, LiteralType.Boolean);
                
                if (expression.Operator == Operator.Or && (bool)anyLiteral.Value == true)
                    return new LiteralExpression(true, LiteralType.Boolean);

                return ReferenceEquals(anyLiteral, leftLiteral) ? right : left;
            }

            return new LogicalExpression(expression.Operator, left, right);
        }

        private Expression NormalizeLogicalLiterals(LogicalExpression expression, LiteralExpression left, LiteralExpression right)
        {
            // These are verified already using CoerceBoolExpression.

            Debug.Assert(left.LiteralType == LiteralType.Boolean);
            Debug.Assert(right.LiteralType == LiteralType.Boolean);

            bool value;

            switch (expression.Operator)
            {
                case Operator.And:
                    value = (bool)left.Value && (bool)right.Value;
                    break;

                case Operator.Or:
                    value = (bool)left.Value || (bool)right.Value;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new LiteralExpression(value, LiteralType.Boolean);
        }

        public Expression ComparisonExpression(ComparisonExpression expression)
        {
            var left = expression.Left.Visit(this);
            var right = expression.Right.Visit(this);

            var leftLiteral = left as LiteralExpression;
            var rightLiteral = right as LiteralExpression;

            if (leftLiteral != null && rightLiteral != null)
                return ResolveComparisonLiterals(expression, leftLiteral, rightLiteral);

            return new ComparisonExpression(expression.Operator, left, right);
        }

        private Expression ResolveComparisonLiterals(ComparisonExpression expression, LiteralExpression leftLiteral, LiteralExpression rightLiteral)
        {
            object left = leftLiteral.Value;
            object right = rightLiteral.Value;
            bool result;

            var type = LiteralUtil.CoerceLiteralValues(ref left, leftLiteral.LiteralType, ref right, rightLiteral.LiteralType);

            switch (expression.Operator)
            {
                case Operator.Eq:
                    result = ResolveEquals(left, right, type);
                    break;

                case Operator.Ne:
                    result = !ResolveEquals(left, right, type);
                    break;

                case Operator.Gt:
                    result = ResolveCompare(left, right, type) > 0;
                    break;

                case Operator.Ge:
                    result = ResolveCompare(left, right, type) >= 0;
                    break;

                case Operator.Lt:
                    result = ResolveCompare(left, right, type) < 0;
                    break;

                case Operator.Le:
                    result = ResolveCompare(left, right, type) <= 0;
                    break;

                default:
                    throw new NotSupportedException();
            }

            return new LiteralExpression(result, LiteralType.Boolean);
        }

        private int ResolveCompare(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Binary:
                case LiteralType.Guid:
                case LiteralType.Null:
                    throw new ODataException(String.Format(
                        ErrorMessages.Expression_CannotCompareTypes, type
                        ));

                default:
                    var comparable = left as IComparable;

                    Debug.Assert(comparable != null);

                    return comparable.CompareTo(right);
            }
        }

        private bool ResolveEquals(object left, object right, LiteralType type)
        {
            if (type == LiteralType.Binary)
                return LiteralUtil.ByteArrayEquals((byte[])left, (byte[])right);
            else
                return Equals(left, right);
        }

        public Expression ArithmeticExpression(ArithmeticExpression expression)
        {
            var left = expression.Left.Visit(this);
            var right = expression.Right.Visit(this);

            var leftLiteral = left as LiteralExpression;
            var rightLiteral = right as LiteralExpression;

            if (leftLiteral != null && rightLiteral != null)
                return ResolveArithmeticLiterals(expression, leftLiteral, rightLiteral);

            return new ArithmeticExpression(expression.Operator, left, right);
        }

        private Expression ResolveArithmeticLiterals(ArithmeticExpression expression, LiteralExpression leftLiteral, LiteralExpression rightLiteral)
        {
            object left = leftLiteral.Value;
            object right = rightLiteral.Value;
            object result;

            var type = LiteralUtil.CoerceLiteralValues(ref left, leftLiteral.LiteralType, ref right, rightLiteral.LiteralType);

            switch (expression.Operator)
            {
                case Operator.Add: result = ResolveAdd(left, right, type); break;
                case Operator.Div: result = ResolveDiv(left, right, type); break;
                case Operator.Mod: result = ResolveMod(left, right, type); break;
                case Operator.Mul: result = ResolveMul(left, right, type); break;
                case Operator.Sub: result = ResolveSub(left, right, type); break;

                default:
                    throw new NotSupportedException();
            }

            if (result == null)
            {
                throw new ODataException(String.Format(
                    ErrorMessages.Expression_IncompatibleTypes,
                    expression.Operator,
                    type
                    ));
            }

            return new LiteralExpression(result, type);
        }

        private object ResolveAdd(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left + (decimal)right;
                case LiteralType.Double: return (double)left + (double)right;
                    // case LiteralType.Duration: return (XmlTimeSpan)left + (XmlTimeSpan)right;
                case LiteralType.Int: return (int)left + (int)right;
                case LiteralType.Long: return (long)left + (long)right;
                case LiteralType.Single: return (float)left + (float)right;
                case LiteralType.String: return (string)left + (string)right;
                default: return null;
            }
        }

        private object ResolveSub(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left - (decimal)right;
                case LiteralType.Double: return (double)left - (double)right;
                    // case LiteralType.Duration: return (XmlTimeSpan)left - (XmlTimeSpan)right;
                case LiteralType.Int: return (int)left - (int)right;
                case LiteralType.Long: return (long)left - (long)right;
                case LiteralType.Single: return (float)left - (float)right;
                default: return null;
            }
        }

        private object ResolveMul(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left * (decimal)right;
                case LiteralType.Double: return (double)left * (double)right;
                case LiteralType.Int: return (int)left * (int)right;
                case LiteralType.Long: return (long)left * (long)right;
                case LiteralType.Single: return (float)left * (float)right;
                default: return null;
            }
        }

        private object ResolveDiv(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left / (decimal)right;
                case LiteralType.Double: return (double)left / (double)right;
                case LiteralType.Int: return (int)left / (int)right;
                case LiteralType.Long: return (long)left / (long)right;
                case LiteralType.Single: return (float)left / (float)right;
                default: return null;
            }
        }

        private object ResolveMod(object left, object right, LiteralType type)
        {
            switch (type)
            {
                case LiteralType.Decimal: return (decimal)left % (decimal)right;
                case LiteralType.Double: return (double)left % (double)right;
                case LiteralType.Int: return (int)left % (int)right;
                case LiteralType.Long: return (long)left % (long)right;
                case LiteralType.Single: return (float)left % (float)right;
                default: return null;
            }
        }

        public Expression MethodCallExpression(MethodCallExpression expression)
        {
            // There are no methods with zero argument count.

            Debug.Assert(expression.Arguments.Length > 0);

            var arguments = new Expression[expression.Arguments.Length];

            bool allLiterals = true;

            for (int i = 0; i < expression.Arguments.Length; i++)
            {
                var normalized = expression.Arguments[i].Visit(this);

                allLiterals = allLiterals && normalized is LiteralExpression;

                arguments[i] = normalized;
            }

            if (allLiterals)
            {
                var literalArguments = new LiteralExpression[arguments.Length];

                for (int i = 0; i < arguments.Length; i++)
                {
                    literalArguments[i] = (LiteralExpression)arguments[i];
                }

                Expression normalizedExpression = NormalizeMethodVisitor.Normalize(expression.Method, literalArguments);
                if (normalizedExpression != null)
                    return normalizedExpression;
            }

            return new MethodCallExpression(expression.MethodCallType, expression.Method, arguments);
        }

        public Expression ResolvedMemberExpression(ResolvedMemberExpression expression)
        {
            throw new InvalidOperationException();
        }

        public Expression CustomResolvedMemberExpression(CustomResolvedMemberExpression expression)
        {
            throw new InvalidOperationException();
        }

        public Expression LambdaExpression(LambdaExpression expression)
        {
            return expression;
        }
    }
}