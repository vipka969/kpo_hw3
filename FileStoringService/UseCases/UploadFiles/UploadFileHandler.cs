using FileStoringService.Models.DTOs;
using FileStoringService.Models.Entities;
using FileStoringService.Repositories;

namespace FileStoringService.UseCases.UploadFiles;

public class UploadFileHandler : IUploadFileHandler
{
    private readonly IFileEntryRepository _repository;
    
    public UploadFileHandler(IFileEntryRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<FileUploadResponse> Handle(string studentName, int taskId, IFormFile file)
    {
        Guid fileId = Guid.NewGuid();
        string fileName = $"{fileId}_{file.FileName}";
        string filePath = Path.Combine("uploads", fileName);
        
        using FileStream stream = File.Create(filePath);
        await file.CopyToAsync(stream);
        
        FileEntry fileEntry = new FileEntry()
        {
            Id = fileId,
            StudentName = studentName,
            TaskId = taskId,
            FileName = file.FileName,
            FilePath = filePath,
            FileSize = file.Length,
            UploadedDate = DateTime.UtcNow
        };
        
        await _repository.AddAsync(fileEntry);
        
        FileUploadResponse response = new FileUploadResponse(fileId, file.FileName, studentName, "uploaded", taskId, DateTime.Now);

        return response;
    }
}