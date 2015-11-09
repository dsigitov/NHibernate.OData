using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace NHibernate.OData
{
    internal class CriterionBuildContext
    {
        public ODataSessionFactoryContext SessionFactoryContext { get; private set; }
        public ODataParserConfiguration Configuration { get; private set; }
        public CriterionVisitor CriterionVisitor { get; private set; }
        public ProjectionVisitor ProjectionVisitor { get; private set; }
        public IDictionary<string, Alias> AliasesByName { get; private set; }

        public int ExpressionLevel
        {
            get { return _lambdaContextStack.Count; }
        }

        private int _aliasCounter;

        private readonly Stack<LambdaExpressionContext> _lambdaContextStack = new Stack<LambdaExpressionContext>();

        public CriterionBuildContext(ODataSessionFactoryContext sessionFactoryContext, ODataParserConfiguration configuration)
        {
            Require.NotNull(sessionFactoryContext, "sessionFactoryContext");
            Require.NotNull(configuration, "configuration");

            SessionFactoryContext = sessionFactoryContext;
            Configuration = configuration;

            CriterionVisitor = new CriterionVisitor(this);
            ProjectionVisitor = new ProjectionVisitor(this);

            AliasesByName = new Dictionary<string, Alias>(StringComparer.Ordinal);
        }

        public void AddAliases(IEnumerable<Alias> aliasesToAdd)
        {
            Require.NotNull(aliasesToAdd, "aliasesToAdd");

            foreach (var alias in aliasesToAdd)
                AddAlias(alias);
        }

        public void AddAlias(Alias alias)
        {
            Require.NotNull(alias, "alias");

            AliasesByName.Add(alias.Name, alias);
        }

        public string CreateUniqueAliasName()
        {
            return "t" + (++_aliasCounter).ToString(CultureInfo.InvariantCulture);
        }

        public void PushLambdaContext(string parameterName, System.Type parameterType, string parameterAlias)
        {
            if (_lambdaContextStack.Any(x => x.ParameterName.Equals(parameterName, StringComparison.Ordinal)))
                throw new ODataException(string.Format(ErrorMessages.Expression_LambdaParameterIsAlreadyDefined, parameterName));

            _lambdaContextStack.Push(new LambdaExpressionContext(parameterName, parameterType, parameterAlias));
        }

        public void PopLambdaContext()
        {
            _lambdaContextStack.Pop();
        }

        public LambdaExpressionContext FindLambdaContext(string parameterName)
        {
            Require.NotNull(parameterName, "parameterName");

            return _lambdaContextStack.FirstOrDefault(x => x.ParameterName.Equals(parameterName, StringComparison.Ordinal));
        }
    }
}
