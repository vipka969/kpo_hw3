using FileAnalisysService.Models.Entities;

namespace FileAnalisysService.Repositories;

public interface IFileContentRepository
{
    Task<FileContent?> GetByIdAsync(Guid fileId);
    Task<List<FileContent>> GetAllAsync();
    Task AddAsync(FileContent fileContent);
    Task<bool> ExistsAsync(Guid fileId);
    
    Task<List<FileContent>> GetAllExceptAsync(Guid excludeFileId);
    Task<List<FileContent>> GetByTaskIdAsync(string taskId);
    Task<List<FileContent>> GetPreviousForTaskAsync(string taskId, string curName,
        DateTime curUploadTime);
}