using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using VideoGuide.Data;
//using PhoneLogApi.View_Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VideoGuide.View_Model;

namespace VideoGuide.Services
{
    public class AuthManager : IAuthManager
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        //private ApplicationUser _user;
        public AuthManager(UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
            //_user = user;
        }
        public async Task<string> CreateToken(ApplicationUser user)
        {
            var signingCredentials = GetSigningCredentials();
            var claims = await GetClaims(user);
            var token = GenerateTokenOptions(signingCredentials, claims);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        private SigningCredentials GetSigningCredentials()
        {
            var key = "ffc632ce-0053-4bab-8077-93a4d14caaad";
            var secret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }
        private async Task<List<Claim>> GetClaims(ApplicationUser user)
        {
            var claims = new List<Claim>
             {
                 new Claim(ClaimTypes.Name, user.UserName)
             };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var expiration =
            //DateTime.UtcNow.AddSeconds(10);
            DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(jwtSettings["Lifetime"]));
            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            return token;
        }
        public async Task<bool> ValidateUser(LoginUserDTO userDTO)
        {
            var user = await _userManager.FindByNameAsync(userDTO.UserName);
            var validPassword = await _userManager.CheckPasswordAsync(user, userDTO.Password);
            await CountAccessFailedAsync(user,user != null && validPassword);
            return (user != null && validPassword);
        }
        public async Task<ApplicationUser> GetUser(LoginUserDTO userDTO)
        {
            var user = await _userManager.FindByNameAsync(userDTO.UserName);
            return user;
        }
        public async Task CountAccessFailedAsync(ApplicationUser user, bool Validation)
        {
            if (Validation)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }
            else if (user != null)
            {
                await _userManager.AccessFailedAsync(user);
            }
        }

        public async Task<bool> ActiveUser(LoginUserDTO userDTO)
        {
            var user = await _userManager.FindByNameAsync(userDTO.UserName);
            return (user?.LockoutEnabled?? false);
        }
    }
}
