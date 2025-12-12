using FileStoringService.Models.DTOs;

namespace FileStoringService.UseCases.UploadFiles;

public interface IUploadFileHandler
{
    Task<FileUploadResponse> Handle(string studentName, int taskId, IFormFile file);
}