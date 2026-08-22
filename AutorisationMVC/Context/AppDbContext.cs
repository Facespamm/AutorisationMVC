using Autorisation.Models;
using AutorisationMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace Autorisation.Context
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>options ): base (options)
        {
        }
      
      public DbSet<Autorisations> Autorisations {  get; set; }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
          base.OnModelCreating(modelBuilder);
          modelBuilder.Entity<Autorisations>().HasIndex(x=>x.Email).IsUnique();

      }
    }
    
}
