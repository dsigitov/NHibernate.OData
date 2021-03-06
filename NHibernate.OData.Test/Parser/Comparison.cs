﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.OData.Test.Support;
using NUnit.Framework;

namespace NHibernate.OData.Test.Parser
{
    [TestFixture]
    internal class Comparison : ParserTestFixture
    {
        [Test]
        public void Equals()
        {
            Verify(
                "1 eq 1",
                new ComparisonExpression(Operator.Eq, OneLiteral, OneLiteral)
            );
        }
    }
}
