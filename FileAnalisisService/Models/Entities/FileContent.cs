namespace FileAnalisysService.Models.Entities;

public class FileContent
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public string StudentName { get; set; }
    public string TaskId { get; set; }
    public DateTime UploadedDate { get; set; }
    
    public AnalysisReport? Report { get; set; }
}