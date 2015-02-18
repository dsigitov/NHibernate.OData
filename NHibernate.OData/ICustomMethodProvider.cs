using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    public interface ICustomMethodProvider
    {
        ICustomMethod FindMethodByName(string name);
    }
}