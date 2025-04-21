using AIAgentCryptoTrading.Backtesting;
using AIAgentCryptoTrading.Core.Interfaces;
using AIAgentCryptoTrading.DataCollector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder.WithOrigins("http://localhost:3000")
                         .AllowAnyMethod()
                         .AllowAnyHeader());
});

// Register services
builder.Services.AddSingleton<IDataCollector, CryptoDataCollector>();
builder.Services.AddScoped<IBacktester, SimpleBacktester>();

// Add HTTP client
builder.Services.AddHttpClient();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//  CORS middleware 
app.UseCors("AllowReactApp");

app.UseAuthorization();

app.MapControllers();
// Redirect root to Swagger UI
app.MapGet("/", context => {
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.Run();