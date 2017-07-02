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
            Query query = new Query()
            {
                Id = queryId,
                Chunks = GetQueryChunks(queryId),
                ParametersInfo = GetQueryParameters(queryId)
            };

            return query;
        }

        private List<QueryChunk> GetQueryChunks(QueryId queryId)
        {
            List<QueryChunk> chunks = new List<QueryChunk>();

            using (var conn = new SQLiteConnection(_connectionString))
            using (var cmd = new SQLiteCommand(@"SELECT * FROM QUERY_CHUNK WHERE QUERY_ID = @query_id AND ENABLED = 1", conn))
            {
                cmd.Parameters.Add(new SQLiteParameter("@query_id", (int)queryId));

                conn.Open();
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    while (dr.Read())
                        chunks.Add(BuildChunk(dr));
            }

            return chunks;
        }

        private List<QueryParameter> GetQueryParameters(QueryId queryId)
        {
            var parameters = new List<QueryParameter>();

            using (var conn = new SQLiteConnection(_connectionString))
            using (var cmd = new SQLiteCommand(@"SELECT C.NAME_SPACE, C.CLASS_NAME, CF.FIELD_NAME, APF.PARAMETER_NAME, APF.PARAMETER_TYPE
                                                 FROM ASS_PARAMETER_FIELD APF
                                                 INNER JOIN CLASS_FIELD CF ON APF.CLASS_FIELD_ID = CF.ID
                                                 INNER JOIN CLASS C ON CF.CLASS_ID = C.ID
                                                 WHERE APF.QUERY_ID = @query_id", conn))
            {
                cmd.Parameters.Add(new SQLiteParameter("@query_id", (int)queryId));

                conn.Open();
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    while (dr.Read())
                        parameters.Add(BuildQueryParameter(dr));
            }

            return parameters;
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

        private QueryParameter BuildQueryParameter(SQLiteDataReader dr) =>
            new QueryParameter
            {
                NameSpace = dr.GetFieldValue<string>(dr.GetOrdinal("NAME_SPACE")),
                ClassName = dr.GetFieldValue<string>(dr.GetOrdinal("CLASS_NAME")),
                FieldName = dr.GetFieldValue<string>(dr.GetOrdinal("FIELD_NAME")),
                ParameterName = dr.GetFieldValue<string>(dr.GetOrdinal("PARAMETER_NAME")),
                ParameterType = (ParameterType)dr.GetFieldValue<long>(dr.GetOrdinal("PARAMETER_TYPE")),
            };
    }
}
