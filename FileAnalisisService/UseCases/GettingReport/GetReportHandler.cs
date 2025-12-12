using FileAnalisysService.DTOs;
using FileAnalisysService.Models.Entities;
using FileAnalisysService.Repositories;

namespace FileAnalisysService.UseCases.GettingReport;

public class GetReportHandler : IGetReportHandler
{
    private readonly IAnalysisReportRepository _reportRepository;

    public GetReportHandler(IAnalysisReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }
    
    public async Task<ReportResponse> Handle(Guid fileId)
    {
        AnalysisReport? report = await _reportRepository.GetByFileIdAsync(fileId);
            
        if (report == null)
        {
            return new ReportResponse(Guid.NewGuid(), fileId, false, 0.0, "not_found", DateTime.UtcNow);
        }
            
        return new ReportResponse(report.Id, report.FileId, report.HasPlagiarism, report.SimilarityPercent, report.Status, report.AnalyzedDate);
    }
}