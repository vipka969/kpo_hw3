using FileAnalisysService.DTOs;
using FileAnalisysService.Models.Entities;
using FileAnalisysService.Repositories;

namespace FileAnalisysService.UseCases.GettingWorkReports;

public class GetWorkReportsHandler : IGetWorkReportsHandler
{
    private readonly IAnalysisReportRepository _reportRepository;
    private readonly IFileContentRepository _fileContentRepository;

    public GetWorkReportsHandler(IAnalysisReportRepository reportRepository, IFileContentRepository fileContentRepository)
    {
        _reportRepository = reportRepository;
        _fileContentRepository = fileContentRepository;
    }
    
    public async Task<WorkReportsResponse> Handle(string workId)
    {
        List<FileContent> fileContents = await _fileContentRepository.GetByTaskIdAsync(workId);
            
        List<ReportSummary> reports = new List<ReportSummary>();
            
        foreach (FileContent fileContent in fileContents)
        {
            AnalysisReport? report = await _reportRepository.GetByFileIdAsync(fileContent.Id);
                
            if (report != null)
            {
                reports.Add(new ReportSummary(fileContent.Id, fileContent.StudentName, report.HasPlagiarism, report.Status));
            }
            else
            {
                reports.Add(new ReportSummary(fileContent.Id, fileContent.StudentName,
                    false, "pending"));
            }
        }
            
        return new WorkReportsResponse(workId, reports);
    }
}