using Usuario.Intf.Models;
using Usuario.DbContext.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;
using System.Net.Mail;
using System.Net;
using MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Usuario.Server.Services
{
    public class UsuarioService
    {
        private readonly IMongoCollection<Usuario.Intf.Models.Usuario> _usuariosCollection;
        private readonly IConfiguration _configuration;

        public UsuarioService(MongoDbContext context, IConfiguration configuration)
        {
            _usuariosCollection = context.Usuarios;
            _configuration = configuration;
        }

        // --- MÉTODOS CRUD BÁSICOS ---

        // --- BUCAR LISTA USER ---
        public async Task<List<Usuario.Intf.Models.Usuario>> GetAsync(string token)
        {

            // Validação do Token
            var validationResult = ValidateToken(token, "");

            if (!validationResult.IsSuccess)
                return null;
            else
                return await _usuariosCollection.Find(_ => true).ToListAsync();
        }

        // --- BUCAR USER ID ---
        public async Task<Usuario.Intf.Models.Usuario?> GetAsync(ObjectId id, string token)

        {
            // Validação do Token
            var validationResult = ValidateToken(token, id.ToString());

            if (!validationResult.IsSuccess)
                return null;
            else
                return await _usuariosCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

        }
        // --- BUCAR USER ID ---
        public async Task<Usuario.Intf.Models.Usuario> GetUserLimited(ObjectId id, string token, string option)

        {

            // Validação do Token
            var validationResult = ValidateToken(token, id.ToString(), option);

            if (!validationResult.IsSuccess)
                return null;
            else
                return await _usuariosCollection.Find(x => x.Id == id) // 2. Define a Projeção (SELECT)
        .Project<Usuario.Intf.Models.Usuario>(Builders<Usuario.Intf.Models.Usuario>.Projection
            .Include(u => u.Name)
            .Include(u => u.Foto)
            .Include(u => u.Id)
        )
        // 3. Executa a consulta
        .FirstOrDefaultAsync();

        

        }
        // --- BUCAR USER Name ---
        public async Task<List<Usuario.Intf.Models.Usuario>> GetListNameAsync(string nome)

        {

            var teste = await _usuariosCollection.Find(_ => true).ToListAsync();
            var resultados = await _usuariosCollection
        // 1. Define o filtro (WHERE/Find)
        .Find(x => x.Name.ToUpper().Contains(nome.ToUpper()))

        // 2. Define a Projeção (SELECT)
        .Project<Usuario.Intf.Models.Usuario>(Builders<Usuario.Intf.Models.Usuario>.Projection
            .Include(u => u.Name)
            .Include(u => u.Email)
            .Include(u => u.Foto)
            .Include(u => u.Id)
        )
        // 3. Executa a consulta
        .ToListAsync();

            return resultados;
        }


        public async Task<Usuario.Intf.Models.Usuario?> FindUser(string email)
        {
            var filter = Builders<Usuario.Intf.Models.Usuario>.Filter.Eq(u => u.Email, email);
            var usuario = await _usuariosCollection.Find(filter).FirstOrDefaultAsync();


            return usuario;
        }

        // --- CREATE ---
        public async Task<Usuario.Intf.Models.Usuario> CreateAsync(UsuarioDto newUsuario)
        {

            var novoUsuario = new Usuario.Intf.Models.Usuario
            {
                Name = newUsuario.Name,
                Sobrenome = newUsuario.Sobrenome,
                Email = newUsuario.Email,
                Password = newUsuario.Password
            };
            await _usuariosCollection.InsertOneAsync(novoUsuario);
            return novoUsuario;
        }

        // --- UPDATE ---
        public async Task<ServiceResult> UpdateAsync(ObjectId id, Usuario.Intf.Models.Usuario updatedUsuario, string token)
        {
            // Validação do Token
            var validationResult = ValidateToken(token, id.ToString());

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }
            await _usuariosCollection.ReplaceOneAsync(x => x.Id == id, updatedUsuario);
            return ServiceResult.Success("Update Realizado com sucesso");
        }


        // --- DELETE ---

        public async Task<ServiceResult> DeleteAsync(ObjectId id, string token)
        {
            // Validação do Token
            var validationResult = ValidateToken(token, id.ToString());

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }
            await _usuariosCollection.DeleteOneAsync(x => x.Id == id);
            return ServiceResult.Success("Delete Realizado com sucesso");

        }

        // --- GERAÇÃO DE JWT ---
        public string GenerateJwtToken(Usuario.Intf.Models.Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Key"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key não configurada no app settings.");
            }

            var key = Encoding.ASCII.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
             {
     new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
     new Claim(ClaimTypes.Role, usuario.Tipo)
    }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // --- ENVIO DE E-MAIL ---
        private async Task<ServiceResult> SendEmailAsync(string toEmail, string subject, string body)
        {
            try
            {
                // Busca as configurações de e-mail do appsettings.json
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";

                // Conversão segura do porto
                if (!int.TryParse(_configuration["EmailSettings:SmtpPort"], out int smtpPort))
                {
                    smtpPort = 587; // Padrão TLS
                }

                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    return ServiceResult.Failure("Erro de configuração do servidor de e-mail. Verifique 'EmailSettings'.", 500);
                }

                var smtpUsername = senderEmail;

                using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                {
                    smtpClient.EnableSsl = true;
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, senderPassword);

                    var emailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, "Revista Brasileira da Educação Básica"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };
                    emailMessage.To.Add(toEmail);

                    await smtpClient.SendMailAsync(emailMessage);
                }

                return ServiceResult.Success("E-mail enviado com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar e-mail: {ex.Message}");
                return ServiceResult.Failure($"Não foi possível enviar o e-mail de recuperação. Falha de autenticação/conexão: {ex.Message}", 500);
            }
        }

        // --- RECUPERAÇÃO DE SENHA ---
        public async Task<ServiceResult> RequestPasswordRecoveryAsync(string? email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return ServiceResult.Success("Se o e-mail estiver cadastrado, um link de recuperação foi enviado.");
            }

            var usuario = await FindUser(email);

            if (usuario is null)
            {
                return ServiceResult.Success("Se o e-mail estiver cadastrado, um link de recuperação foi enviado.");
            }

            // Gera um token de vida curta (60 minutos) específico para recuperação de senha
            var recoveryToken = GenerateRecoveryJwtToken(usuario);
            var frontendBaseUrl = _configuration["FrontendUrl"];

            if (string.IsNullOrEmpty(frontendBaseUrl))
            {
                return ServiceResult.Failure("Erro de configuração: 'FrontendUrl' não definida.", 500);
            }

            // CONSTRUÇÃO DO LINK: ID + TOKEN
            string recoveryLink = $"{frontendBaseUrl}/alterandoSenha?id={usuario.Id}&token={recoveryToken}";

            string emailBody = $@"
                <p>Olá, {usuario.Name},</p>
                <p>Recebemos uma solicitação para redefinir a senha de acesso à Revista Brasileira da Educação Básica.</p>
                <p>Para redefinir sua senha, clique no link abaixo. Este link expirará em 60 minutos:</p>
                <p><a href='{recoveryLink}' style='padding: 10px 20px; background-color: #007bff; color: white; text-decoration: none; border-radius: 5px; display: inline-block;'>Redefinir Senha</a></p>
                <p>Se você não solicitou esta redefinição, por favor, ignore este e-mail.</p>
            ";

            var emailResult = await SendEmailAsync(email, "Redefinição de Senha de Acesso", emailBody);

            return emailResult.IsSuccess
             ? ServiceResult.Success("Se o e-mail estiver cadastrado, um link de recuperação foi enviado.")
             : emailResult;
        }

        // Método auxiliar para gerar um JWT com 1 hora de duração para recuperação
        private string GenerateRecoveryJwtToken(Usuario.Intf.Models.Usuario usuario)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Key"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key não configurada no app settings.");
            }

            var key = Encoding.ASCII.GetBytes(jwtKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
             {
     new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
     new Claim("TokenType", "PasswordReset")
    }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // --- REDEFINIÇÃO DE SENHA ---
        public async Task<ServiceResult> ResetPasswordAsync(ObjectId userId, string? token, string? newPassword)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(newPassword))
            {
                return ServiceResult.Failure("Token e nova senha são obrigatórios.", 400);
            }

            // 1. Validação do Token
            var validationResult = ValidateRecoveryToken(token, userId.ToString());

            if (!validationResult.IsSuccess)
            {
                return validationResult;
            }

            // 2. Localiza o usuário
            var usuario = await GetAsync(userId, token);
            if (usuario is null)
            {
                // Se o usuário não for encontrado, tratamos como token inválido por segurança
                return ServiceResult.Failure("Token inválido ou expirado.", 401);
            }

            // 3. Aplica o Hash e Atualiza a Senha
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
            usuario.Password = hashedPassword;

            await UpdateAsync(userId, usuario, token);

            return ServiceResult.Success("Senha redefinida com sucesso!", 200);
        }

        // Método auxiliar para validar se o JWT é válido e para o usuário correto
        private ServiceResult ValidateRecoveryToken(string token, string expectedUserId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Key"];
            var key = Encoding.ASCII.GetBytes(jwtKey!);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                if (userIdClaim != expectedUserId)
                {
                    return ServiceResult.Failure("Token inválido para este usuário.", 401);
                }

                var tokenTypeClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "TokenType")?.Value;
                if (tokenTypeClaim != "PasswordReset")
                {
                    return ServiceResult.Failure("Token não é um token de redefinição de senha.", 401);
                }

                return ServiceResult.Success();
            }
            catch (SecurityTokenExpiredException)
            {
                return ServiceResult.Failure("Token de recuperação expirado.", 401);
            }
            catch (Exception)
            {
                return ServiceResult.Failure("Token de recuperação inválido.", 401);
            }
        }
        private ServiceResult ValidateToken(string token, string expectedUserId, string option = "0")
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Key"];
            var key = Encoding.ASCII.GetBytes(jwtKey!);

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
                if (!string.IsNullOrEmpty(expectedUserId) && userIdClaim != expectedUserId && role != "0" && option != "42")
                {
                    return ServiceResult.Failure("Token inválido para este usuário.", 401);
                }

                return ServiceResult.Success();
            }
            catch (SecurityTokenExpiredException)
            {
                return ServiceResult.Failure("Token de recuperação expirado.", 401);
            }
            catch (Exception)
            {
                return ServiceResult.Failure("Token de recuperação inválido.", 401);
            }
        }
    }
}