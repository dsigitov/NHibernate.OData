using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;

namespace NHibernate.OData
{
    public interface ICustomMethod
    {
        ArgumentType[] ArgumentTypes { get; }
        object Normalize(object[] literalValues);
        ICriterion CreateCriterion(object[] arguments);
        bool IsBool { get; }
    }
}
