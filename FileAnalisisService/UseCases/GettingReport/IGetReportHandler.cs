using FileAnalisysService.DTOs;

namespace FileAnalisysService.UseCases.GettingReport;

public interface IGetReportHandler
{
    Task<ReportResponse> Handle (Guid fileId);
}