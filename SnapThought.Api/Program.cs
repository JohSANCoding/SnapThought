using Microsoft.EntityFrameworkCore;
using SnapThought.Api.Data;
using SnapThought.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=snapthought.db"));

// Reformatting provider is swappable via config (Llm:Provider). Gemini (hosted) is the
// default; Ollama (local) is kept as an alternate for the local-machine vision. Each is a
// typed HttpClient so the base address / timeout live next to the registration.
var llmProvider = builder.Configuration["Llm:Provider"] ?? "Gemini";
if (llmProvider.Equals("Ollama", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IReformatService, OllamaReformatService>(client =>
    {
        var baseUrl = builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromMinutes(2);
    });
}
else
{
    builder.Services.AddHttpClient<IReformatService, GeminiReformatService>(client =>
    {
        client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
        client.Timeout = TimeSpan.FromMinutes(2);
    });
}

builder.Services.AddScoped<ISnapThoughtService, SnapThoughtService>();

var app = builder.Build();

// Apply migrations on startup (single-machine base build).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
