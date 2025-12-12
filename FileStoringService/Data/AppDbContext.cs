using FileStoringService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Data;

public class AppDbContext : DbContext
{
    public DbSet<FileEntry> FileEntries => Set<FileEntry>();
    
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
