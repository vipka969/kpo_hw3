using FileAnalisysService.Data;
using FileAnalisysService.DTOs;
using FileAnalisysService.Repositories;
using FileAnalisysService.Services;
using FileAnalisysService.UseCases.AnalysisFiles;
using FileAnalisysService.UseCases.GettingReport;
using FileAnalisysService.UseCases.GettingWorkReports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

    
builder.Services.AddScoped<IFileContentRepository, FileContentRepository>();
builder.Services.AddScoped<IAnalysisReportRepository, AnalysisReportRepository>();

builder.Services.AddScoped<PlagiarismChecker>();

builder.Services.AddScoped<IAnalyseFileHandler, AnalyseFileHandler>();
builder.Services.AddScoped<IGetReportHandler, GetReportHandler>();
builder.Services.AddScoped<IGetWorkReportsHandler, GetWorkReportsHandler>();

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "File Analisis Service v1");
    c.RoutePrefix = "swagger";
});

using (IServiceScope scope = app.Services.CreateScope())
{
    AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    await dbContext.Database.EnsureCreatedAsync();
    Console.WriteLine("Database created/verified");
}

app.MapPost("/api/analysis/{fileId}", async (Guid fileId, [FromQuery] string studentName, [FromQuery] string taskId,
    IAnalyseFileHandler handler) =>
{
    try
    {
        AnalysisResponse response = await handler.Handle(fileId, studentName, taskId);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Analysis failed: {ex.Message}");
    }
});

app.MapGet("/api/analysis/reports/{fileId}", async (Guid fileId, IGetReportHandler handler) =>
{
    ReportResponse response = await handler.Handle(fileId);
    return Results.Ok(response);
});

app.MapGet("/api/analysis/works/{workId}/reports", async (string workId, IGetWorkReportsHandler handler) =>
{
    try
    {
        WorkReportsResponse response = await handler.Handle(workId);
        return Results.Ok(response);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error in GetWorkReports: {ex.Message}");
        return Results.Problem($"Error: {ex.Message}");
    }
    
});

app.Run();
