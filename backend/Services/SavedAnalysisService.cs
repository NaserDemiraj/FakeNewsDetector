using FakeNewsDetector.Models;

namespace FakeNewsDetector.Services
{
    public class SavedAnalysisService : ISavedAnalysisService
    {
        private readonly List<SavedAnalysis> _analyses = new List<SavedAnalysis>();
        private readonly ILogger<SavedAnalysisService> _logger;
        
        public SavedAnalysisService(ILogger<SavedAnalysisService> logger)
        {
            _logger = logger;
            
            // Add some sample data
            _analyses.Add(new SavedAnalysis
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Climate Change Report",
                Url = "https://example.com/climate-report",
                Score = 85.5,
                Verdict = "Likely Legitimate",
                Date = DateTime.UtcNow.AddDays(-2)
            });
            
            _analyses.Add(new SavedAnalysis
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Celebrity Scandal Exposed",
                Url = "https://example.com/celebrity-scandal",
                Score = 32.1,
                Verdict = "Potentially Fake News",
                Date = DateTime.UtcNow.AddDays(-1)
            });
            
            _analyses.Add(new SavedAnalysis
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Economic Forecast 2023",
                Url = "https://example.com/economic-forecast",
                Score = 78.9,
                Verdict = "Likely Legitimate",
                Date = DateTime.UtcNow.AddHours(-5)
            });
        }
        
        public void SaveAnalysis(SavedAnalysis analysis)
        {
            _analyses.Add(analysis);
            _logger.LogInformation("Analysis saved: {Title}", analysis.Title);
        }
        
        public List<SavedAnalysis> GetRecentAnalyses(int count)
        {
            return _analyses
                .OrderByDescending(a => a.Date)
                .Take(count)
                .ToList();
        }
        
        public List<SavedAnalysis> GetAllAnalyses()
        {
            return _analyses.ToList();
        }
    }
}
