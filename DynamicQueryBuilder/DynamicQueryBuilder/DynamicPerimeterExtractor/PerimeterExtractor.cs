using DynamicQueryBuilder.DynamicPerimeterExtractor.contracts;
using DynamicQueryBuilder.DynamicQueryBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DynamicQueryBuilder.DynamicPerimeterExtractor
{
    public class PerimeterExtractor
    {
        private string _connectionString { get; set; }

        public PerimeterExtractor(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public List<long> Extract()
        {
            var inputData = new List<object> { new DDEExtractionParameters { MinDate = new DateTime(2017, 1, 1) } };
            var executor = new QueryExecutor<DDEExtractionOutput>(QueryId.DDE_Extraction, inputData, _connectionString);

            return executor.Execute().Select(x => x.ProgPratica).ToList();
        } 
    }
}
