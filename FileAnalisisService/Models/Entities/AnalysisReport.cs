namespace FileAnalisysService.Models.Entities;

public class AnalysisReport
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public bool HasPlagiarism { get; set; }
    public double SimilarityPercent { get; set; }
    public string Status { get; set; }
    public DateTime AnalyzedDate { get; set; }
}
