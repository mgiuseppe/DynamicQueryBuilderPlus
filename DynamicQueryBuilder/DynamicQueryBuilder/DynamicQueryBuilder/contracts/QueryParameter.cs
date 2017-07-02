namespace DynamicQueryBuilder.DynamicQueryBuilder.contracts
{
    public class QueryParameter
    {
        public string NameSpace { get; set; }
        public string ClassName { get; set; }
        public string FieldName { get; set; }
        public string ParameterName { get; set; }
        public ParameterType ParameterType { get; set; }
    }
}
