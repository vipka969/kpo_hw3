using System.Net.Http.Headers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.MapGet("/health", () => new 
{ 
    status = "healthy", 
    service = "api-gateway",
    timestamp = DateTime.UtcNow 
});

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/api/files/{fileId}", async (Guid fileId, IHttpClientFactory clientFactory) =>
{
    try
    {
        HttpClient client = clientFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync($"http://filestoring:5001/api/files/{fileId}");

        string responseContent = await response.Content.ReadAsStringAsync();

        return Results.Text(responseContent, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            detail: "File Storage Service is temporarily unavailable. Please try again later.",
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
    
});

app.MapPost("/api/files/upload", async (HttpRequest request, IHttpClientFactory clientFactory) =>
{
    try
    {
        if (!request.HasFormContentType)
        {
            return Results.BadRequest("Expected multipart/form-data");
        }

        HttpClient client = clientFactory.CreateClient();
        IFormCollection form = await request.ReadFormAsync();
        MultipartFormDataContent multipartContent = new MultipartFormDataContent();
        
        foreach (IFormFile file in form.Files)
        {
            StreamContent fileContent = new StreamContent(file.OpenReadStream());
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            multipartContent.Add(fileContent, "file", file.FileName);
        }
        
        foreach (string key in form.Keys)
        {
            if (key != "file")
            {
                string formValue = form[key];
                if (formValue != null)
                {
                    multipartContent.Add(new StringContent(formValue), key);
                }
            }
        }

        HttpResponseMessage response = await client.PostAsync("http://filestoring:5001/api/files/upload", multipartContent);
        string responseContent = await response.Content.ReadAsStringAsync();
        return Results.Text(responseContent, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            detail: "File Storage Service is unavailable",
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
}).DisableAntiforgery();

app.MapGet("/api/files/{fileId}/download", async (Guid fileId, IHttpClientFactory clientFactory) =>
{
    try
    {
        HttpClient client = clientFactory.CreateClient();
        HttpResponseMessage response = await client.GetAsync($"http://filestoring:5001/api/files/{fileId}/download");

        if (response.IsSuccessStatusCode)
        {
            byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();
            string contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            string fileName = response.Content.Headers.ContentDisposition?.FileName ?? $"file_{fileId}";

            return Results.File(fileBytes, contentType, fileName);
        }

        string errorContent = await response.Content.ReadAsStringAsync();
        return Results.Text(errorContent, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            detail: "File Storage Service is unavailable",
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
});

app.MapPost("/api/analysis/{fileId}", async (Guid fileId, HttpRequest request, IHttpClientFactory clientFactory) =>
{
    try
    {
        HttpClient client = clientFactory.CreateClient();

        string queryString = request.QueryString.Value ?? "";

        HttpResponseMessage response =
            await client.PostAsync($"http://fileanalysis:5002/api/analysis/{fileId}{queryString}", null);

        string responseContent = await response.Content.ReadAsStringAsync();
        return Results.Text(responseContent, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            detail: "File Analysis Service is unavailable",
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
});

app.MapGet("/api/analysis/reports/{fileId}", async (Guid fileId, IHttpClientFactory clientFactory) =>
{
    try
    {
        HttpClient client = clientFactory.CreateClient();
        HttpResponseMessage response = await client.GetAsync($"http://fileanalysis:5002/api/analysis/reports/{fileId}");

        string responseContent = await response.Content.ReadAsStringAsync();
        return Results.Text(responseContent, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            detail: "File Analysis Service is unavailable",
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
});

app.MapGet("/api/analysis/works/{workId}/reports", async (string workId, IHttpClientFactory clientFactory) =>
{
    try
    {
        HttpClient client = clientFactory.CreateClient();
        HttpResponseMessage response =
            await client.GetAsync($"http://fileanalysis:5002/api/analysis/works/{workId}/reports");

        string responseContent = await response.Content.ReadAsStringAsync();
        return Results.Text(responseContent, "application/json", statusCode: (int)response.StatusCode);
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            detail: "File Analysis Service is unavailable",
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
});

app.MapGet("/api/visual/wordcloud/{fileId}", async (Guid fileId, IHttpClientFactory clientFactory) =>
{
    try
    {
        HttpClient client = clientFactory.CreateClient();

        HttpResponseMessage fileResponse = await client.GetAsync($"http://filestoring:5001/api/files/{fileId}/download");

        if (!fileResponse.IsSuccessStatusCode)
        {
            string errorText = await fileResponse.Content.ReadAsStringAsync();
            return Results.Text(errorText, "application/json", statusCode: (int)fileResponse.StatusCode);
        }

        byte[] fileBytes = await fileResponse.Content.ReadAsByteArrayAsync();
        string fileText = System.Text.Encoding.UTF8.GetString(fileBytes);

        if (string.IsNullOrWhiteSpace(fileText))
        {
            return Results.Problem(
                detail: "File is empty or contains no readable text.",
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        var chartRequest = new
        {
            format = "png",
            width = 800,
            height = 600,
            text = fileText
        };

        HttpResponseMessage wordCloudResponse = await client.PostAsJsonAsync("https://quickchart.io/wordcloud", chartRequest);

        if (!wordCloudResponse.IsSuccessStatusCode)
        {
            return Results.Problem(
                detail: "Failed to generate word cloud using QuickChart API.",
                statusCode: StatusCodes.Status502BadGateway
            );
        }

        byte[] imageBytes = await wordCloudResponse.Content.ReadAsByteArrayAsync();

        return Results.File(imageBytes, "image/png", $"{fileId}_wordcloud.png");
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            detail: "One of the backend services is unavailable. Try again later.",
            statusCode: StatusCodes.Status503ServiceUnavailable
        );
    }
    catch (Exception)
    {
        return Results.Problem(
            detail: "Unexpected error occurred while generating the word cloud.",
            statusCode: StatusCodes.Status500InternalServerError
        );
    }
});

app.Run("http://0.0.0.0:8080");

