namespace FileAnalisysService.DTOs;

public sealed record WorkReportsResponse(
    string WorkId,
    List<ReportSummary> Reports
    );

public sealed record ReportSummary(
    Guid FileId,
    string StudentName,
    bool PlagiarismDetected,
    string Status
    );