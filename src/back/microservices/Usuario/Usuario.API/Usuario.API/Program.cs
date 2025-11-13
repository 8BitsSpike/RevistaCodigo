using Usuario.DbContext.Persistence;
using Usuario.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<UsuarioDatabaseSettings>(builder.Configuration.GetSection("UsuarioDatabase"));
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<UsuarioService>();
builder.Services.AddControllers();

// Configuração do Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuração do JWT
var jwtKey = builder.Configuration["Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new InvalidOperationException("JWT Key not configured in app settings. Please check 'Key' in appsettings.json.");
}
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        // Tornando as validações mais explícitas
        ValidateIssuer = false,
        ValidateAudience = false,

        // Valida se a chave usada para assinar o token é a mesma que temos (a Key)
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),

        // Garante que o token não expirou.
        ValidateLifetime = true,

        // Diz ao sistema para usar o campo "sub" (NameIdentifier) como o ID principal.
        NameClaimType = ClaimTypes.NameIdentifier
    };

});

// Configuração do CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
    policy =>
    {
        policy.AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod();
    });
});

var app = builder.Build();

// Ambiente de desenvolvimento: habilita Swagger e endpoint raiz
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // Interface gráfica do Swagger

    app.MapGet("/", () => Results.Ok("API de Usuário está rodando"));

}

app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();