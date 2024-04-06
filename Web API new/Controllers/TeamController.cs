    using System.ComponentModel.DataAnnotations;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Security.Claims;
    using System.Text;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http.HttpResults;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Serilog;
    using Web_API_new.Data;
    using Web_API_new.DTOs;
    using Web_API_new.Models;

    namespace Web_API_new.Controllers;

    [ApiController]
    [Route("/api/team")]
    public class TeamController : ControllerBase
    {

        public TeamController()
        {
            
        }
        
        [HttpGet("getTeams")]
        public async Task<IActionResult> GetTeams([FromServices] DataContext context)
        {
            List<TeamModel> teams = await context.Teams.Include(t => t.Players).ToListAsync();
            List<TeamDTO> teamDTOs = new List<TeamDTO>();
            

            foreach (var team in teams)
            {
                int playersCount = team.Players.Count;

                TeamDTO teamDTO = new TeamDTO
                {
                    Id = team.Id,
                    Name = team.Name,
                    PlayersCount = playersCount,
                };

                teamDTOs.Add(teamDTO);
            }
            Log.Error("Teams opgehaald.");
            return Ok(teamDTOs);
        }

        [HttpPost("join")]
        [Authorize]
        public async Task<IActionResult> Join([FromServices] DataContext context, UserManager<UserModel> userManager, [FromBody] JoinTeamRequest model)
        {
            var userName = GetUserNameFromToken();
            if (userName == null)
            {
                Log.Error("Gebruiker niet gevonden " + userName);
                return BadRequest("User not found");
            }

            var user = context.Users.Include(u => u.Team).FirstOrDefault(u => u.Email == userName);
            if (user == null)
            {
                Log.Error("Gebruiker niet gevonden " + userName);
                return BadRequest("User not found");
            }

            if (user.Team != null)
            {
                Log.Error("Gebruiker zit niet in een team.");
                return BadRequest("User is already in a team");
            }

            if (context.Teams.Include(t => t.Players).FirstOrDefault(t => t.Id == model.Id).Players.Count >= 20)
            {
                Log.Error("Team waar je bij wil voegen is al vol.");
                return BadRequest("Team is already full.");
            }
            var teamId = model.Id;
            
            var teamToJoin = context.Teams.FirstOrDefault(t => t.Id == teamId);
            user.Team = teamToJoin;
            try
            {
                context.SaveChanges();
                Log.Information("Gegevens zijn opgeslagen voor " + userName);
                return Ok("Joined the team.");
            }
            catch
            {
                Log.Error("Er is iets mis gegaan bij het op slaan.");
                return BadRequest("Something went wrong");
            }
        }
        
        public class JoinTeamRequest
        {
            public Guid Id { get; set; }
        }

        [HttpPost("removeFromTeam")]
        [Authorize]
        public async Task<IActionResult> RemoveFromTeam([FromServices] DataContext context, [FromBody] RemoveFromTeamRequest model)
        {

            var userName = GetUserNameFromToken();
        
            if (userName == null)
            {
                Log.Error("Gebruiker niet gevonden");
                return BadRequest("Unable to retrieve username from token.");
            }
            
            var requestedUserId = model.UserId;

            var GetUserToRemove = context.Users.Include(u => u.Team).FirstOrDefault(u => u.Id == requestedUserId);
            if (GetUserToRemove == null)
            {
                Log.Error("Er is iets mis gegaan bij het op slaan.");
                return BadRequest("User not found");
            }

            GetUserToRemove.Team = null;

            try
            {
                await context.SaveChangesAsync();
                Log.Error("Gebruiker is verwijderd van het team.");
                return Ok("User removed from team successfully");
            }
            catch (Exception)
            {
                Log.Error("Er is iets mis gegaan bij het verwijderen van de gebruiker uit het team.");
                return StatusCode(500, "Failed to remove user from team");
            }
        }

        public class RemoveFromTeamRequest
        {
            public string UserId { get; set; }
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


        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateTeam([FromServices] DataContext context, UserManager<UserModel> userManager, [FromBody] CreateTeamData model)
        {
            var userName = GetUserNameFromToken();
            if (userName == null)
                return BadRequest(new {status = 500, message = "Gebruiker is niet ingelogd."});

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
                return BadRequest(new {status = 400, message = "Gebruiker niet gevonden."});

            if (model.Name != "" || model.Name != null)
            {
                if (context.Teams.Where(t => t.Name.Equals(model.Name)).Count() == 0)
                {
                    TeamModel team = new TeamModel()
                    {
                        Name = model.Name,
                        Leader = user,
                    };
                    context.Teams.Add(team);

                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch
                    {
                        return BadRequest(new {status = 500, message = "Kon niet worden opgeslagen."});
                    }

                    user.Team = context.Teams.FirstOrDefault(t => t.Name == team.Name);
                    
                    try
                    {
                        await context.SaveChangesAsync();
                    }
                    catch
                    {
                        return BadRequest(new {status = 500, message = "Kon niet worden opgeslagen."});
                    }

                    return Ok(new {status = 200, message = "Team created"});
                }

                return BadRequest(new {status = 400, message = "Team bestaat al of mag niet null zijn."});
            }

            return BadRequest(new {status = 500, message = "Er is iets mis gegaan."});
        }

        public class CreateTeamData
        {
            [MaxLength(30)]
            [MinLength(3)]
            [Required]
            [RegularExpression(@"^[A-Za-z0-9' -]*$", ErrorMessage = "Only letters, digits, ', and - are allowed.")]
            public string Name { get; set; }
        }
    }