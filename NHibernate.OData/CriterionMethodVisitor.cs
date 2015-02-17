using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;

namespace NHibernate.OData
{
    internal class CriterionMethodVisitor : QueryMethodVisitorBase<ICriterion>
    {
        private static readonly CriterionMethodVisitor _instance = new CriterionMethodVisitor();

        private CriterionMethodVisitor()
        {
        }

        public static ICriterion CreateCriterion(Method method, Expression[] arguments)
        {
            return method.Visit(_instance, arguments);
        }

        public override ICriterion SubStringOfMethod(SubStringOfMethod method, Expression[] arguments)
        {
            if (arguments[0].Type != ExpressionType.Literal)
                return base.SubStringOfMethod(method, arguments);

            if (arguments[1].Type == ExpressionType.CustomResolvedMember)
            {
                return ((CustomResolvedMemberExpression)arguments[1]).CustomMemberExpression.CreateLikeCriterion(
                    LiteralUtil.CoerceString(((LiteralExpression)arguments[0])),
                    MatchMode.Anywhere
                );
            }

            return Restrictions.Like(
                ProjectionVisitor.CreateProjection(arguments[1]),
                LiteralUtil.CoerceString(((LiteralExpression)arguments[0])),
                MatchMode.Anywhere
            );
        }

        public override ICriterion StartsWithMethod(StartsWithMethod method, Expression[] arguments)
        {
            if (arguments[1].Type != ExpressionType.Literal)
                return base.StartsWithMethod(method, arguments);

            if (arguments[0].Type == ExpressionType.CustomResolvedMember)
            {
                return ((CustomResolvedMemberExpression)arguments[0]).CustomMemberExpression.CreateLikeCriterion(
                    LiteralUtil.CoerceString(((LiteralExpression)arguments[1])),
                    MatchMode.Start
                );
            }

            return Restrictions.Like(
                ProjectionVisitor.CreateProjection(arguments[0]),
                LiteralUtil.CoerceString(((LiteralExpression)arguments[1])),
                MatchMode.Start
            );
        }

        public override ICriterion EndsWithMethod(EndsWithMethod method, Expression[] arguments)
        {
            if (arguments[1].Type != ExpressionType.Literal)
                return base.EndsWithMethod(method, arguments);

            if (arguments[0].Type == ExpressionType.CustomResolvedMember)
            {
                return ((CustomResolvedMemberExpression)arguments[0]).CustomMemberExpression.CreateLikeCriterion(
                    LiteralUtil.CoerceString(((LiteralExpression)arguments[1])),
                    MatchMode.End
                );
            }

            return Restrictions.Like(
                ProjectionVisitor.CreateProjection(arguments[0]),
                LiteralUtil.CoerceString(((LiteralExpression)arguments[1])),
                MatchMode.End
            );
        }
    }
}
