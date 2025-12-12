using FileAnalisysService.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace FileAnalisysService.Data;

public class AppDbContext : DbContext
{
    public DbSet<FileContent> FileContents => Set<FileContent>();
    public DbSet<AnalysisReport> AnalysisReports => Set<AnalysisReport>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FileContent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.StudentName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TaskId).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.TaskId);
        });
        
        modelBuilder.Entity<AnalysisReport>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SimilarityPercent);
            entity.Property(e => e.Status).HasMaxLength(20);
            
            entity.HasOne<FileContent>()
                .WithOne()
                .HasForeignKey<AnalysisReport>(e => e.FileId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
