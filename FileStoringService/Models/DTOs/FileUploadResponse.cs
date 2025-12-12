namespace FileStoringService.Models.DTOs;

public sealed record FileUploadResponse(
    Guid FileId,
    string FileName,
    string StudentName,
    string Status,
    int TaskId,
    DateTime DateCreated
    );