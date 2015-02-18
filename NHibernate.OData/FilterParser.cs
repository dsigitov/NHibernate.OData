using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class FilterParser : Parser
    {
        public FilterParser(string source, ODataParserConfiguration configuration)
            : base(source, ParserMode.Normal, configuration)
        {
        }

        public Expression Parse()
        {
            var result = ParseBool();

            ExpectAtEnd();

            return result;
        }
    }
}
