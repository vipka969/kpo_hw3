using FileAnalisysService.Models.Entities;

namespace FileAnalisysService.Repositories;

public interface IAnalysisReportRepository
{
    Task<AnalysisReport?> GetByFileIdAsync(Guid fileId);
    Task<List<AnalysisReport>> GetByTaskIdAsync(string taskId);
    Task AddAsync(AnalysisReport report);
    Task UpdateAsync(AnalysisReport report);
}