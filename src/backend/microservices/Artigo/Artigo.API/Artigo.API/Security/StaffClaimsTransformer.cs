using Artigo.Intf.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Artigo.API.Security
{
    /// <sumario>
    /// Transforma o ClaimsPrincipal, adicionando a função interna (FuncaoTrabalho)
    /// do usuário autenticado como uma 'Claim' de Role (FuncaoTrabalho).
    /// </sumario>
    public class StaffClaimsTransformer : IClaimsTransformation
    {
        private readonly IStaffRepository _staffRepository;

        public StaffClaimsTransformer(IStaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // O pipeline de autenticação já validou o token, garantindo que o 'sub' (UsuarioId) exista.
            var userId = principal.FindFirstValue("sub");

            if (string.IsNullOrEmpty(userId))
            {
                return principal;
            }

            var staff = await _staffRepository.GetByUsuarioIdAsync(userId);

            // Se o usuário for um Staff ativo, adiciona a FuncaoTrabalho como Claim de Role.
            if (staff != null && staff.IsActive)
            {
                var identity = principal.Identity as ClaimsIdentity;
                if (identity == null)
                {
                    identity = new ClaimsIdentity(principal.Identity);
                }

                // Remove roles antigas para evitar duplicação ou conflito
                var existingRoleClaims = identity.FindAll(ClaimTypes.Role).ToList();
                foreach (var claim in existingRoleClaims)
                {
                    identity.RemoveClaim(claim);
                }

                // Adiciona a função de trabalho como uma claim de "Role"
                identity.AddClaim(new Claim(ClaimTypes.Role, staff.Job.ToString()));

                return new ClaimsPrincipal(identity);
            }

            return principal;
        }
    }
}