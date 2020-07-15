using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.DTOs;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IMapper _mapper;
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        public AuthController(IConfiguration config, IMapper mapper, UserManager<User> userManager,
        SignInManager<User> signInManager)
        {
            _config = config;
            _mapper = mapper;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDTO userForRegisterDTO)
        {
            var userCreate = _mapper.Map<User>(userForRegisterDTO);

            var result = await _userManager.CreateAsync(userCreate, userForRegisterDTO.Password);

            var userToReturn = _mapper.Map<UserForDetailDto>(userCreate);

            if (result.Succeeded)
            {
                return CreatedAtAction("GetUser", new { controller = "Users", id = userCreate.Id }, userToReturn);
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserForLoginDTO userForLoginDTO)
        {
            var user = await _userManager.FindByNameAsync(userForLoginDTO.UserName);

            var result = await _signInManager.CheckPasswordSignInAsync(user, userForLoginDTO.Password, false);

            if (result.Succeeded)
            {
                var appUser = _mapper.Map<UserForListDto>(user);

                return Ok(new
                {
                    token = GenerateJWTToken(user),
                    user = appUser
                });
            }
            return Unauthorized();
        }

        private async Task<string> GenerateJWTToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var roles = await _userManager.GetRolesAsync(user);

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8
                    .GetBytes(_config.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDecriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDecriptor);

            return tokenHandler.WriteToken(token);
        }

    }
}