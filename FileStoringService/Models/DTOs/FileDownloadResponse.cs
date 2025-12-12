namespace FileStoringService.Models.DTOs;

public sealed record FileDownloadResponse(
    Guid FileId,
    string FileName,
    string StudentName,
    int TaskId,
    string ContentType,
    long FileSize,
    DateTime DateCreated
    );