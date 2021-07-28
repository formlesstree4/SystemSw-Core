namespace SystemSw_Api.Models
{
    public class ExtronConfiguration
    {
        public string Port { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public string Type { get; set; }
        public bool AutoOpen { get; set; }
    }
}