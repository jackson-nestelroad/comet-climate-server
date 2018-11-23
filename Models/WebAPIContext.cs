using Microsoft.EntityFrameworkCore;

namespace WebAPI.Models
{
    public class WebAPIContext : DbContext
    {
        public WebAPIContext(DbContextOptions<WebAPIContext> options) : base(options) { }

        // Tables in database
        public DbSet<Weather> Weather { get; set; }
        public DbSet<Twitter> Twitter { get; set; }
        public DbSet<Errors> Errors { get; set; }
    }
}