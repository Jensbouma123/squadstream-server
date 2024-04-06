using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Web_API_new.Data;
using Web_API_new.DTOs;
using Web_API_new.Models;
using QRCoder;
using Serilog;
using TwoFactorAuthNet;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Web_API_new.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    
    private readonly UserManager<UserModel> _userManager;
    private readonly SignInManager<UserModel> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly DataContext _context;
    
    public AuthController(UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IConfiguration configuration, DataContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }
    
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO model)
    {
        if (ModelState.IsValid)
        {
            var userExists = await _userManager.FindByNameAsync(model.Email);
            if (userExists != null)
            {
                Log.Error("Gebruiker bestaat al.");

                return BadRequest(new { status = 500, message = "Gebruiker bestaat al." });
            }

            UserModel user = new UserModel()
            {
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                Log.Error("Gebruiker kon niet worden aangemaakt.");
                return BadRequest(new { status = 500, message = "Gebruiker kon niet worden aangemaakt." });
            }

            Log.Information("Gebruiker is aangemaakt.");

            return Ok(new { status = 200, message = "Gebruiker is aangemaakt." });
        }
        Log.Error("Gebruiker kon niet worden aangemaakt omdat de gegevens niet gevalideerd konden worden.");
        return BadRequest(new { status = 200, message = "Gebruiker kon niet worden aangemaakt." });
    }
    
        
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO model)
    {
        var user = await _userManager.FindByNameAsync(model.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
        {
            
            if (user.TwoFactorEnabled)
            {
                if (model.TwoFaCode.IsNullOrEmpty())
                {
                    Log.Error("2FA is aangezet voor gebruiker.");
                    return Ok(new { status = 200, message = "2FA enabled", twoFaEnabled = true });
                }
                else
                {
                    var tfa = new TwoFactorAuth();
                    if (!tfa.VerifyCode(user.TwoFactorSecretKey, model.TwoFaCode))
                    {
                        Log.Error("2FA code is niet geldig.");
                        return BadRequest("Invalid two-factor code: " + model.TwoFaCode);
                    }
                }
            }
            
            var userRoles = await _userManager.GetRolesAsync(user);

            if (!userRoles.Contains("teamlid"))
            {
                await _userManager.AddToRoleAsync(user, "teamlid");
            }

            var isTeamLeader = await _context.Teams.AnyAsync(t => t.LeaderId == user.Id);
            if (isTeamLeader && !userRoles.Contains("teamleider"))
            {
                await _userManager.RemoveFromRoleAsync(user, "teamlid");
                await _userManager.AddToRoleAsync(user, "teamleider");
            }
            

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims);
            
            var options = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Domain = "locahost:3000",
                Expires = DateTime.UtcNow.AddHours(3)
            };

            var userData = new
            {
                id = user.Id,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                twoFaEnabled = user.TwoFactorEnabled,
            };

            Response.Cookies.Append("squadstream-acc-token", new JwtSecurityTokenHandler().WriteToken(token), options);
            
            Log.Information("Gebruiker is ingelogd.");
            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo,
                user = userData,
                roles = userRoles
            });
        }
        Log.Error("Poging tot inloggen mislukt.");
        return Unauthorized();
    }
    
    [HttpGet("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        Response.Cookies.Delete("squadstream-acc-token");
        Log.Error("Gebruiker is uitgelogd");
        return Ok("Loggout");
    }
    
    [HttpGet("user")]
    [Authorize]
    public async Task<IActionResult> GetUser()
    {
        var nameClaim = GetUserNameFromToken();
        var name = nameClaim;
        if (nameClaim == null)
        {
            Log.Error("Gebruiker kon niet worden gevonden van token.");
            return BadRequest("Something went wrong");
        }
        
        var user = await _userManager.FindByNameAsync(name);
        if (user == null)
        {
            Log.Error("Gebruiker kon niet worden gevonden.");
            return BadRequest("Something went wrong.");
        }
        
        var userData = new
        {
            id = user.Id,
            email = user.Email,
            firstName = user.FirstName,
            lastName = user.LastName,
            twoFaEnabled = user.TwoFactorEnabled,
        };
        
        Log.Information("Informatie van gebruiker is opgevraagd.");
        return Ok(userData);
    }

    [HttpGet("team")]
    [Authorize]
    public async Task<IActionResult> GetTeam()
    {
        var userName = GetUserNameFromToken();
        
        if (userName == null)
        {
            Log.Error("Gebruiker kon niet worden gevonden.");
            return BadRequest("Er is iets mis gegaan.");
        }

        var user = await _context.Users
            .Include(u => u.Team)
            .ThenInclude(t => t.Leader) 
            .Include(u => u.Team)
            .ThenInclude(t => t.Players) 
            .FirstOrDefaultAsync(u => u.Email == userName);

        if (user != null)
        {
            await _context.Entry(user).Reference(u => u.Team).LoadAsync();
        }
        else
        {
            Log.Error("Gebruiker kon niet worden gevonden.");
            return BadRequest("Gebruiker kon niet gevonden worden.");
        }   

        if (user.Team == null)
        {
            Log.Error("Speler zit niet in een team.");
            return BadRequest("User is not associated with any team.");
        }
        
        var leader = user.Team.Leader;

        var leaderData = new
        {
            Id = leader.Id,
            FirstName = leader.FirstName,
            LastName = leader.LastName,
        };

        var teamData = new
        {
            Name = user.Team.Name,
            Leader = leaderData,
            Players = user.Team.Players.Select(p => new
            {
                p.FirstName,
                p.LastName,
                p.Id
            }),
        };

        Log.Information("Team gegevens opgevraagd.");
        return Ok(teamData);
    }


    private JwtSecurityToken GetToken(List<Claim> authClaims)
    {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return token;
    }
    
    
    private string GetUserNameFromToken()
    {
        var jwtToken = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(jwtToken) as JwtSecurityToken;

        if (jsonToken == null)
            return null;
        
                
        var expClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp);
        if (expClaim == null)
        {
            return null;
        }

        if (!long.TryParse(expClaim.Value, out long expUnix))
        {
            return null;
        }

        var expirationDate = DateTimeOffset.FromUnixTimeSeconds(expUnix).DateTime;

        if (expirationDate <= DateTime.UtcNow)
        {
            return null;
        }


        var nameClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name");
        return nameClaim?.Value;
    }


    [HttpGet("2fa-qrcode")]
    [Authorize]
    public async Task<IActionResult> GenerateQRCodeTwoFactor()
    {
        var userName = GetUserNameFromToken();

        if (userName == null)
        {
            Log.Error("Gebruiker kon niet worden gevonden.");
            return BadRequest("Unable to retrieve username from token.");
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            Log.Error("Gebruiker kon niet worden gevonden.");
            return BadRequest("Something went wrong.");
        }

        if (user.TwoFactorEnabled)
        {
            Log.Error("2FA staat al aan voor deze gebruiker.");
            return BadRequest("Two-factor authentication is already enabled for this user.");
        }

        var tfa = new TwoFactorAuth("SquadStream");
        var secret = tfa.CreateSecret(160);

        user.TwoFactorSecretKey = secret;
        
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            Log.Error("er is iets mis gegaan bij het aanzetten van 2FA.");
            return BadRequest(
                new { status = 500, message = "Failed to enable two-factor authentication for the user." });
        }


        var authenticatorUri = $"otpauth://totp/{Uri.EscapeDataString("SquadStream")}:{Uri.EscapeDataString(user.Email)}?secret={user.TwoFactorSecretKey}&issuer={Uri.EscapeDataString("SquadStream")}";

        Log.Information("URL voor QRcode is gegenereerd.");
        return Ok(new { status = 200, message = "QR code generated succesfully", url = authenticatorUri });

    }
    
    [HttpGet("enable-2fa")]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactorAuthentication()
    {
        var userName = GetUserNameFromToken();

        if (userName == null)
        {
            Log.Error("Gebruiker kon niet worden gevonden.");
            return BadRequest("Unable to retrieve username from token.");
        }

        var user = await _userManager.FindByNameAsync(userName);
        if (user == null)
        {
            Log.Error("Gebruiker kon niet worden gevonden.");
            return BadRequest("Something went wrong.");
        }

        if (user.TwoFactorEnabled)
        {
            Log.Error("2FA staat al aan voor deze gebruiker.");
            return BadRequest("Two-factor authentication is already enabled for this user.");
        }


        user.TwoFactorEnabled = true;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            Log.Error("2FA kon niet worden aangezet voor deze gebruiker.");
            return BadRequest(
                new { status = 500, message = "Failed to enable two-factor authentication for the user." });
        }

        Log.Information("2FA is aangezet voor deze gebruiker.");
        return Ok(new { status = 200, message = "Two-factor authentication enabled successfully." });
    }


}