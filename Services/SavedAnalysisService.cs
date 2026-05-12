using FakeNewsDetector.Models;

namespace FakeNewsDetector.Services
{
    public class SavedAnalysisService : ISavedAnalysisService
    {
        private readonly List<SavedAnalysis> _analyses = new List<SavedAnalysis>();
        private readonly ILogger<SavedAnalysisService> _logger;
        
        // Add some sample data
        public SavedAnalysisService(ILogger<SavedAnalysisService> logger)
        {
            _logger = logger;
            
            // Add sample data
            _analyses.Add(new SavedAnalysis
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Climate Change Report",
                Url = "https://example.com/climate-report",
                Score = 85,
                Verdict = "Likely Legitimate",
                Date = DateTime.UtcNow.AddHours(-2)
            });
            
            _analyses.Add(new SavedAnalysis
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Political Scandal Exposed",
                Url = "https://example.com/political-scandal",
                Score = 32,
                Verdict = "Potentially Fake News",
                Date = DateTime.UtcNow.AddDays(-1)
            });
            
            _analyses.Add(new SavedAnalysis
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Health Benefits of Coffee",
                Url = "https://example.com/coffee-benefits",
                Score = 76,
                Verdict = "Likely Legitimate",
                Date = DateTime.UtcNow.AddDays(-2)
            });
        }
        
        public void SaveAnalysis(SavedAnalysis analysis)
        {
            _analyses.Add(analysis);
            _logger.LogInformation("Saved analysis: {Title}", analysis.Title);
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
