"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle, CardDescription, CardFooter } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs"
import { Progress } from "@/components/ui/progress"
import { Badge } from "@/components/ui/badge"
import { Search, LinkIcon, FileText, AlertCircle, CheckCircle, AlertTriangle, Info, Bot } from "lucide-react"

// Types for our analysis result
interface AnalysisFactor {
  name: string
  score: number
  details?: string
}

interface AnalysisResult {
  success: boolean
  score: number
  verdict: string
  explanation: string
  factors: AnalysisFactor[]
}

export default function Home() {
  const [url, setUrl] = useState("")
  const [text, setText] = useState("")
  const [isAnalyzing, setIsAnalyzing] = useState(false)
  const [result, setResult] = useState<AnalysisResult | null>(null)
  const [error, setError] = useState<string | null>(null)

  const analyzeNews = async (type: "url" | "text") => {
    setIsAnalyzing(true)
    setError(null)

    try {
      const content = type === "url" ? url : text

      // Try to connect to the backend
      const response = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/api/Analysis`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          type,
          content,
        }),
      })

      if (response.ok) {
        const data = await response.json()
        setResult(data)
      } else {
        // Fallback to mock data if backend is not available
        console.log("Backend not available, using mock data")
        setResult({
          success: true,
          score: 75,
          verdict: "Likely Legitimate",
          explanation:
            "This is a mock result since the backend is not available. In a real scenario, we would analyze this content using the Gemini AI model.",
          factors: [
            { name: "Source Credibility", score: 80, details: "The content appears to come from a reputable source" },
            { name: "Factual Accuracy", score: 70, details: "Most claims appear to be factually accurate" },
            { name: "Balanced Reporting", score: 75, details: "The content presents multiple perspectives" },
            { name: "Sensationalism", score: 85, details: "Low level of sensationalist language" },
            { name: "Citation Quality", score: 90, details: "Good use of citations and references" },
          ],
        })
      }
    } catch (error) {
      console.error("Error analyzing news:", error)
      // Fallback to mock data
      setResult({
        success: true,
        score: 75,
        verdict: "Likely Legitimate",
        explanation:
          "This is a mock result since the backend is not available. In a real scenario, we would analyze this content using the Gemini AI model.",
        factors: [
          { name: "Source Credibility", score: 80, details: "The content appears to come from a reputable source" },
          { name: "Factual Accuracy", score: 70, details: "Most claims appear to be factually accurate" },
          { name: "Balanced Reporting", score: 75, details: "The content presents multiple perspectives" },
          { name: "Sensationalism", score: 85, details: "Low level of sensationalist language" },
          { name: "Citation Quality", score: 90, details: "Good use of citations and references" },
        ],
      })
    } finally {
      setIsAnalyzing(false)
    }
  }

  const getStatusIcon = (score: number) => {
    if (score > 70) return <CheckCircle className="h-6 w-6 text-green-500" />
    if (score > 40) return <AlertTriangle className="h-6 w-6 text-yellow-500" />
    return <AlertCircle className="h-6 w-6 text-red-500" />
  }

  const getStatusColor = (score: number) => {
    if (score > 70) return "bg-green-500"
    if (score > 40) return "bg-yellow-500"
    return "bg-red-500"
  }

  const getStatusText = (score: number) => {
    if (score > 70) return "text-green-500"
    if (score > 40) return "text-yellow-500"
    return "text-red-500"
  }

  const getBadgeVariant = (score: number): "success" | "warning" | "destructive" => {
    if (score > 70) return "success"
    if (score > 40) return "warning"
    return "destructive"
  }

  return (
    <main className="container mx-auto py-8 px-4">
      <h1 className="text-3xl font-bold mb-6">AI-Powered Fake News Detector</h1>

      <Card className="mb-8">
        <CardHeader>
          <CardTitle>Analyze News Content with AI</CardTitle>
          <CardDescription>
            Enter a URL or paste text content to analyze its credibility using Gemini AI
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Tabs defaultValue="url">
            <TabsList className="mb-4">
              <TabsTrigger value="url" className="flex items-center gap-2">
                <LinkIcon className="h-4 w-4" />
                Analyze URL
              </TabsTrigger>
              <TabsTrigger value="text" className="flex items-center gap-2">
                <FileText className="h-4 w-4" />
                Analyze Text
              </TabsTrigger>
            </TabsList>

            <TabsContent value="url">
              <div className="space-y-4">
                <Input placeholder="Enter news article URL" value={url} onChange={(e) => setUrl(e.target.value)} />
                <Button
                  onClick={() => analyzeNews("url")}
                  disabled={!url || isAnalyzing}
                  className="flex items-center gap-2"
                >
                  {isAnalyzing ? (
                    "Analyzing..."
                  ) : (
                    <>
                      <Search className="h-4 w-4" />
                      Analyze URL
                    </>
                  )}
                </Button>
              </div>
            </TabsContent>

            <TabsContent value="text">
              <div className="space-y-4">
                <Textarea
                  placeholder="Paste news article text"
                  value={text}
                  onChange={(e) => setText(e.target.value)}
                  rows={6}
                />
                <Button
                  onClick={() => analyzeNews("text")}
                  disabled={!text || isAnalyzing}
                  className="flex items-center gap-2"
                >
                  {isAnalyzing ? (
                    "Analyzing..."
                  ) : (
                    <>
                      <Search className="h-4 w-4" />
                      Analyze Text
                    </>
                  )}
                </Button>
              </div>
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      {error && (
        <Card className="mb-8 border-red-500">
          <CardContent className="pt-6">
            <div className="flex items-start gap-3">
              <AlertCircle className="h-5 w-5 text-red-500 mt-0.5" />
              <div>
                <h3 className="font-bold text-red-500">Error</h3>
                <p>{error}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      )}

      {isAnalyzing && (
        <Card className="mb-8">
          <CardContent className="pt-6">
            <div className="flex flex-col items-center justify-center py-8">
              <div className="mb-4">
                <div className="animate-spin h-8 w-8 border-4 border-blue-500 border-t-transparent rounded-full"></div>
              </div>
              <h3 className="text-xl font-medium mb-2">Analyzing content with AI...</h3>
              <p className="text-sm text-muted-foreground mb-6 text-center max-w-md">
                Our AI is examining the article for signs of misinformation, checking source credibility, analyzing
                language patterns, and comparing with known facts.
              </p>
              <Progress value={45} className="w-full max-w-md mb-2" />
              <p className="text-xs text-muted-foreground">This may take a few moments</p>
            </div>
          </CardContent>
        </Card>
      )}

      {result && !isAnalyzing && (
        <div className="space-y-6">
          <Card
            className={`border-t-4 ${
              result.score > 70 ? "border-t-green-500" : result.score > 40 ? "border-t-yellow-500" : "border-t-red-500"
            }`}
          >
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>AI Analysis Results</CardTitle>
                <div
                  className={`p-2 rounded-full ${
                    result.score > 70 ? "bg-green-50" : result.score > 40 ? "bg-yellow-50" : "bg-red-50"
                  }`}
                >
                  {getStatusIcon(result.score)}
                </div>
              </div>
              <CardDescription className="flex items-center gap-2">
                <Bot className="h-4 w-4" />
                Analyzed by Gemini AI
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div
                className={`p-6 rounded-lg ${
                  result.score > 70
                    ? "bg-green-50 border-green-200"
                    : result.score > 40
                      ? "bg-yellow-50 border-yellow-200"
                      : "bg-red-50 border-red-200"
                } border`}
              >
                <h3 className={`text-xl font-bold mb-2 ${getStatusText(result.score)}`}>{result.verdict}</h3>
                <p className="text-gray-700 dark:text-gray-300">{result.explanation}</p>
              </div>

              <div>
                <h3 className="text-lg font-medium mb-4 flex items-center">Credibility Score</h3>
                <div className="bg-gray-100 dark:bg-gray-800 p-6 rounded-lg">
                  <div className="flex items-center gap-4 mb-2">
                    <Progress value={result.score} className="flex-1 h-3" />
                    <span className={`text-2xl font-bold ${getStatusText(result.score)}`}>
                      {Math.round(result.score)}%
                    </span>
                  </div>
                  <div className="flex justify-between text-xs text-muted-foreground mt-1">
                    <span>Likely Fake</span>
                    <span>Questionable</span>
                    <span>Likely Legitimate</span>
                  </div>
                </div>
              </div>

              <div>
                <h3 className="text-lg font-medium mb-4">Analysis Factors</h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {result.factors.map((factor, index) => (
                    <Card key={index} className="overflow-hidden">
                      <div className={`h-1 ${getStatusColor(factor.score)}`}></div>
                      <CardContent className="pt-4">
                        <div className="flex justify-between items-center mb-2">
                          <h4 className="font-medium">{factor.name}</h4>
                          <Badge variant={getBadgeVariant(factor.score)}>{Math.round(factor.score)}%</Badge>
                        </div>
                        <Progress value={factor.score} className="h-2 mb-2" />
                        {factor.details && <p className="text-sm text-muted-foreground">{factor.details}</p>}
                      </CardContent>
                    </Card>
                  ))}
                </div>
              </div>

              <div className="bg-blue-50 dark:bg-blue-900 p-4 rounded-lg border border-blue-100 dark:border-blue-800">
                <div className="flex items-start">
                  <Info className="h-5 w-5 text-blue-500 dark:text-blue-400 mt-0.5 mr-3 flex-shrink-0" />
                  <div>
                    <h4 className="font-medium text-blue-800 dark:text-blue-200 mb-1">AI-Powered Analysis</h4>
                    <p className="text-sm text-blue-700 dark:text-blue-300 mb-2">
                      This analysis was performed by Gemini AI, which evaluates content based on multiple factors
                      including source credibility, factual accuracy, and language patterns.
                    </p>
                    <p className="text-sm text-blue-700 dark:text-blue-300">
                      Remember that AI analysis is not perfect. Always verify information with multiple trusted sources.
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
            <CardFooter className="flex justify-between">
              <Button
                variant="outline"
                onClick={() => {
                  setResult(null)
                  setUrl("")
                  setText("")
                }}
              >
                Analyze Another
              </Button>
            </CardFooter>
          </Card>
        </div>
      )}
    </main>
  )
}
