using FileStoringService.Models.Entities;

namespace FileStoringService.Repositories;

public interface IFileEntryRepository
{
    Task<FileEntry?> GetByIdAsync(Guid fileId);
    Task<List<FileEntry>> GetAllAsync();
    Task AddAsync(FileEntry fileEntry);
    Task UpdateAsync(FileEntry fileEntry);
    Task DeleteAsync(Guid fileId);
    
    Task<bool> ExistsAsync(Guid fileId);
    Task<FileEntry?> GetByFileNameAsync(string fileName);
    Task<List<FileEntry>> GetByTaskIdAsync(int taskId);
    Task<List<FileEntry>> GetByStudentNameAsync(string studentName);
    
    Task<FileEntry?> GetWithPathAsync(Guid fileId);
    Task<Dictionary<Guid, string>> GetFilePathsAsync(List<Guid> fileIds);
}