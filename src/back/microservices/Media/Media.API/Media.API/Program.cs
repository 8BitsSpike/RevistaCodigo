using Media.DbContext;
using Media.DbContext.Persistence;
using Media.Server.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLogging();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});
builder.Services.Configure<MediaDatabaseSettings>(
    builder.Configuration.GetSection("MediaDatabase"));

builder.Services.AddSingleton(sp =>
{
    var settingsOptions = sp.GetRequiredService<IOptions<MediaDatabaseSettings>>();
var logger = sp.GetRequiredService<ILogger<MongoDbContext>>();
return new MongoDbContext(settingsOptions, logger);
});
builder.Services.AddScoped<MediaService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowAllOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapGet("/", () => Results.Ok("API Midia está rodando e aguarda seu comando!"));
}

app.UseAuthorization();

app.MapControllers();

app.Run();
