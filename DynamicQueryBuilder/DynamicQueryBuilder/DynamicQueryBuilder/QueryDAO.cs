using DynamicQueryBuilder.DynamicQueryBuilder.contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicQueryBuilder.DynamicQueryBuilder
{
    public partial class QueryDAO
    {
        public string _connectionString { get; set; }

        public QueryDAO(string connectionString) => this._connectionString = connectionString;

        public Query GetQuery(QueryId queryId)
        {
            Query query = new Query() { Id = queryId, Chunks = new List<QueryChunk>() };

            using(var conn = new SQLiteConnection(_connectionString))
            using(var cmd = new SQLiteCommand(@"SELECT * FROM QUERY_CHUNK WHERE QUERY_ID = @query_id AND ENABLED = 1",conn))
            {
                cmd.Parameters.Add(new SQLiteParameter("@query_id", (int)queryId));

                conn.Open();
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    while (dr.Read())
                        query.Chunks.Add(BuildChunk(dr));
            }

            return query;
        }

        private QueryChunk BuildChunk(SQLiteDataReader dr) =>
            new QueryChunk
            {
                Id = dr.GetFieldValue<long>(dr.GetOrdinal("ID")),
                QueryId = (QueryId)dr.GetFieldValue<long>(dr.GetOrdinal("QUERY_ID")),
                Type = (ChunkType)dr.GetFieldValue<long>(dr.GetOrdinal("TYPE")),
                Order = dr.GetFieldValue<long>(dr.GetOrdinal("ORDER")),
                Text = dr.GetFieldValue<string>(dr.GetOrdinal("TEXT")),
            };
    }
}
