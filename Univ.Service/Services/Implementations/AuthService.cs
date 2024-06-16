using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Univ.Core.Entities;
using Univ.Service.Dtos.UserDtos;
using Univ.Service.Exceptions;
using Univ.Service.Services.Interfaces;

namespace Univ.Service.Services.Implementations
{
    public class AuthService:IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<AppUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        public string Login(UserLoginDto loginDto)
        {
            AppUser? user = _userManager.FindByNameAsync(loginDto.Username).Result;

            if (user == null || !_userManager.CheckPasswordAsync(user, loginDto.Password).Result) throw new RestException(StatusCodes.Status401Unauthorized, "UserName or Password incorrect!");

            List<Claim> claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim("Fullname", user.Fullname));

            var roles = _userManager.GetRolesAsync(user).Result;

            claims.AddRange(roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList());

            string secret = _configuration.GetSection("JWT:Secret").Value;

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                signingCredentials: creds,
                issuer: _configuration.GetSection("JWT:Issuer").Value,
                audience: _configuration.GetSection("JWT:Audience").Value,
                expires: DateTime.Now.AddDays(3)
                );

            string tokenStr = new JwtSecurityTokenHandler().WriteToken(token);

            return tokenStr;
        }
    }
}
