using DynamicQueryBuilder.DynamicQueryBuilder.contracts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;

namespace DynamicQueryBuilder.DynamicQueryBuilder
{
    /// <summary>
    /// Per utilizzare il QueryExecutor basta:
    /// Lato codice:
    /// - Creare una DTO con i campi di output della query (opzionale)
    /// - Creare una o più DTO con i parametri di input (opzionale)
    /// - Istanziare QueryExecutor passando come parametro la lista delle DTO con i parametri di input e l'id della query che si vuole eseguire e specificando come 
    ///   Tipo generico la DTO di output
    /// - Chiamare il metodo Execute che restituirà un'istanza della DTO per ogni riga restituita dalla query
    /// Lato DB:
    /// - Censire le DTO create e i relativi campi
    /// - Censire la query e le relative parti
    /// - Associare le colonne del result set della query con i campi della DTO di output
    /// - Associare i parametri di input della query con i campi delle DTO di input
    /// 
    /// NB: un parametro o una lista NULL verranno considerati come un parametro con valore DBNull.Value
    /// NB: 
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        private void AddParameters(IDbCommand cmd)
        {
            foreach (var parameterInfo in _query.ParametersInfo.Where(p => p.ParameterType == ParameterType.Input || p.ParameterType == ParameterType.InputList))
            {
                var dtoType = Type.GetType(String.Format("{0}.{1}", parameterInfo.NameSpace, parameterInfo.ClassName));
                var dtoContainingValue = _inputData.First(c => c.GetType() == dtoType);
                var value = dtoContainingValue.GetType().GetProperty(parameterInfo.FieldName).GetValue(dtoContainingValue);

                if (parameterInfo.ParameterType == ParameterType.Input)
                    AddSingleValueParameter(cmd, parameterInfo.ParameterName, value);
                else //List
                    AddListParameter(cmd, parameterInfo.ParameterName, (IList)value);
            }
        }

        private void AddSingleValueParameter(IDbCommand cmd, string parameterName, object value)
        {
            var parameter = new SQLiteParameter(parameterName, value ?? DBNull.Value);
            cmd.Parameters.Add(parameter);
        }

        private void AddListParameter(IDbCommand cmd, string parameterName, IEnumerable values)
        {
            var parameterNames = new List<string>();
            var nParameter = 1;
            foreach (var value in values ?? new List<object> { DBNull.Value })
            {
                var currName = string.Format("{0}{1}", parameterName, nParameter++);
                parameterNames.Add(currName);
                cmd.Parameters.Add(new SQLiteParameter(currName, value));
            }
            cmd.CommandText = cmd.CommandText.Replace(parameterName, string.Join(",", parameterNames));
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
