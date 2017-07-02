using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DynamicQueryBuilder.DynamicQueryBuilder.QueryDAO;

namespace DynamicQueryBuilder.DynamicQueryBuilder.contracts
{
    public class Query
    {
        public QueryId Id { get; set; }
        public List<QueryChunk> Chunks { get; set; }

        public string BuildString() => String.Join(" ", Chunks.OrderBy(c => c.Type).ThenBy(c => c.Order).Select(c => c.Text));
    }
}
