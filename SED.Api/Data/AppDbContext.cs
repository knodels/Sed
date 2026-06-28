using Microsoft.EntityFrameworkCore;
using SED.Api.Models;

namespace SED.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<ApprovalRoute> ApprovalRoutes => Set<ApprovalRoute>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>()
            .HasOne(d => d.Author)
            .WithMany()
            .HasForeignKey(d => d.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Document>()
            .Property(d => d.CreatedAt)
            .HasConversion(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        modelBuilder.Entity<ApprovalRoute>()
            .HasOne(r => r.Document)
            .WithMany(d => d.ApprovalRoutes)
            .HasForeignKey(r => r.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ApprovalRoute>()
            .HasOne(r => r.Approver)
            .WithMany()
            .HasForeignKey(r => r.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApprovalRoute>()
            .Property(r => r.SignedAt)
            .HasConversion(
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        modelBuilder.Entity<DocumentVersion>()
            .HasOne(v => v.Document)
            .WithMany(d => d.DocumentVersions)
            .HasForeignKey(v => v.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DocumentVersion>()
            .Property(v => v.CreatedAt)
            .HasConversion(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
    }
}