using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;
using Samples.MongoDb.EFCore.Api.Models;

namespace Samples.MongoDb.EFCore.Api.Configurations
{
    public class MoviesConfiguration : IEntityTypeConfiguration<Movie>
    {
        public void Configure(EntityTypeBuilder<Movie> builder)
        {
            builder.ToCollection("movies");

            builder.HasKey(c => c._id);

            builder.Property(x => x._id)
                .HasElementName("_id")
                .HasBsonRepresentation(MongoDB.Bson.BsonType.Int64)
                .IsRequired();

            builder.Property(x => x.Title)
                .HasElementName("title")
                .HasBsonRepresentation(MongoDB.Bson.BsonType.String)
                .IsRequired();

            builder.Property(x => x.Rated)
                .HasElementName("rated")
                .HasBsonRepresentation(MongoDB.Bson.BsonType.Double)
                .IsRequired();

            builder.Property(x => x.Synopsis)
                .HasElementName("synopsis")
                .HasBsonRepresentation(MongoDB.Bson.BsonType.String)
                .IsRequired();

        }
    }
}
