using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SecureFilePipeline.Db.Entities;

namespace SecureFilePipeline.Db;

public class FileMetadataContext : DbContext
{
    public FileMetadataContext(DbContextOptions<FileMetadataContext> options)
        : base(options) { }

    public DbSet<FileMetadata> Files => Set<FileMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileMetadata>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Properties)
         .HasColumnType("jsonb")
         .HasConversion(
             v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
             v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions)null)
         );
        });
    }
}
