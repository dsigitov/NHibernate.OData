﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData.Test.Support
{
    internal abstract class NormalizedTestFixture : ParserTestFixture
    {
        protected override void Verify(Expression actual, Expression expected)
        {
            base.Verify(actual.Visit(new AliasingNormalizeVisitor(new CriterionBuildContext(ODataSessionFactoryContext.Empty, new ODataParserConfiguration()), null, null)), expected);
        }

        protected void Verify(string source, object value)
        {
            if (!(value is Expression))
                value = new LiteralExpression(value);

            base.Verify(source, (Expression)value);
        }

        protected override Expression VerifyThrows(Expression expression)
        {
            return expression.Visit(new AliasingNormalizeVisitor(new CriterionBuildContext(ODataSessionFactoryContext.Empty, new ODataParserConfiguration()), null, null));
        }
    }
}
