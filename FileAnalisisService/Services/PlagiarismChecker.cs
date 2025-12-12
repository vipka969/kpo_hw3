namespace FileAnalisysService.Services;

public class PlagiarismChecker
{
    
    public (bool Detected, double Similarity) Check(string newFile, List<string> files)
    {
        if (files == null || !files.Any())
        {
            return (false, 0);
        }

        foreach (var existingContent in files)
        {
            double similarity = CalculateSimilarity(newFile, existingContent);
            if (similarity >= 0.8)
            {
                return (true, similarity);
            }
        }

        return (false, 0);
    }

    private double CalculateSimilarity(string text1, string text2)
    {
        text1 = NormalizeText(text1);
        text2 = NormalizeText(text2);

        if (text1 == text2)
        {
            return 1.0;
        }

        string[] words1 = text1.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string[] words2 = text2.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        int common = words1.Intersect(words2).Count();
        int total = Math.Max(words1.Length, words2.Length);

        return total == 0 ? 0 : (double)common / total;
    }

    private string NormalizeText(string text)
    {
        return text.ToLower()
            .Replace("\r", " ")
            .Replace("\n", " ")
            .Replace(".", " ")
            .Replace(",", " ")
            .Trim();
    }
}