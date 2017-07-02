namespace DynamicQueryBuilder.DynamicQueryBuilder.contracts
{
    public class QueryChunk
    {
        public long Id { get; set; }
        public QueryId QueryId { get; internal set; }
        public ChunkType Type { get; set; }
        public long Order { get; set; }
        public string Text { get; set; }
    }
}