using System.Text;
using FileAnalisysService.DTOs;
using FileAnalisysService.Models.Entities;
using FileAnalisysService.Repositories;
using FileAnalisysService.Services;

namespace FileAnalisysService.UseCases.AnalysisFiles;

public class AnalyseFileHandler : IAnalyseFileHandler
{
    private readonly HttpClient _httpClient;
    private readonly IFileContentRepository _contentRepository;
    private readonly IAnalysisReportRepository _reportRepository;
    private readonly PlagiarismChecker _checker;
    
    public AnalyseFileHandler(HttpClient httpClient, IFileContentRepository contentRepository, IAnalysisReportRepository reportRepository, PlagiarismChecker checker)
    {
        _httpClient = httpClient;
        _contentRepository = contentRepository;
        _reportRepository = reportRepository;
        _checker = checker;
    }
    
    public async Task<AnalysisResponse> Handle(Guid fileId, string name, string taskId)
    {
        DateTime now = DateTime.UtcNow;

        byte[] fileBytes = await GetFileFromStoringService(fileId);
        string fileText = Encoding.UTF8.GetString(fileBytes);

        FileContent fileContent = new FileContent
        {
            Id = fileId,
            Content = fileText,
            StudentName = name,
            TaskId = taskId,
            UploadedDate = now
        };
        await _contentRepository.AddAsync(fileContent);

        List<FileContent> files = await _contentRepository.GetPreviousForTaskAsync(taskId, name, now);
        List<string> previousTexts = files.Select(f => f.Content).ToList();

        (bool plagiarismDetected, double similarity) = _checker.Check(fileText, previousTexts);

        AnalysisReport report = new AnalysisReport
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            HasPlagiarism = plagiarismDetected,
            SimilarityPercent = similarity * 100,
            Status = "completed",
            AnalyzedDate = now
        };
        await _reportRepository.AddAsync(report);

        return new AnalysisResponse(report.Id, fileId, "completed", now);
    }
    
    private async Task<byte[]> GetFileFromStoringService(Guid fileId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(
            $"http://filestoring:5001/api/files/{fileId}/download");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get file: {response.StatusCode}");
        }
            
        return await response.Content.ReadAsByteArrayAsync();
    }
}
