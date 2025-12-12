using FileAnalisysService.DTOs;

namespace FileAnalisysService.UseCases.AnalysisFiles;

public interface IAnalyseFileHandler
{
    Task<AnalysisResponse> Handle(Guid fileId, string name, string taskId);
}