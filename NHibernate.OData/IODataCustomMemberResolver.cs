using System.Collections.Generic;
using System.Reflection;

namespace NHibernate.OData
{
    public interface IODataCustomMemberResolver
    {
        bool CanResolve(PropertyInfo property);

        ICustomMemberExpression Resolve(
            PropertyInfo property,
            string path,
            IList<IMemberExpressionComponent> memberComponents
        );
    }
}