using Microsoft.EntityFrameworkCore;

namespace bot.dkr
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(@"connection string");
        }
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
    }
}
