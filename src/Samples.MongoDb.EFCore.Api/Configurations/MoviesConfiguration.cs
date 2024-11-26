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
                .IsRequired();

            builder.Property(x => x.Title)
                .HasElementName("title")
                .IsRequired();

            builder.Property(x => x.Rated)
                .HasElementName("rated")
                .IsRequired();

            builder.Property(x => x.Synopsis)
                .HasElementName("synopsis")
                .IsRequired();

        }
    }
}
