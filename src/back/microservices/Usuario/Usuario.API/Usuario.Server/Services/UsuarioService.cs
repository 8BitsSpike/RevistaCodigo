using Usuario.Intf.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Usuario.DbContext.Persistence;
using BCrypt.Net;
using Microsoft.IdentityModel.Tokens;

namespace Usuario.Server.Services
{
    public class UsuarioService(MongoDbContext context, IConfiguration configuration)
    {
        private readonly MongoDbContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        public virtual async Task<List<Usuar>> GetAsync() =>
            await _context.Usuarios.Find(_ => true).ToListAsync();

        public virtual async Task<Usuar?> GetAsync(ObjectId id) =>
            await _context.Usuarios.Find(x => x.Id == id).FirstOrDefaultAsync();

        public virtual async Task<Usuar?> AuthAsync(string email) =>
           await _context.Usuarios.Find(x => x.Email == email).FirstOrDefaultAsync();

        public virtual async Task UpdateAsync(ObjectId id, Usuar updatedUsuar) =>
            await _context.Usuarios.ReplaceOneAsync(x => x.Id == id, updatedUsuar);

        public virtual async Task<Usuar?> CreateAsync(UsuarDto newUsuar)
        {
            try
            {
                Usuar novo = new()
                {
                    Name = newUsuar.Name,
                    Email = newUsuar.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(newUsuar.Password)
                };
                await _context.Usuarios.InsertOneAsync(novo);
                return novo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public virtual async Task DeleteAsync(ObjectId id) =>
            await _context.Usuarios.DeleteOneAsync(x => x.Id == id);

        public virtual string GenerateJwtToken(Usuar model)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var keyString = _configuration["Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                throw new InvalidOperationException("JWT Key not configured in app settings.");
            }
            var key = Encoding.ASCII.GetBytes(keyString);

            var claims = new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.NameIdentifier,
                             model.Id.ToString()),
                new(ClaimTypes.Email,
                             model.Email.ToString())
            });

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}