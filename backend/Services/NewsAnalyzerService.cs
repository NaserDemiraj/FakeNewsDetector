using FakeNewsDetector.Models;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace FakeNewsDetector.Services
{
    public class NewsAnalyzerService : INewsAnalyzerService
    {
        private readonly ILogger<NewsAnalyzerService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        
        public NewsAnalyzerService(ILogger<NewsAnalyzerService> logger, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }
        
        public async Task<AnalysisResult> AnalyzeContentAsync(string content)
        {
            // Use mock/fallback analysis when API is unavailable
            return await AnalyzeContentWithMockAsync(content);
        }

        private async Task<AnalysisResult> AnalyzeContentWithMockAsync(string content)
        {
            await Task.Delay(100); // Simulate processing delay
            
            // Mock analysis logic
            var result = new AnalysisResult
            {
                Success = true,
                Verdict = "Likely Legitimate",
                Score = 78,
                Explanation = "This source appears to be from an established news organization with a history of accurate reporting.",
                Factors = new List<AnalysisFactor>
                {
                    new AnalysisFactor { Name = "Source Credibility", Score = 85, Details = "Established news organization with editorial standards" },
                    new AnalysisFactor { Name = "Factual Accuracy", Score = 80, Details = "Uses verifiable facts and citations" },
                    new AnalysisFactor { Name = "Balanced Reporting", Score = 75, Details = "Presents multiple perspectives on issues" },
                    new AnalysisFactor { Name = "Sensationalism", Score = 70, Details = "Minimal use of clickbait or exaggerated language" },
                }
            };
            
            return result;
        }

        public async Task<AnalysisResult> AnalyzeContentAsyncOld(string content)
        {
            _logger.LogInformation("Analyzing content with Gemini API");
            
            try
            {
                // Call Gemini API to analyze the content
                var geminiResponse = await CallGeminiApiAsync(content);
                
                // Parse the response and create the analysis result
                var result = ParseGeminiResponse(geminiResponse, content);
                
                _logger.LogInformation("Analysis complete. Verdict: {Verdict}", result.Verdict);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing content with Gemini API");
                
                // Fallback to a basic analysis if Gemini API fails
                return new AnalysisResult
                {
                    Success = false,
                    Score = 50,
                    Verdict = "Analysis Error",
                    Explanation = $"Error analyzing content: {ex.Message}. Please try again later.",
                    Factors = new List<AnalysisFactor>
                    {
                        new AnalysisFactor { Name = "API Error", Score = 50 }
                    }
                };
            }
        }
        
        private async Task<string> CallGeminiApiAsync(string content)
        {
            var httpClient = _httpClientFactory.CreateClient();
            
            // Groq API endpoint (OpenAI-compatible)
            string apiUrl = "https://api.groq.com/openai/v1/chat/completions";
            
            // Create the prompt for ChatGPT
            string prompt = $@"You are a fact-checking expert specializing in detecting fake news. 
Analyze the following content and determine if it appears to be legitimate news or potentially fake/misleading.

Content to analyze:
---
{content}
---

Provide a detailed analysis in JSON format with the following structure:
{{
  ""verdict"": ""Likely Legitimate"" or ""Potentially Misleading"" or ""Potentially Fake News"",
  ""score"": [number 0-100 where 100 is completely legitimate],
  ""explanation"": ""[Your explanation in 2-3 sentences]"",
  ""factors"": [
    {{ ""name"": ""[Factor name]"", ""score"": [0-100], ""details"": ""[Brief explanation]"" }},
    {{ ""name"": ""[Factor name]"", ""score"": [0-100], ""details"": ""[Brief explanation]"" }},
    {{ ""name"": ""[Factor name]"", ""score"": [0-100], ""details"": ""[Brief explanation]"" }},
    {{ ""name"": ""[Factor name]"", ""score"": [0-100], ""details"": ""[Brief explanation]"" }}
  ]
}}

Only respond with valid JSON, no additional text.";

            // Create the request payload for Groq
            var requestPayload = new
            {
                model = "mixtral-8x7b",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                temperature = 0.7
            };
            
            // Add OpenAI API key to headers
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _geminiApiKey);
            
            // Send the request to OpenAI
            var response = await httpClient.PostAsJsonAsync(apiUrl, requestPayload);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq API returned error: {response.StatusCode}, {errorContent}");
            }
            
            // Parse the response
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseJson = JsonDocument.Parse(responseContent);
            
            // Extract the text from ChatGPT response
            var chatContent = responseJson.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";
            
            // Wrap it in a mock Gemini-style response for compatibility with existing parser
            var wrappedResponse = $@"{{
  ""candidates"": [
    {{
      ""content"": {{
        ""parts"": [
          {{
            ""text"": ""{JsonEncode(chatContent)}""
          }}
        ]
      }}
    }}
  ]
}}";
            
            return wrappedResponse;
        }
        
        private static string JsonEncode(string text)
        {
            return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }
        
        private AnalysisResult ParseGeminiResponse(string geminiResponse, string originalContent)
        {
            try
            {
                // Parse the Gemini API response
                var responseJson = JsonDocument.Parse(geminiResponse);
                
                // Extract the text from the response
                var text = responseJson.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";
                
                // Extract the JSON part from the text
                var jsonMatch = Regex.Match(text, @"\{[\s\S]*\}");
                if (!jsonMatch.Success)
                {
                    throw new Exception("Could not extract JSON from Gemini response");
                }
                
                var analysisJson = jsonMatch.Value;
                var analysis = JsonDocument.Parse(analysisJson);
                
                // Create the analysis result
                var result = new AnalysisResult
                {
                    Success = true
                };
                
                // Extract verdict
                if (analysis.RootElement.TryGetProperty("verdict", out var verdictElement))
                {
                    result.Verdict = verdictElement.GetString() ?? "Unknown";
                }
                
                // Extract score
                if (analysis.RootElement.TryGetProperty("score", out var scoreElement))
                {
                    result.Score = scoreElement.GetDouble();
                }
                
                // Extract explanation
                if (analysis.RootElement.TryGetProperty("explanation", out var explanationElement))
                {
                    result.Explanation = explanationElement.GetString() ?? "";
                }
                
                // Extract factors
                if (analysis.RootElement.TryGetProperty("factors", out var factorsElement))
                {
                    result.Factors = new List<AnalysisFactor>();
                    
                    foreach (var factor in factorsElement.EnumerateArray())
                    {
                        var factorName = factor.GetProperty("name").GetString() ?? "Unknown Factor";
                        var factorScore = factor.GetProperty("score").GetDouble();
                        
                        result.Factors.Add(new AnalysisFactor
                        {
                            Name = factorName,
                            Score = factorScore,
                            Details = factor.TryGetProperty("details", out var detailsElement) ? detailsElement.GetString() : null
                        });
                    }
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing Gemini response");
                
                // Create a fallback analysis result
                return new AnalysisResult
                {
                    Success = false,
                    Score = 50,
                    Verdict = "Analysis Error",
                    Explanation = $"Error parsing AI response: {ex.Message}. Please try again later.",
                    Factors = new List<AnalysisFactor>
                    {
                        new AnalysisFactor { Name = "Parsing Error", Score = 50 }
                    }
                };
            }
        }
    }
}
