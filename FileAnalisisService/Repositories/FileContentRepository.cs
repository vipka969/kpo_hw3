using FileAnalisysService.Data;
using FileAnalisysService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileAnalisysService.Repositories;

public class FileContentRepository : IFileContentRepository
{
    private readonly AppDbContext _context;

    public FileContentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FileContent?> GetByIdAsync(Guid fileId)
    {
        return await _context.FileContents.FirstOrDefaultAsync(f => f.Id == fileId);
    }

    public async Task<List<FileContent>> GetAllAsync()
    {
        return await _context.FileContents.ToListAsync();
    }

    public async Task<List<FileContent>> GetAllExceptAsync(Guid excludeFileId)
    {
        return await _context.FileContents.Where(f => f.Id != excludeFileId).ToListAsync();
    }

    public async Task AddAsync(FileContent fileContent)
    {
        await _context.FileContents.AddAsync(fileContent);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid fileId)
    {
        return await _context.FileContents.AnyAsync(f => f.Id == fileId);
    }
    
    public async Task<List<FileContent>> GetByTaskIdAsync(string taskId)
    {
        return await _context.FileContents.Where(f => f.TaskId == taskId).ToListAsync();
    }
    
    public async Task<List<FileContent>> GetPreviousForTaskAsync(string taskId, string curName, DateTime curUploadTime)
    {
        return await _context.FileContents
            .Where(f => f.TaskId == taskId && f.StudentName != curName && f.UploadedDate < curUploadTime)
            .ToListAsync();
    }

}