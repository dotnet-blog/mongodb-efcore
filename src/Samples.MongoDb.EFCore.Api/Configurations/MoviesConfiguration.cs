﻿using Microsoft.EntityFrameworkCore;
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

            builder.Property(x => x.ImdbId)
               .HasElementName("imdbid")
               .HasBsonRepresentation(MongoDB.Bson.BsonType.String)
               .IsRequired(false);

            builder.Property(x => x.Rating)
                .HasElementName("rating")
                .HasBsonRepresentation(MongoDB.Bson.BsonType.Double)
                .IsRequired(false);

            builder.Property(x => x.Synopsis)
                .HasElementName("synopsis")
                .HasBsonRepresentation(MongoDB.Bson.BsonType.String)
                .IsRequired(false);

            builder.Property(x => x.DateTimeCreated)
              .HasElementName("dateTimeCreated")
              .HasBsonRepresentation(MongoDB.Bson.BsonType.DateTime)
              .IsRequired();

            builder.Property(x => x.DateTimeModified)
              .HasElementName("dateTimeModified")
              .HasBsonRepresentation(MongoDB.Bson.BsonType.DateTime)
              .IsRequired(false);
        }
    }
}
