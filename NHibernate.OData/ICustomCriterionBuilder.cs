using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Criterion;

namespace NHibernate.OData
{
    public interface ICustomCriterionBuilder
    {
        ICriterion Like(IProjection projection, string value, MatchMode matchMode);
    }
}