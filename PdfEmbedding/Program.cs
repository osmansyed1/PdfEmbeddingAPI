using Microsoft.OpenApi.Models;
using PdfEmbedding.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<PdfProcessingService>();
builder.Services.AddSingleton<EmbeddingService>(provider =>
{
    var apiKey = builder.Configuration["OpenAI:ApiKey"]
        ?? throw new InvalidOperationException("OpenAI API key not found in configuration");
    return new EmbeddingService(apiKey);
});

builder.Services.AddSingleton<StorageService>(provider =>
{
    var storagePath = builder.Configuration["Storage:Path"] ?? "Vectors";
    return new StorageService(storagePath);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PDF Embeddings API",
        Version = "v1",
        Description = "API for processing PDFs and generating embeddings"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PDF Embeddings API V1");
    });
}

app.UseHttpsRedirection();
    
app.UseAuthorization();

app.MapControllers();
var vectorsPath = app.Configuration["Storage:Path"] ?? "Vectors";
if (!Directory.Exists(vectorsPath))
{
    Directory.CreateDirectory(vectorsPath);
}
app.Run();
