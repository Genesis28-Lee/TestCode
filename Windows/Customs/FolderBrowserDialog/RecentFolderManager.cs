using System.IO;
using System.Text.Json;

public static class RecentFolderManager
{
    private static string _filePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "MyApp", "recent_folders.json");

    public static List<string> LoadRecent()
    {
        if (!File.Exists(_filePath)) return new List<string>();
        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<string>>(json) ?? new();
    }

    public static void SaveRecent(IEnumerable<string> paths)
    {
        var dir = Path.GetDirectoryName(_filePath)!;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        var json = JsonSerializer.Serialize(paths.Distinct().Take(10));
        File.WriteAllText(_filePath, json);
    }
}
