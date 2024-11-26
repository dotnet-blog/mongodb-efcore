using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;
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

        internal class ObjectIdValueGenerator : ValueGenerator<string>
        {
            public override bool GeneratesTemporaryValues => false;

            public override string Next(EntityEntry entry)
            {
                return ObjectId.GenerateNewId().ToString();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
        }
    }
}
