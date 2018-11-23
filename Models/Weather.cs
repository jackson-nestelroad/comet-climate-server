namespace WebAPI.Models
{
    public class Weather
    {
        public int id { get; set; }
        public int temperature { get; set; }
        public string condition { get; set; }
        public int high { get; set; }
        public int low { get; set; }
        public int wind_speed { get; set; }
        public string wind_direction { get; set; }
        public int wind_chill { get; set; }
        public int precipitation { get; set; }
        public int[] forecast { get; set; }
        public System.DateTime last_updated { get; set; }
    }
}