using System.Text.Json;
using AnyTalk.Models;

namespace AnyTalk.Services;

public class HistoryManager
{
    private static HistoryManager? instance;
    private readonly string historyFilePath;
    private List<TranscriptionEntry> entries;

    public static HistoryManager Instance
    {
        get
        {
            instance ??= new HistoryManager();
            return instance;
        }
    }

    private HistoryManager()
    {
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AnyTalk"
        );
        
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        historyFilePath = Path.Combine(appDataPath, "history.json");
        entries = LoadHistoryFromFile();
    }

    public void AddEntry(string text)
    {
        var entry = new TranscriptionEntry
        {
            Text = text,
            Timestamp = DateTime.Now,
            WordCount = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length
        };

        entries.Insert(0, entry); // Add to beginning of list
        SaveHistoryToFile();
        
        // Notify any listeners (like MainForm) that the word count has changed
        WordCountChanged?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? WordCountChanged;

    public List<TranscriptionEntry> GetEntries()
    {
        return entries;
    }

    public int GetTotalWordCount()
    {
        return entries.Sum(e => e.WordCount);
    }

    private List<TranscriptionEntry> LoadHistoryFromFile()
    {
        try
        {
            if (File.Exists(historyFilePath))
            {
                string json = File.ReadAllText(historyFilePath);
                var loadedEntries = JsonSerializer.Deserialize<List<TranscriptionEntry>>(json);
                if (loadedEntries != null)
                {
                    return loadedEntries;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error loading history: {ex.Message}",
                "History Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
        return new List<TranscriptionEntry>();
    }

    private void SaveHistoryToFile()
    {
        try
        {
            string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            File.WriteAllText(historyFilePath, json);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error saving history: {ex.Message}",
                "History Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
        }
    }
}

public class TranscriptionEntry
{
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public int WordCount { get; set; }
}
