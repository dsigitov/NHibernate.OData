using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.OData.Extensions;
using NHibernate.Type;

namespace NHibernate.OData
{
    internal class ProjectionMethodVisitor : QueryMethodVisitorBase<IProjection>
    {
        private readonly CriterionBuildContext _context;

        public ProjectionMethodVisitor(CriterionBuildContext context)
        {
            Require.NotNull(context, "context");

            _context = context;
        }

        public IProjection CreateProjection(Method method, Expression[] arguments)
        {
            return method.Visit(this, arguments);
        }

        public override IProjection SubStringMethod(SubStringMethod method, Expression[] arguments)
        {
            if (arguments.Length == 2)
            {
                return new SqlFunctionProjection(
                    "substring",
                    NHibernateUtil.String,
                    _context.ProjectionVisitor.CreateProjection(arguments[0]),
                    _context.ProjectionVisitor.CreateProjection(arguments[1])
                );
            }
            else
            {
                return new SqlFunctionProjection(
                    "substring",
                    NHibernateUtil.String,
                    _context.ProjectionVisitor.CreateProjection(arguments[0]),
                    _context.ProjectionVisitor.CreateProjection(arguments[1]),
                    _context.ProjectionVisitor.CreateProjection(arguments[2])
                );
            }
        }

        public override IProjection ConcatMethod(ConcatMethod method, Expression[] arguments)
        {
            if (arguments.Length == 1)
            {
                return _context.ProjectionVisitor.CreateProjection(arguments[0]);
            }
            else
            {
                return new SqlFunctionProjection(
                    "concat",
                    NHibernateUtil.String,
                    _context.ProjectionVisitor.CreateProjection(arguments[0]),
                    _context.ProjectionVisitor.CreateProjection(arguments[1])
                );
            }
        }

        public override IProjection LengthMethod(LengthMethod method, Expression[] arguments)
        {
            return new SqlFunctionProjection(
                "length",
                NHibernateUtil.Int32,
                _context.ProjectionVisitor.CreateProjection(arguments[0])
            );
        }

        public override IProjection ReplaceMethod(ReplaceMethod method, Expression[] arguments)
        {
            return new SqlFunctionProjection(
                "replace",
                NHibernateUtil.String,
                _context.ProjectionVisitor.CreateProjection(arguments[0]),
                _context.ProjectionVisitor.CreateProjection(arguments[1]),
                _context.ProjectionVisitor.CreateProjection(arguments[2])
            );
        }

        public override IProjection ToUpperMethod(ToUpperMethod method, Expression[] arguments)
        {
            return new SqlFunctionProjection(
                "upper",
                NHibernateUtil.String,
                _context.ProjectionVisitor.CreateProjection(arguments[0])
            );
        }

        public override IProjection ToLowerMethod(ToLowerMethod method, Expression[] arguments)
        {
            return new SqlFunctionProjection(
                "lower",
                NHibernateUtil.String,
                _context.ProjectionVisitor.CreateProjection(arguments[0])
            );
        }

        public override IProjection TrimMethod(TrimMethod method, Expression[] arguments)
        {
            return new SqlFunctionProjection(
                "trim",
                NHibernateUtil.String,
                _context.ProjectionVisitor.CreateProjection(arguments[0])
            );
        }

        public override IProjection IndexOfMethod(IndexOfMethod method, Expression[] arguments)
        {
            // The standard registered locate function expects three parameters,
            // and the rest seems tot support it.

            return new SqlFunctionProjection(
                "locate",
                NHibernateUtil.Int32,
                _context.ProjectionVisitor.CreateProjection(arguments[1]),
                _context.ProjectionVisitor.CreateProjection(arguments[0]),
                Projections.Constant(1)
            );
        }

        public override IProjection CeilingMethod(CeilingMethod method, Expression[] arguments)
        {
            return new SqlFunctionProjection(
                "ceil",
                NHibernateUtil.Int32,
                _context.ProjectionVisitor.CreateProjection(arguments[0])
            );
        }

        public override IProjection FloorMethod(FloorMethod method, Expression[] arguments)
        {
            return new SqlFunctionProjection(
                "floor",
                NHibernateUtil.Int32,
                _context.ProjectionVisitor.CreateProjection(arguments[0])
            );
        }

        public override IProjection RoundMethod(RoundMethod method, Expression[] arguments)
        {
            return new SqlFunctionProjection(
                "round",
                NHibernateUtil.Int32,
                _context.ProjectionVisitor.CreateProjection(arguments[0])
            );
        }

        public override IProjection CastMethod(CastMethod method, Expression[] arguments)
        {
            var projection = _context.ProjectionVisitor.CreateProjection(arguments[0]);

            switch (LiteralUtil.CoerceString((LiteralExpression)arguments[1]))
            {
                case "Edm.Byte":
                case "Edm.SByte":
                case "Edm.Int16":
                case "Edm.Int32":
                    return new SqlFunctionProjection("round", NHibernateUtil.Int32, projection);

                case "Edm.Int64":
                    return new SqlFunctionProjection("round", NHibernateUtil.Int64, projection);

                default:
                    return projection;
            }
        }

        public override IProjection YearMethod(YearMethod method, Expression[] arguments)
        {
            return DatePartMethod(method, arguments, "year");
        }

        public override IProjection MonthMethod(MonthMethod method, Expression[] arguments)
        {
            return DatePartMethod(method, arguments, "month");
        }

        public override IProjection DayMethod(DayMethod method, Expression[] arguments)
        {
            return DatePartMethod(method, arguments, "day");
        }

        public override IProjection HourMethod(HourMethod method, Expression[] arguments)
        {
            return DatePartMethod(method, arguments, "hour");
        }

        public override IProjection MinuteMethod(MinuteMethod method, Expression[] arguments)
        {
            return DatePartMethod(method, arguments, "minute");
        }

        public override IProjection SecondMethod(SecondMethod method, Expression[] arguments)
        {
            return DatePartMethod(method, arguments, "second");
        }

        private IProjection DatePartMethod(DatePartMethod method, Expression[] arguments, string function)
        {
            return new SqlFunctionProjection(
                function,
                NHibernateUtil.Int32,
                _context.ProjectionVisitor.CreateProjection(arguments[0])
            );
        }

        public override IProjection CustomMethod(ICustomMethod customMethod, Expression[] arguments)
        {
            return customMethod.CreateProjection(
                arguments.Select(x => x.Type == ExpressionType.Literal
                    ? ((LiteralExpression)x).Value
                    : _context.ProjectionVisitor.CreateProjection(x)
                ).ToArray()
            );
        }
    }
}
