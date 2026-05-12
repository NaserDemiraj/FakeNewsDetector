using FakeNewsDetector.Models;

namespace FakeNewsDetector.Services
{
    public interface ISavedAnalysisService
    {
        void SaveAnalysis(SavedAnalysis analysis);
        List<SavedAnalysis> GetRecentAnalyses(int count);
        List<SavedAnalysis> GetAllAnalyses();
    }
}
