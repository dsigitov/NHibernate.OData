using NHibernate.Criterion;

namespace NHibernate.OData
{
    public interface ICustomMemberExpression
    {
        bool IsBool { get; }
        IProjection CreateProjection();

        ICriterion CreateLikeCriterion(string value, MatchMode matchMode);
        ICriterion CreateIsNullCriterion(bool isNotNull);
        ICriterion CreateComparisonCriterion(Operator @operator, object value);
    }
}