using Microsoft.EntityFrameworkCore;
using Raton.Models.DbModels;

namespace Raton.Services.DbServices
{
    public class DataContext : DbContext
    {
        public DbSet<AnimalModel> Animals { get; set; }
        public DbSet<PointModel> Points { get; set; }
        public DbSet<CatchModel> Catches { get; set; }
        public DbSet<SeriesModel> Series { get; set; }


        public DataContext()
        {
            Database.EnsureCreated();
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Filename=ARaton.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<AnimalModel>()
                .Property(e => e.Sex)
                .HasConversion<string>();
        }

        public void Clear()
        {
            this.ChangeTracker.Clear();
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}