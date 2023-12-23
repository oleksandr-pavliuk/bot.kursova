using Microsoft.EntityFrameworkCore;

namespace bot.dkr
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(@"Host=localhost;Port=5432;Database=dkr.bot;Username=postgres;Password=databasePass1;");
        }
        public ApplicationContext()
        {
            Database.EnsureCreated();
        }
    }
}
