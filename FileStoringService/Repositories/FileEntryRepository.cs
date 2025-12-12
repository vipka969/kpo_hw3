using FileStoringService.Data;
using FileStoringService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileStoringService.Repositories;

public class FileEntryRepository : IFileEntryRepository
{
    private readonly AppDbContext _context;
    
    public FileEntryRepository(AppDbContext context) => _context = context;
    
    public async Task<FileEntry?> GetByIdAsync(Guid fileId)
    {
        return await _context.FileEntries.FirstOrDefaultAsync(f => f.Id == fileId);
    }

    public async Task<List<FileEntry>> GetAllAsync()
    {
        return await _context.FileEntries
            .OrderByDescending(f => f.UploadedDate)
            .ToListAsync();
    }

    public async Task AddAsync(FileEntry fileEntry)
    {
        await _context.FileEntries.AddAsync(fileEntry);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(FileEntry fileEntry)
    {
        _context.FileEntries.Update(fileEntry);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid fileId)
    {
        return await _context.FileEntries.AnyAsync(f => f.Id == fileId);
    }

    public async Task<FileEntry?> GetByFileNameAsync(string fileName)
    {
        return await _context.FileEntries.FirstOrDefaultAsync(f => f.FileName == fileName);
    }

    public async Task<List<FileEntry>> GetByTaskIdAsync(int taskId)
    {
        return await _context.FileEntries
            .Where(f => f.TaskId == taskId)
            .OrderBy(f => f.UploadedDate)
            .ToListAsync();
    }

    public async Task<List<FileEntry>> GetByStudentNameAsync(string studentName)
    {
        return await _context.FileEntries
            .Where(f => f.StudentName == studentName)
            .OrderByDescending(f => f.UploadedDate)
            .ToListAsync();
    }

    public async Task<FileEntry?> GetWithPathAsync(Guid fileId)
    {
        return await _context.FileEntries
            .Where(f => f.Id == fileId)
            .Select(f => new FileEntry 
            { 
                Id = f.Id, 
                FilePath = f.FilePath,
                FileName = f.FileName
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Dictionary<Guid, string>> GetFilePathsAsync(List<Guid> fileIds)
    {
        return await _context.FileEntries
            .Where(f => fileIds.Contains(f.Id))
            .Select(f => new { f.Id, f.FilePath })
            .ToDictionaryAsync(x => x.Id, x => x.FilePath);
    }

    public async Task DeleteAsync(Guid fileId)
    {
        FileEntry? fileEntry = await GetByIdAsync(fileId);
        if (fileEntry != null)
        {
            _context.FileEntries.Remove(fileEntry);
            await _context.SaveChangesAsync();
        }
    }
}