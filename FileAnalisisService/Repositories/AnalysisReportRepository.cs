using FileAnalisysService.Data;
using FileAnalisysService.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileAnalisysService.Repositories;

public class AnalysisReportRepository : IAnalysisReportRepository
{
    private readonly AppDbContext _context;

    public AnalysisReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AnalysisReport?> GetByFileIdAsync(Guid fileId)
    {
        return await _context.AnalysisReports
            .FirstOrDefaultAsync(r => r.FileId == fileId);
    }

    public async Task<List<AnalysisReport>> GetByTaskIdAsync(string taskId)
    {
        return await _context.AnalysisReports
            .Join(_context.FileContents, report => report.FileId, file => file.Id, (report, file) => new { Report = report, File = file })
            .Where(x => x.File.TaskId == taskId)
            .Select(x => x.Report)
            .ToListAsync();
    }

    public async Task AddAsync(AnalysisReport report)
    {
        await _context.AnalysisReports.AddAsync(report);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AnalysisReport report)
    {
        _context.AnalysisReports.Update(report);
        await _context.SaveChangesAsync();
    }
}