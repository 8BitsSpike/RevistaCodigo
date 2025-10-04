using Media.DbContext;
using Media.DbContext.Persistence;
using Media.Server.Services;
using Microsoft.AspNetCore.Builder;
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
builder.Services.AddOpenApi();


var app = builder.Build();

app.UseCors("AllowAllOrigins");

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI(options =>
    {

        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Media.API v1");
    });
}

app.UseAuthorization();

app.MapControllers();

app.Run();
