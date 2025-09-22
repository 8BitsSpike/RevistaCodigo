using Usuario.DbContext;
using Usuario.Intf.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Usuario.DbContext;

namespace Usuario.Server.Services
{
    public class UsuarioService
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        public UsuarioService(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public virtual async Task<List<Usuar>> GetAsync() =>
            await _context.Usuarios.Find(_ => true).ToListAsync();

        public virtual async Task<Usuar?> GetAsync(ObjectId id) =>
            await _context.Usuarios.Find(x => x.Id == id).FirstOrDefaultAsync();

        public virtual async Task<Usuar?> AuthAsync(string email) =>
           await _context.Usuarios.Find(x => x.Email == email).FirstOrDefaultAsync();

        public virtual async Task UpdateAsync(ObjectId id, Usuar updatedUsuar) =>
            await _context.Usuarios.ReplaceOneAsync(x => x.Id == id, updatedUsuar);

        public virtual async Task<Usuar?> CreateAsync(UsuarioDto newUsuar)
        {
            try
            {
                Usuar novo = new Usuar()
                {
                    Name = newUsuar.Name,
                    Email = newUsuar.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(newUsuar.Password)
                };
                await _context.Usuarios.InsertOneAsync(novo);
                return novo;
            }
            catch (System.Exception)
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
                new Claim(ClaimTypes.NameIdentifier,
                          model.Id.ToString()),
                new Claim(ClaimTypes.Email,
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
