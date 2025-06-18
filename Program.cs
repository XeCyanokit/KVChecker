using System.Net.Http.Headers;
using System.Text.Json;

const string ApiUrl = "https://api.unshared.shop/checkkv.php";
const int KvFileSize = 16 * 1024; // 16 KB

var currentDir = Directory.GetCurrentDirectory();
var binFiles = Directory.GetFiles(currentDir, "*.bin", SearchOption.AllDirectories)
                        .Where(file => new FileInfo(file).Length == KvFileSize)
                        .ToList();

if (!binFiles.Any())
{
    Console.WriteLine("No 16KB .bin files found.");
    return;
}

Console.WriteLine($"Found {binFiles.Count} file(s). Starting check...");

using var client = new HttpClient();

foreach (var filePath in binFiles)
{
    try
    {
        using var multipart = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(filePath);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        multipart.Add(fileContent, "file", Path.GetFileName(filePath));

        var response = await client.PostAsync(ApiUrl, multipart);
        var responseString = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<Dictionary<string, string>>(responseString);

        Console.WriteLine($"[{filePath}]");
        if (result != null && result.ContainsKey("status"))
        {
            Console.WriteLine($"Status: {result["status"]}");
            Console.WriteLine($"Message: {result["message"]}");
            Console.WriteLine($"Serial: {result["serial"]}");
        }
        else
        {
            Console.WriteLine("Invalid response.");
        }
        Console.WriteLine(new string('-', 40));
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error checking file {Path.GetFileName(filePath)}: {ex.Message}");
    }
}
