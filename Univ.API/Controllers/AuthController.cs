using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Univ.Core.Entities;
using Univ.Service.Dtos.UserDtos;
using Univ.Service.Services.Interfaces;

namespace Univ.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public AuthController(IAuthService authService, RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _authService = authService;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        //[HttpGet("users")]
        //public async Task<IActionResult> CreateUser()
        //{

        //    await _roleManager.CreateAsync(new IdentityRole("Admin"));
        //    await _roleManager.CreateAsync(new IdentityRole("Member"));


        //    AppUser admin = new AppUser
        //    {
        //        Fullname = "Admin",
        //        UserName = "admin",
        //    };

        //    await _userManager.CreateAsync(admin, "Admin123");

        //    AppUser member = new AppUser
        //    {
        //        Fullname = "Member1",
        //        UserName = "member1",
        //    };

        //    await _userManager.CreateAsync(member, "Member123");

        //    await _userManager.AddToRoleAsync(admin, "Admin");
        //    await _userManager.AddToRoleAsync(member, "Member");

        //    return Ok(admin.Id);
        //}


        [HttpPost("login")]
        public ActionResult Login(UserLoginDto loginDto)
        {
            var token = _authService.Login(loginDto);
            return Ok(new { token });
        }


        [Authorize]
        [HttpGet("profile")]
        public ActionResult Profile()
        {
            return Ok(User.Identity.Name);
        }
    }
}
