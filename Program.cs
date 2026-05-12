using FakeNewsDetector.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Fake News Detector API", Version = "v1" });
});

// Add HTTP client
builder.Services.AddHttpClient();

// Register our services
builder.Services.AddSingleton<INewsAnalyzerService, NewsAnalyzerService>();
builder.Services.AddSingleton<ISavedAnalysisService, SavedAnalysisService>();

// Add CORS with a more permissive policy for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add SPA static files
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/out";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Fake News Detector API v1"));
    app.UseDeveloperExceptionPage();
}

// Apply CORS before any other middleware that might handle responses
app.UseCors("AllowAll");

app.UseHttpsRedirection();
app.UseAuthorization();

// Use static files and SPA static files
app.UseStaticFiles();
app.UseSpaStaticFiles();

// Map controllers before the SPA fallback
app.MapControllers();

// Map the API routes
app.MapControllerRoute(
    name: "default",
    pattern: "api/{controller}/{action=Index}/{id?}");

// Serve the SPA
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    
    if (app.Environment.IsDevelopment())
    {
        // In development, proxy requests to the Next.js dev server
        // Comment this out if you're running the Next.js app separately
        // spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
    }
});

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application started. API should be available at:");
logger.LogInformation("http://localhost:5000/api/Analysis");
logger.LogInformation("https://localhost:7126/api/Analysis");

app.Run();
