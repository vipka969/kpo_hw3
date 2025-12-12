namespace FileAnalisysService.DTOs;

public sealed record AnalysisResponse(
    Guid AnalysisId,
    Guid FileId,
    string Status, 
    DateTime StartedDate
    );
