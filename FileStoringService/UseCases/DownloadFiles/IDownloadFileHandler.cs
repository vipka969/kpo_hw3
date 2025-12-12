using FileStoringService.Models.DTOs;

namespace FileStoringService.UseCases.DownloadFiles;

public interface IDownloadFileHandler
{
    Task<FileDownloadResponse> Handle(Guid fileId);
}