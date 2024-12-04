using Microsoft.EntityFrameworkCore;
using Samples.MongoDb.EFCore.Api.Models;

namespace Samples.MongoDb.EFCore.Api
{
    public class MediaLibraryDbContext:DbContext
    {
        public DbSet<Movie> Movies { get; init; }

        public MediaLibraryDbContext(DbContextOptions options)
       : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
