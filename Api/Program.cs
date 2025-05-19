using Microsoft.EntityFrameworkCore;
using Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<CachingTaskDbContext>(options => {
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

});

// in memory cache
builder.Services.AddMemoryCache();

builder.Services.AddControllers();

// Distributed Redis Cache 
builder.Services.AddStackExchangeRedisCache(opts =>
{
    opts.Configuration = builder.Configuration.GetConnectionString("Redis")
                        ?? "localhost:6379";
    opts.InstanceName = "ResilientCacheDemo:";
});



// HttpClient + Polly policies 
builder.Services.AddHttpClient<ExternalService>()
    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
    .AddPolicyHandler(Policies.GetRetryPolicy())
    .AddPolicyHandler(Policies.GetCircuitBreakerPolicy())
    .AddPolicyHandler(Policies.GetTimeoutPolicy())
    .AddPolicyHandler(Policies.GetFallbackPolicy());



// Register our demo service 
builder.Services.AddScoped<DemoService>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();