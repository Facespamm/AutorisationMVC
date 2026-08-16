using Autorisation.Models;
using Microsoft.EntityFrameworkCore;

namespace Autorisation.Context
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext>options ): base (options)
        {
        }
      
      public DbSet<Autorisations> Autorisations {  get; set; }
    }
}
