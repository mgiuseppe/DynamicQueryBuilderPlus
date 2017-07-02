using DynamicQueryBuilder.DynamicQueryBuilder.contracts;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace DynamicQueryBuilder.DynamicQueryBuilder
{
    public class QueryExecutor<T>
    {
        private string _connectionString { get; set; }
        private QueryId _queryId { get; set; }
        private Query _query { get; set; }
        private List<object> _inputData { get; set; }
        public QueryExecutor(QueryId queryId, List<object> inputData, string connectionString)
        {
            this._connectionString = connectionString;
            this._queryId = queryId;
            this._query = new QueryDAO(_connectionString).GetQuery(_queryId);
            this._inputData = inputData;
        }

        public List<T> Execute()
        {
            var outputList = Activator.CreateInstance<List<T>>();

            using (var conn = new SQLiteConnection(_connectionString))
            using (var cmd = new SQLiteCommand())
            {
                cmd.Connection = conn;
                cmd.CommandText = BuildText();
                AddParameters(cmd);
                
                conn.Open();
                using (var dr = cmd.ExecuteReader(CommandBehavior.SingleResult))
                    while (dr.Read())
                        outputList.Add(BuildOutput(dr));
            }

            return outputList;
        }

        private string BuildText()
        {
            return String.Join(" ", _query.Chunks.OrderBy(c => c.Type).ThenBy(c => c.Order).Select(c => c.Text)); ;
        }

        private void AddParameters(SQLiteCommand cmd)
        {
            foreach (var parameterInfo in _query.ParametersInfo.Where(p => p.ParameterType == ParameterType.Input))
            {
                var dtoType = Type.GetType(String.Format("{0}.{1}", parameterInfo.NameSpace, parameterInfo.ClassName));
                var dtoContainingValue = _inputData.First(c => c.GetType() == dtoType);
                var value = dtoContainingValue.GetType().GetProperty(parameterInfo.FieldName).GetValue(dtoContainingValue);

                var parameter = new SQLiteParameter(parameterInfo.ParameterName, value);
                cmd.Parameters.Add(parameter);
            }
        }

        private T BuildOutput(IDataReader dr)
        {
            var output = Activator.CreateInstance<T>();

            foreach(var outputParam in _query.ParametersInfo.Where(p => p.ParameterType == ParameterType.Output))
            {
                var value = dr.GetValue(dr.GetOrdinal(outputParam.ParameterName));
                output.GetType().GetProperty(outputParam.FieldName).SetValue(output, value);
            }

            return output;
        }
    }
}
