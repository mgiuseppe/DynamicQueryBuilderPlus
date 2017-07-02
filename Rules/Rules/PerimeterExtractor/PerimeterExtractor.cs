using DynamicQueryBuilder.DynamicPerimeterExtractor.contracts;
using DynamicQueryBuilder.DynamicQueryBuilder;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace DynamicQueryBuilder.DynamicPerimeterExtractor
{
    public class PerimeterExtractor
    {
        private string _connectionString { get; set; }
        private PerimeterExtractorInputDto _input { get; set; }

        public PerimeterExtractor(PerimeterExtractorInputDto input, string connectionString)
        {
            this._input = input;
            this._connectionString = connectionString;
        }

        public List<long> Extract()
        {
            var output = new List<long>();

            using (var conn = new SQLiteConnection(_connectionString))
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = new QueryDAO(_connectionString)
                                      .GetQuery(_input.QueryId)
                                      .BuildString();

                conn.Open();
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    while (dr.Read())
                        output.Add(dr.GetFieldValue<long>(dr.GetOrdinal("ID")));
            }

            return output;
        } 
    }
}
