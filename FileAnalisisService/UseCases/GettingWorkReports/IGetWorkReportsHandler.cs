using FileAnalisysService.DTOs;

namespace FileAnalisysService.UseCases.GettingWorkReports;

public interface IGetWorkReportsHandler
{
    Task<WorkReportsResponse> Handle(string workId);
}