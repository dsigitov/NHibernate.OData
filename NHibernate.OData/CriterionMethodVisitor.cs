using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;

namespace NHibernate.OData
{
    internal class CriterionMethodVisitor : QueryMethodVisitorBase<ICriterion>
    {
        private readonly ODataParserConfiguration _configuration;

        private CriterionMethodVisitor(ODataParserConfiguration configuration)
        {
            Require.NotNull(configuration, "configuration");

            _configuration = configuration;
        }

        public static ICriterion CreateCriterion(Method method, Expression[] arguments, ODataParserConfiguration configuration)
        {
            return method.Visit(new CriterionMethodVisitor(configuration), arguments);
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

            ICriterion customCriterion = null;

            if (_configuration.CustomCriterionBuilder != null)
                customCriterion = _configuration.CustomCriterionBuilder.Like(
                    ProjectionVisitor.CreateProjection(arguments[1]),
                    LiteralUtil.CoerceString(((LiteralExpression)arguments[0])),
                    MatchMode.Anywhere
                );

            if (customCriterion != null)
                return customCriterion;

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
