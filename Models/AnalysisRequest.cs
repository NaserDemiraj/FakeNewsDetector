namespace FakeNewsDetector.Models
{
    public class AnalysisRequest
    {
        public string Type { get; set; } = "text"; // "url" or "text"
        public string Content { get; set; } = string.Empty;
    }
}
