using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileStoringService.Models.Entities;

[Table("FileEntries")]
public class FileEntry
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string StudentName { get; set; } = string.Empty;
    
    [Required]
    public int TaskId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string FilePath { get; set; } = string.Empty;
    
    public long FileSize { get; set; }
    
    [Required]
    public DateTime UploadedDate { get; set; }
}