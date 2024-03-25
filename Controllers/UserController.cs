using Azure;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using projectUsers.Data.Context;
using projectUsers.Data.Models;
using projectUsers.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace projectUsers.Controllers {

    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        UsersContext _db;

        public UserController(IConfiguration configuration, UserManager<User> userManager, UsersContext db)
        {
            _configuration = configuration;
            _userManager = userManager;
            _db = db;
        }
        #region login static
        //public ActionResult Login(LoginDTO credntials) //loginDTO==credntials
        //{
        //    if (credntials.Username != "admin" || credntials.Password != "pass")
        //    {
        //        return Unauthorized();
        //    }
        //    var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.GivenName, "Ahmed Mahmoud"),
        //    new Claim("Nationality","Egyptian")
        //};
        //    var secretKey = _configuration.GetValue<string>("secretKey");
        //    var keyInBytes = Encoding.ASCII.GetBytes(secretKey);
        //    var key = new SymmetricSecurityKey(keyInBytes);
        //    var algorithmAndKey = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //    //system.identitymodel.tokens.jwt part
        //    var jwtToken = new JwtSecurityToken(
        //        claims: claims,
        //        signingCredentials: algorithmAndKey,
        //        expires: DateTime.Now.AddMinutes(90)
        //        );
        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    return Ok(tokenHandler.WriteToken(jwtToken));
        //}
        #endregion

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            var EmailExists = _db.Users.Any(x=>x.Email==registerDTO.Email);
            if (EmailExists)
            {
                var error = new
                {
                    ErrorMessage = "Email already exists",
                };
                return Conflict(error);
            }
            var user = new User
            { UserName = registerDTO.Username, 
                Email = registerDTO.Email, 
                Name = registerDTO.Name, 
                Age = registerDTO.Age,
                Role = registerDTO.Role,
                Nationality= registerDTO.Nationality,
                Phone=registerDTO.Phone
            };
            var result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            await _userManager.AddClaimsAsync(user, new List<Claim>
        {
            new Claim("UserId",user.Id),
            new Claim("role",registerDTO.Role),
            new Claim("UserName", registerDTO.Username),
            new Claim("Email", registerDTO.Email),
            new Claim("Name", registerDTO.Name),
            new Claim("Age",registerDTO.Age.ToString() ),
            new Claim("Nationality",registerDTO.Nationality),
            new Claim("Phone",registerDTO.Phone)
        });
            return Ok("User Created");
        }
        #region register an admin
        //[HttpPost]
        //[Route("register")]
        //public async Task<ActionResult> AdminRegister(RegisterDTO registerDTO)
        //{
        //    var user = new User { UserName = registerDTO.Username, Email = registerDTO.Email, Name = registerDTO.Name };
        //    var result = await _userManager.CreateAsync(user, registerDTO.Password);
        //    if (!result.Succeeded)
        //    {
        //        return BadRequest(result.Errors);
        //    }
        //    await _userManager.AddClaimsAsync(user, new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier,user.Id),
        //    new Claim(ClaimTypes.GivenName, "Ahmed Mahmoud"),
        //    new Claim("Nationality","Egyptian")
        //});
        //    return StatusCode(StatusCodes.Status201Created, "User Created");
        //}
        #endregion

        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> Login(LoginDTO credntials)
        {
            //  var requiredUser = await _userManager.FindByNameAsync(credntials.Username) ;
            var requiredEmail = await _userManager.FindByEmailAsync(credntials.Email);
            if (requiredEmail == null)
            {
                var error = new
                {
                    ErrorMessage = "Email or Password is incorrect",
                };
                return Unauthorized(error);
            }
            //check for locking
            var LockedUser = await _userManager.IsLockedOutAsync(requiredEmail);
            var isAuth = await _userManager.CheckPasswordAsync(requiredEmail, credntials.Password);
            //      var userRole = await _userManager.GetRolesAsync(requiredUser);
            
            if (!isAuth)
            {
                        await _userManager.AccessFailedAsync(requiredEmail);

        // Check if the user should be locked out
        if (await _userManager.IsLockedOutAsync(requiredEmail))
        {
            var error1 = new
            {
                ErrorMessage = "User is locked out. Please try again later.",
            };
            return Unauthorized(error1);
        }

        // Otherwise, return an error message
        var error = new
        {
            ErrorMessage = "Email or Password is incorrect",
        };
        return Unauthorized(error);
    }

    // If authentication succeeds, reset the failed login attempt count
    await _userManager.ResetAccessFailedCountAsync(requiredEmail);
            if (LockedUser)
            {
                var error = new
                {
                    ErrorMessage = "User Locked out for 15 minutes for registering 3 wrong times",
                };
                return Unauthorized(error);
            }
            var claims =await _userManager.GetClaimsAsync(requiredEmail);
            var secretKey = _configuration.GetValue<string>("secretKey");
            var keyInBytes = Encoding.ASCII.GetBytes(secretKey);
            var key = new SymmetricSecurityKey(keyInBytes);
            var algorithmAndKey = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expDate = DateTime.Now.AddMinutes(15);
            var Message = "User logged in";
            //system.identitymodel.tokens.jwt part
            var jwtToken = new JwtSecurityToken(
                claims: claims,
                signingCredentials: algorithmAndKey,
                expires: expDate
               // roles : userRole
                );
            var tokenHandler = new JwtSecurityTokenHandler();
            return Ok(new TokenDTO
            {
                Token = tokenHandler.WriteToken(jwtToken),
                Exp = expDate,
                Message = Message
            });
         //   return Ok();
        }
    }
}

