using MangoFusionApi.Models;
using MangoFusionApi.Models.Dto;
using MangoFusionApi.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace MangoFusionApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApiResponse response;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly string secretKey;
        public AuthController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.response = new ApiResponse();
            this.userManager = userManager;
            this.roleManager = roleManager;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret")??"";
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser newUser = new ApplicationUser
                {
                    Email = model.Email,
                    UserName = model.Email,
                    Name = model.Name,
                    NormalizedEmail = model.Email.ToUpper()
                };

                var result = await userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    if (!roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                    {
                        await roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                        await roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                    }

                    if (model.Role.Equals(SD.Role_Admin, StringComparison.CurrentCultureIgnoreCase))
                    {
                        await userManager.AddToRoleAsync(newUser, SD.Role_Admin);
                    }
                    else
                    {
                        await userManager.AddToRoleAsync(newUser, SD.Role_Customer);
                    }

                        response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    return Ok(response);
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        response.ErrorMessages.Add(error.Description);
                    }

                    response.StatusCode=HttpStatusCode.BadRequest;
                    response.IsSuccess = false;
                    return BadRequest(response);
                }
            }
            else
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                foreach (var error in ModelState.Values)
                {
                    foreach (var item in error.Errors)
                    {
                        response.ErrorMessages.Add(item.ErrorMessage);
                    }
                }
            }

            response.StatusCode = HttpStatusCode.BadRequest;
            response.IsSuccess = false;
            return BadRequest(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (ModelState.IsValid)
            {
                var userFromDb = await userManager.FindByEmailAsync(model.Email);
                if (userFromDb != null)
                {
                    bool isValid = await userManager.CheckPasswordAsync(userFromDb, model.Password);
                    if (!isValid)
                    {
                        response.Result = new LoginResponseDto();
                        response.StatusCode = HttpStatusCode.BadRequest;
                        response.IsSuccess = false;
                        response.ErrorMessages.Add("Invalid credentials");
                    }

                    // Generate JWT Token
                    JwtSecurityTokenHandler tokenHandler = new();
                    byte[] key = Encoding.ASCII.GetBytes(secretKey);

                    SecurityTokenDescriptor tokenDescriptor = new()
                    {
                        Subject = new ClaimsIdentity(
                            [
                                new ("fullname", userFromDb.Name),
                                new ("id", userFromDb.Id),
                                new (ClaimTypes.Email, userFromDb.Email!.ToString()),
                                new (ClaimTypes.Role, userManager.GetRolesAsync(userFromDb).Result.FirstOrDefault()!)
                            ]),
                        Expires = DateTime.UtcNow.AddDays(7),
                        SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };

                    SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

                    LoginResponseDto loginResponseDto = new LoginResponseDto
                    { 
                        Email = userFromDb.Email,
                        Token = tokenHandler.WriteToken(token),
                        Role = userManager.GetRolesAsync(userFromDb).Result.FirstOrDefault()!
                    };

                    response.Result = loginResponseDto;
                    response.StatusCode = HttpStatusCode.OK;
                    response.IsSuccess = true;
                    return Ok(response);
                }

                response.Result = new LoginResponseDto();
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                response.ErrorMessages.Add("Invalid credentials");
            }
            else
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                response.IsSuccess = false;
                foreach (var error in ModelState.Values)
                {
                    foreach (var item in error.Errors)
                    {
                        response.ErrorMessages.Add(item.ErrorMessage);
                    }
                }
            }

            response.StatusCode = HttpStatusCode.BadRequest;
            response.IsSuccess = false;
            return BadRequest(response);
        }
    }
}
