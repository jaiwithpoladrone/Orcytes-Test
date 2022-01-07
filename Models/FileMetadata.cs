namespace CSharpFileUpload.Models
{
    public class FileMetadata
    {
        public int ChunkSize { get; set; }
        public string FileUid { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
        public int TotalChunks { get; set; }
        public int TotalFileSize { get; set; }
        public string ContentType { get; set; }
        public bool Completed { get; set; }
    }
}
