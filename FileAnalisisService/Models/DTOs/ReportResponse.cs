namespace FileAnalisysService.DTOs;

public sealed record ReportResponse(
    Guid ReportId,
    Guid FileId,
    bool HasPlagiarism,
    double SimilarityPercent,
    string Status,
    DateTime AnalyzedDate
    );