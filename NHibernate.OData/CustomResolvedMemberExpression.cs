namespace NHibernate.OData
{
    public class CustomResolvedMemberExpression : Expression
    {
        public ICustomMemberExpression CustomMemberExpression { get; private set; }

        public CustomResolvedMemberExpression(ICustomMemberExpression customMemberExpression)
            : base(ExpressionType.CustomResolvedMember)
        {
            Require.NotNull(customMemberExpression, "customMemberExpression");

            CustomMemberExpression = customMemberExpression;
        }

        public override bool IsBool
        {
            get { return CustomMemberExpression.IsBool; }
        }

        public override T Visit<T>(IVisitor<T> visitor)
        {
            return visitor.CustomResolvedMemberExpression(this);
        }
    }
}