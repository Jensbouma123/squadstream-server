using Web_API_new.Utilities;

namespace Web_API_new.DTOs.Match
{
    public class MatchReportDTO
    {
        private string _code;
        public string Code
        {
            get { return _code; }
            set { _code = SanitizationUtility.RemoveHtmlTags(value); }
        }
    }
}