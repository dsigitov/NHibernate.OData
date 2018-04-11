using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;

namespace NHibernate.OData
{
    public interface ICustomMethod
    {
        string Name { get; }
        bool IsBool { get; }
        ArgumentType[] ArgumentTypes { get; }

        bool TryNormalize(object[] literalValues, out object normalizedValue);

        ICriterion CreateCriterion(object[] arguments);
        IProjection CreateProjection(object[] arguments);
    }
}
