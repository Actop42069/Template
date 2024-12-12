namespace Common.Configurations
{
    public class ApplicationConfiguration
    {
        public const string SECTION_NAME = "Application";
        public string ClientUrl { get; set; } 
        public string AdminUrl { get; set; }
        public string ApiUrl { get; set; }
    }
}
