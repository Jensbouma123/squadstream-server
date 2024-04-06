        using System.Text.RegularExpressions;
        using Microsoft.AspNetCore.SignalR;
        using Web_API_new.DTO.Responses;
        using Web_API_new.DTOs.Match;

        namespace Web_API_new.Hubs;

        public class MatchReportHub : Hub
        {
            private static List<MatchDetailsDTO> games = new List<MatchDetailsDTO>();
            
            public async Task JoinSpecificMatchReport(MatchReportDTO matchReport)
            {
                var game = games.FirstOrDefault(g => g.Match == matchReport.Code);
                if (game == null)
                {
                    await Clients.Caller.SendAsync("ErrorMessage", new ErrorMessageDTO()
                    {
                        Status = 400,
                        Message = "Team niet gevonden."
                    });
                }
                else
                {
                    try
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, matchReport.Code);
                    }
                    catch (Exception e)
                    {
                        await Clients.Caller.SendAsync("ErrorMessage", new ErrorMessageDTO()
                        {
                            Status = 500,
                            Message = "Er is iets mis gegaan.",
                        });
                    }
                }
            }
            
            
            public async Task GetAvailableGames()
            {
                await Clients.All.SendAsync("GetAvailableGames", games);
            }

            public async Task CreateMatchReport(MatchDetailsDTO matchDetails)
            {
                var gameExist = games.FirstOrDefault(g => g.Match == matchDetails.Match);
                if (gameExist != null)
                {
                    await Clients.Caller.SendAsync("ErrorMessage", new ErrorMessageDTO()
                    {
                        Status = 500,
                        Message = "Er is al een team met die naam die live is. Stop deze eerst.",
                    });
                }
                else
                {
                    games.Add(matchDetails);
                    await Clients.All.SendAsync("GetAvailableGames", games);
                }
            }

            public async Task CreateAction(MatchActionDTO action)
            {
                var game = games.FirstOrDefault(g => g.Match.Equals(action.Code));
                if (game != null)
                {
                    if (action.Type == "goal")
                    {
                        if (action.HomeTeam == "homeTeam")
                        {
                            game.ScoreHome = (game.ScoreHome + 1);
                        }
                        else if(action.HomeTeam == "oppTeam")
                        {
                            game.ScoreOpp = (game.ScoreOpp + 1);
                        }
                    }

                    game.Actions.Add(new MatchActionDTO()
                    {
                        Id = Guid.NewGuid(),
                        Type = action.Type,
                        HomeTeam = action.HomeTeam,
                        Minute = action.Minute,
                        ThumbsUp = 0,
                        ThumbsDown = 0,
                    });

                    await Clients.All
                        .SendAsync("GetAvailableGames", games);
                }
                else
                {
                    await Clients.Caller.SendAsync("ErrorMessage", new ErrorMessageDTO()
                    {
                        Status = 400,
                        Message = "Er is iets mis gegaan.",
                    });
                }
            }

            public async Task StopMatchReport(MatchReportDTO matchReport)
            {
                var game = games.FirstOrDefault(g => g.Match.Equals(matchReport.Code));
                if (game != null)
                {
                    games.Remove(game);
                    await Clients.All
                        .SendAsync("StopMatchReport");
                }
                else
                {
                    await Clients.Caller.SendAsync("ErrorMessage", new ErrorMessageDTO()
                    {
                        Status = 400,
                        Message = "Live wedstrijdverslag niet gevonden om te stoppen.",
                    });
                }
            }

            public async Task AddThumb(ActionThumbDTO actionThumb)
            {
                Console.WriteLine(actionThumb.Index);
                var game = games.FirstOrDefault(g => g.Match == actionThumb.Match);
                if (game != null)
                {
                    var action = game.Actions.FirstOrDefault(a => a.Id == actionThumb.Index);
                    if (action != null)
                    {
                        if (actionThumb.Type.Equals("up"))
                            action.ThumbsUp++;
                        else if (actionThumb.Type.Equals("down"))
                            action.ThumbsDown++;
                    }
                }
                await Clients.All
                    .SendAsync("GetAvailableGames", games);
            }
        }