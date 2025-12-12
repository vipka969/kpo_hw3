using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace FileStoringService.Models.DTOs;

public sealed record FileUploadRequest(
    [property: FromForm][Required][MinLength(2)] string StudentName,
    [property: FromForm][Required][Range(1, int.MaxValue)] int TaskId,
    [property: FromForm][Required] IFormFile File
    );