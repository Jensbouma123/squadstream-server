using Web_API_new.Utilities;

namespace Web_API_new.DTOs.Match;

public class MatchActionDTO
{
    
    public Guid Id { get; set; }
    
    private string _type;
    public string Type
    {
        get { return _type; }
        set { _type = SanitizationUtility.RemoveHtmlTags(value); }
    }

    public int Minute { get; set; }

    private string _homeTeam;
    public string HomeTeam
    {
        get { return _homeTeam; }
        set { _homeTeam = SanitizationUtility.RemoveHtmlTags(value); }
    }

    private string _code;
    public string Code
    {
        get { return _code; }
        set { _code = SanitizationUtility.RemoveHtmlTags(value); }
    }

    public int ThumbsUp { get; set; } = 0;
    public int ThumbsDown { get; set; } = 0;
}