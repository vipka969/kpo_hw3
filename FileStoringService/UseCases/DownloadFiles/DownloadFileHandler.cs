using FileStoringService.Models.DTOs;
using FileStoringService.Models.Entities;
using FileStoringService.Repositories;

namespace FileStoringService.UseCases.DownloadFiles;

public class DownloadFileHandler : IDownloadFileHandler
{
    private readonly IFileEntryRepository _repository;
    
    public DownloadFileHandler(IFileEntryRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<FileDownloadResponse> Handle(Guid fileId)
    {
        FileEntry? fileEntry = await _repository.GetByIdAsync(fileId);

        if (fileEntry == null)
        {
            throw new FileNotFoundException($"File with ID {fileId} not found");
        }

        if (!File.Exists(fileEntry.FilePath))
        {
            throw new FileNotFoundException($"Physical file not found at {fileEntry.FilePath}");
        }
        
        FileInfo fileInfo = new FileInfo(fileEntry.FilePath);
        string contentType = GetContentType(fileEntry.FileName);
        
        FileDownloadResponse response =  new FileDownloadResponse(
            FileId: fileId,
            StudentName: fileEntry.StudentName,
            FileName: fileEntry.FileName,
            TaskId: fileEntry.TaskId,
            ContentType: contentType,
            FileSize: fileEntry.FileSize,
            DateCreated: fileEntry.UploadedDate
        );
        
        return response;
    }
    
    private string GetContentType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".txt" => "text/plain",
            ".doc" or ".docx" => "application/msword",
            _ => "application/octet-stream"
        };
    }
}