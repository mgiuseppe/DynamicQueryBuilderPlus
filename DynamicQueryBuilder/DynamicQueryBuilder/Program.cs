using DynamicQueryBuilder.DynamicPerimeterExtractor;
using DynamicQueryBuilder.DynamicPerimeterExtractor.contracts;
using DynamicQueryBuilder.DynamicQueryBuilder;
using System;
using System.Diagnostics;

namespace DynamicQueryBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = @"Data Source=C:perimeterExtractor.db;FailIfMissing=True;";
            var perimeter = new PerimeterExtractor(connectionString).Extract();

            Debug.WriteLine(String.Join(",", perimeter));
        }
    }
}