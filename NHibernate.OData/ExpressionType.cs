using System.Text;
using System.Collections.Generic;
using System;

namespace NHibernate.OData
{
    public enum ExpressionType
    {
        Literal,
        Bool,
        Comparison,
        Arithmetic,
        Not,
        IsOf,
        BoolCast,
        MethodCall,
        Negative,
        Member,
        Paren,
        ArithmeticUnary,
        ResolvedMember,
        CustomResolvedMember,
        Lambda
    }
}