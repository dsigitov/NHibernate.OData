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

        object Normalize(object[] literalValues);

        ICriterion CreateCriterion(object[] arguments);
        IProjection CreateProjection(object[] arguments);
    }
}
