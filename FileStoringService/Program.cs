using FileStoringService.Data;
using FileStoringService.Models.DTOs;
using FileStoringService.Repositories;
using FileStoringService.UseCases.DownloadFiles;
using FileStoringService.UseCases.UploadFiles;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFileEntryRepository, FileEntryRepository>();

builder.Services.AddScoped<IUploadFileHandler, UploadFileHandler>();
builder.Services.AddScoped<IDownloadFileHandler, DownloadFileHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Storing Service v1");
    c.RoutePrefix = "swagger";
});

using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    Console.WriteLine("Database created/verified");
}

app.MapPost("/api/files/upload", async ([FromForm] FileUploadRequest request, IUploadFileHandler handler) =>
{
    try
    {
        if (request.File == null || request.File.Length == 0)
        {
            return Results.BadRequest("Файл пустой");
        }

        if (request.File.Length > 10 * 1024 * 1024)
        {
            return Results.BadRequest("Файл слишком большой (макс 10MB)");
        }

        FileUploadResponse response = await handler.Handle(request.StudentName, request.TaskId, request.File);

        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ошибка загрузки файла: {ex.Message}");
    }
}
).DisableAntiforgery()
.WithName("UploadFile")
.WithTags("Files");

app.MapGet("/api/files/{fileId}", async (Guid fileId, IDownloadFileHandler handler) =>
{
    try
    {
        FileDownloadResponse response = await handler.Handle(fileId);
        return Results.Ok(response);
    }
    catch (FileNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ошибка получения информации о файле: {ex.Message}");
    }
}).WithName("GetFileInfo")
.WithTags("Files");

app.MapGet("api/files/{fileId}/download", async (Guid fileId, IDownloadFileHandler handler, HttpContext context) =>
{
    try
    {
        FileDownloadResponse fileInfo = await handler.Handle(fileId);
        
        string filePath = Path.Combine("uploads", $"{fileId}_{fileInfo.FileName}");

        if (!File.Exists(filePath))
        {
            return Results.NotFound($"Файл не найден на диске: {filePath}");
        }
        
        byte[] fileBytes = await File.ReadAllBytesAsync(filePath);
        
        string contentType = GetContentType(fileInfo.FileName);
        
        return Results.File(fileBytes, contentType, fileInfo.FileName);
    }
    catch (FileNotFoundException ex)
    {
        return Results.NotFound(ex.Message);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ошибка скачивания файла: {ex.Message}");
    }
}).WithName("DownloadFile")
.WithTags("Files");


string GetContentType(string fileName)
{
    var extension = Path.GetExtension(fileName).ToLowerInvariant();
    return extension switch
    {
        ".pdf" => "application/pdf",
        ".txt" => "text/plain",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".zip" => "application/zip",
        _ => "application/octet-stream"
    };
}

app.Run();
