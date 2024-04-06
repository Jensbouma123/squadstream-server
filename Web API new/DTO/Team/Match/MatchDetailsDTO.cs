using Web_API_new.Utilities;

namespace Web_API_new.DTOs.Match;

public class MatchDetailsDTO
{
    private string _match;
    public string Match
    {
        get { return _match; }
        set { _match = SanitizationUtility.RemoveHtmlTags(value); }
    }

    private string _homeTeam;
    public string HomeTeam
    {
        get { return _homeTeam; }
        set { _homeTeam = SanitizationUtility.RemoveHtmlTags(value); }
    }

    private string _oppTeam;
    public string OppTeam
    {
        get { return _oppTeam; }
        set { _oppTeam = SanitizationUtility.RemoveHtmlTags(value); }
    }
    
    public int ScoreHome { get; set; } = 0;
    public int ScoreOpp { get; set; } = 0;
    public int Minute { get; set; } = 0;

    public List<MatchActionDTO> Actions { get; set; } = new List<MatchActionDTO>();
}