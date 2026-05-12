namespace FakeNewsDetector.Models
{
    public class AnalysisResult
    {
        public bool Success { get; set; }
        public double Score { get; set; }
        public string Verdict { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
        public List<AnalysisFactor> Factors { get; set; } = new List<AnalysisFactor>();
    }

    public class AnalysisFactor
    {
        public string Name { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
