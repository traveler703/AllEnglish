namespace AllEnBackend.Dtos
{
    public class ImportJsonRequest
    {
        public string JsonContent { get; set; } = string.Empty;
    }

    public class ImportResponse
    {
        public bool Success { get; set; }
        public int ImportedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
