using Microsoft.EntityFrameworkCore;

namespace GameClient.Databases
{
    public class AccountContext : DbContext
    {
        public static string ConnectionString = string.Empty;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (string.IsNullOrEmpty(ConnectionString) == false)
            {
                optionsBuilder.UseMySQL(ConnectionString);
            }
            optionsBuilder.EnableSensitiveDataLogging(false);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }

        public DbSet<UserModel> Users { get; set; }
    }
}
