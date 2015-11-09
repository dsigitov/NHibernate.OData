using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.OData
{
    internal class OrderByParser : Parser
    {
        private readonly AliasingNormalizeVisitor _normalizeVisitor;
        private readonly ProjectionVisitor _projectionVisitor;

        public OrderByParser(string source, ODataParserConfiguration configuration, AliasingNormalizeVisitor normalizeVisitor, ProjectionVisitor projectionVisitor)
            : base(source, ParserMode.Normal, configuration)
        {
            Require.NotNull(normalizeVisitor, "normalizeVisitor");
            Require.NotNull(projectionVisitor, "projectionVisitor");

            _normalizeVisitor = normalizeVisitor;
            _projectionVisitor = projectionVisitor;
        }

        public OrderBy[] Parse()
        {
            var orderBys = new List<OrderBy>();

            while (true)
            {
                var result = ParseCommon();

                var projection = _projectionVisitor.CreateProjection(
                    result.Visit(_normalizeVisitor)
                );

                if (AtEnd)
                {
                    orderBys.Add(new OrderBy(projection, OrderByDirection.Ascending));

                    break;
                }
                else
                {
                    var direction = GetOrderByDirection(Current);

                    if (!direction.HasValue)
                        direction = OrderByDirection.Ascending;
                    else
                        MoveNext();

                    orderBys.Add(new OrderBy(projection, direction.Value));

                    if (AtEnd)
                        break;

                    if (Current != SyntaxToken.Comma)
                        throw new ODataException(ErrorMessages.OrderByParser_ExpectedNextOrEnd);
                    
                    MoveNext();
                }
            }

            return orderBys.ToArray();
        }
    }
}
