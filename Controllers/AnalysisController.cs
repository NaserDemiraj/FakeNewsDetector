using FakeNewsDetector.Models;
using FakeNewsDetector.Services;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FakeNewsDetector.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalysisController : ControllerBase
    {
        private readonly INewsAnalyzerService _analyzerService;
        private readonly ISavedAnalysisService _savedAnalysisService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AnalysisController> _logger;

        public AnalysisController(
            INewsAnalyzerService analyzerService,
            ISavedAnalysisService savedAnalysisService,
            IHttpClientFactory httpClientFactory,
            ILogger<AnalysisController> logger)
        {
            _analyzerService = analyzerService;
            _savedAnalysisService = savedAnalysisService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeNews([FromBody] AnalysisRequest request)
        {
            try
            {
                _logger.LogInformation("Received analysis request: {RequestType}", request.Type);
                
                if (string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest("Content cannot be empty");
                }

                string content;
                string title = "Untitled Analysis";
                string sourceUrl = "";
                
                if (request.Type == "url")
                {
                    // Validate URL
                    if (!Uri.TryCreate(request.Content, UriKind.Absolute, out var uriResult) || 
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        return BadRequest("Invalid URL format");
                    }

                    sourceUrl = request.Content;
                    
                    try
                    {
                        // Fetch content from URL
                        var httpClient = _httpClientFactory.CreateClient();
                        httpClient.DefaultRequestHeaders.Add("User-Agent", "FakeNewsDetector/1.0");
                        var response = await httpClient.GetAsync(request.Content);
                        
                        if (!response.IsSuccessStatusCode)
                        {
                            return BadRequest($"Failed to fetch content from URL: {response.StatusCode}");
                        }
                        
                        var htmlContent = await response.Content.ReadAsStringAsync();
                        
                        // Extract text and title from HTML using HtmlAgilityPack
                        var extractionResult = ExtractTextAndTitleFromHtml(htmlContent);
                        content = extractionResult.Text;
                        title = string.IsNullOrEmpty(extractionResult.Title) ? "Article from " + new Uri(request.Content).Host : extractionResult.Title;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error fetching URL content");
                        return BadRequest($"Error fetching URL content: {ex.Message}");
                    }
                }
                else
                {
                    content = request.Content;
                    title = "Text Analysis";
                }
                
                if (string.IsNullOrWhiteSpace(content))
                {
                    return BadRequest("No content could be extracted for analysis");
                }
                
                // Analyze the content
                var result = await _analyzerService.AnalyzeContentAsync(content);
                
                // Save the analysis result
                var savedAnalysis = new SavedAnalysis
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = title,
                    Url = sourceUrl,
                    Score = result.Score,
                    Verdict = result.Verdict,
                    Date = DateTime.UtcNow
                };
                
                _savedAnalysisService.SaveAnalysis(savedAnalysis);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing content");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpGet("recent")]
        public IActionResult GetRecentAnalyses()
        {
            try
            {
                _logger.LogInformation("Fetching recent analyses");
                var recentAnalyses = _savedAnalysisService.GetRecentAnalyses(10);
                return Ok(recentAnalyses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent analyses");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        
        [HttpGet("stats")]
        public IActionResult GetAnalysisStats()
        {
            try
            {
                _logger.LogInformation("Fetching analysis stats");
                var allAnalyses = _savedAnalysisService.GetAllAnalyses();
                
                var stats = new
                {
                    TotalAnalyses = allAnalyses.Count,
                    FakeNewsDetected = allAnalyses.Count(a => a.Score < 40),
                    AverageScore = allAnalyses.Any() ? allAnalyses.Average(a => a.Score) : 0
                };
                
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis stats");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private (string Text, string Title) ExtractTextAndTitleFromHtml(string html)
        {
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                
                // Extract title
                string title = "";
                var titleNode = doc.DocumentNode.SelectSingleNode("//title");
                if (titleNode != null)
                {
                    title = titleNode.InnerText.Trim();
                }
                
                // Try to get a better title from h1 if title is too generic
                if (string.IsNullOrEmpty(title) || title.Contains("Home") || title.Contains("Index"))
                {
                    var h1Node = doc.DocumentNode.SelectSingleNode("//h1");
                    if (h1Node != null)
                    {
                        title = h1Node.InnerText.Trim();
                    }
                }

                // Remove script and style elements
                var nodes = doc.DocumentNode.SelectNodes("//script|//style");
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        node.Remove();
                    }
                }

                // Get text from body
                var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
                if (bodyNode == null)
                {
                    return (doc.DocumentNode.InnerText, title);
                }

                // Extract text from paragraphs, headings, and other content elements
                var contentNodes = bodyNode.SelectNodes("//p|//h1|//h2|//h3|//h4|//h5|//h6|//article|//section");
                if (contentNodes == null || contentNodes.Count == 0)
                {
                    return (bodyNode.InnerText, title);
                }

                var sb = new StringBuilder();
                foreach (var node in contentNodes)
                {
                    sb.AppendLine(node.InnerText.Trim());
                }

                var text = sb.ToString();
                
                // Clean up the text
                text = System.Net.WebUtility.HtmlDecode(text);
                text = text.Replace("\t", " ").Replace("\r", " ");
                text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
                
                return (text.Trim(), title);
            }
            catch
            {
                // Fallback to simple regex-based extraction
                var text = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", " ");
                text = System.Net.WebUtility.HtmlDecode(text);
                text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
                return (text.Trim(), "");
            }
        }
    }
}
