using FakeNewsDetector.Models;
using System.Text.RegularExpressions;

namespace FakeNewsDetector.Services
{
    public class NewsAnalyzerService : INewsAnalyzerService
    {
        private readonly ILogger<NewsAnalyzerService> _logger;
        
        // Lists of words for analysis
        private readonly string[] _clickbaitTerms = new[] 
        { 
            "you won't believe", "shocking", "mind-blowing", "unbelievable", 
            "incredible", "insane", "amazing", "jaw-dropping", "secret", 
            "they don't want you to know", "this will change everything" 
        };
        
        private readonly string[] _emotionalTerms = new[] 
        { 
            "outrageous", "horrific", "terrifying", "devastating", "alarming",
            "catastrophic", "scandalous", "bombshell", "explosive", "fury", "rage"
        };
        
        private readonly string[] _balancedReportingTerms = new[] 
        { 
            "however", "on the other hand", "according to", "experts say", 
            "research shows", "studies indicate", "evidence suggests", 
            "data shows", "analysis reveals", "sources confirm" 
        };
        
        private readonly string[] _conspiracyTerms = new[] 
        { 
            "conspiracy", "cover-up", "they're hiding", "government doesn't want you to know",
            "secret agenda", "what they don't tell you", "shadow", "illuminati", 
            "new world order", "deep state", "controlled by" 
        };
        
        public NewsAnalyzerService(ILogger<NewsAnalyzerService> logger)
        {
            _logger = logger;
        }
        
        public async Task<AnalysisResult> AnalyzeContentAsync(string content)
        {
            _logger.LogInformation("Analyzing content of length: {Length}", content.Length);
            
            // Normalize content
            content = content.ToLowerInvariant();
            
            // Calculate individual factors
            var sourceCredibility = AnalyzeSourceCredibility(content);
            var emotionalLanguage = AnalyzeEmotionalLanguage(content);
            var factConsistency = AnalyzeFactConsistency(content);
            var clickbaitScore = AnalyzeClickbait(content);
            var conspiracyScore = AnalyzeConspiracyTheories(content);
            
            // Calculate overall score (weighted average)
            double overallScore = (
                sourceCredibility * 0.25 +
                emotionalLanguage * 0.2 +
                factConsistency * 0.25 +
                clickbaitScore * 0.15 +
                conspiracyScore * 0.15
            );
            
            // Create result
            var result = new AnalysisResult
            {
                Success = true,
                Score = Math.Round(overallScore, 1),
                Factors = new List<AnalysisFactor>
                {
                    new AnalysisFactor { Name = "Source Credibility", Score = Math.Round(sourceCredibility, 1) },
                    new AnalysisFactor { Name = "Emotional Language", Score = Math.Round(emotionalLanguage, 1) },
                    new AnalysisFactor { Name = "Fact Consistency", Score = Math.Round(factConsistency, 1) },
                    new AnalysisFactor { Name = "Clickbait Detection", Score = Math.Round(clickbaitScore, 1) },
                    new AnalysisFactor { Name = "Conspiracy Theories", Score = Math.Round(conspiracyScore, 1) }
                }
            };
            
            // Determine verdict and explanation
            if (result.Score > 70)
            {
                result.Verdict = "Likely Legitimate";
                result.Explanation = "This content appears to be from a credible source and contains balanced reporting with factual information.";
            }
            else if (result.Score > 40)
            {
                result.Verdict = "Potentially Misleading";
                result.Explanation = "This content contains some questionable claims and may use emotional language or clickbait tactics.";
            }
            else
            {
                result.Verdict = "Potentially Fake News";
                result.Explanation = "This content contains multiple red flags including excessive emotional language, clickbait tactics, and conspiracy theories.";
            }
            
            _logger.LogInformation("Analysis complete. Score: {Score}, Verdict: {Verdict}", 
                result.Score, result.Verdict);
                
            return result;
        }
        
        private double AnalyzeSourceCredibility(string content)
        {
            // In a real implementation, this would check against a database of known sources
            // For this simplified version, we'll check for indicators of credibility
            
            double score = 60; // Start with a neutral-positive score
            
            // Check for citations and references
            if (content.Contains("according to") || 
                content.Contains("cited") || 
                content.Contains("referenced") ||
                content.Contains("study published"))
            {
                score += 15;
            }
            
            // Check for quotes from experts
            if (content.Contains("expert") || 
                content.Contains("professor") || 
                content.Contains("researcher") ||
                content.Contains("scientist") ||
                content.Contains("official"))
            {
                score += 10;
            }
            
            // Check for balanced reporting
            foreach (var term in _balancedReportingTerms)
            {
                if (content.Contains(term))
                {
                    score += 5;
                    // Cap the bonus at 20
                    if (score > 80) break;
                }
            }
            
            return Math.Min(100, score);
        }
        
        private double AnalyzeEmotionalLanguage(string content)
        {
            // Higher score means less emotional language (more objective)
            double score = 80; // Start with a good score
            
            // Count emotional terms
            int emotionalTermCount = 0;
            foreach (var term in _emotionalTerms)
            {
                // Count occurrences
                int count = Regex.Matches(content, $@"\b{term}\b").Count;
                emotionalTermCount += count;
            }
            
            // Reduce score based on emotional term density
            // Normalize by content length to avoid penalizing longer articles
            double contentLength = content.Length;
            double emotionalDensity = (emotionalTermCount * 100) / Math.Max(1, contentLength);
            
            // Reduce score based on density
            score -= emotionalDensity * 200; // Multiplier to make the effect significant
            
            return Math.Max(0, Math.Min(100, score));
        }
        
        private double AnalyzeFactConsistency(string content)
        {
            // In a real implementation, this would check facts against a database
            // For this simplified version, we'll look for indicators of fact-based reporting
            
            double score = 50; // Start with a neutral score
            
            // Check for data and statistics
            if (Regex.IsMatch(content, @"\d+%") || 
                Regex.IsMatch(content, @"\d+ percent") ||
                Regex.IsMatch(content, @"statistics show") ||
                Regex.IsMatch(content, @"data indicates"))
            {
                score += 15;
            }
            
            // Check for specific dates
            if (Regex.IsMatch(content, @"\b\d{1,2}/\d{1,2}/\d{2,4}\b") || 
                Regex.IsMatch(content, @"\b(January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},\s+\d{4}\b"))
            {
                score += 10;
            }
            
            // Check for balanced reporting terms
            foreach (var term in _balancedReportingTerms)
            {
                if (content.Contains(term))
                {
                    score += 5;
                    // Cap the bonus at 30
                    if (score > 80) break;
                }
            }
            
            return Math.Min(100, score);
        }
        
        private double AnalyzeClickbait(string content)
        {
            // Higher score means less clickbait (better)
            double score = 90; // Start with a good score
            
            // Check for clickbait phrases
            foreach (var term in _clickbaitTerms)
            {
                if (content.Contains(term))
                {
                    score -= 15;
                    // Floor at 0
                    if (score <= 0) 
                    {
                        score = 0;
                        break;
                    }
                }
            }
            
            // Check for excessive punctuation
            int exclamationCount = content.Count(c => c == '!');
            if (exclamationCount > 3)
            {
                score -= Math.Min(30, exclamationCount * 2);
            }
            
            // Check for ALL CAPS sections
            var allCapsMatches = Regex.Matches(content, @"\b[A-Z]{4,}\b");
            if (allCapsMatches.Count > 2)
            {
                score -= Math.Min(20, allCapsMatches.Count * 5);
            }
            
            return Math.Max(0, score);
        }
        
        private double AnalyzeConspiracyTheories(string content)
        {
            // Higher score means fewer conspiracy theory indicators (better)
            double score = 90; // Start with a good score
            
            // Check for conspiracy theory phrases
            foreach (var term in _conspiracyTerms)
            {
                if (content.Contains(term))
                {
                    score -= 15;
                    // Floor at 0
                    if (score <= 0) 
                    {
                        score = 0;
                        break;
                    }
                }
            }
            
            return Math.Max(0, score);
        }
    }
}
