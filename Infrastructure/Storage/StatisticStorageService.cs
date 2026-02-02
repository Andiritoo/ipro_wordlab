using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Domain;


namespace Infrastructure.Storage;

public class StatisticStorageService
{
    private const string filePath = "statistics.json";

    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true
    };

    public async Task SaveAsync(Statistics statistics)
    {
        List<Statistics> statisticsList;

        if (File.Exists(filePath))
        {
            var existingJson = await File.ReadAllTextAsync(filePath);

            statisticsList = JsonSerializer.Deserialize<List<Statistics>>(existingJson, _options) ?? new List<Statistics>();
        }
        else
        {
            statisticsList = new List<Statistics>();
        }

        // Ensure only one entry per user
        statisticsList.RemoveAll(x => x.UserName == statistics.UserName);

        statisticsList.Add(statistics);

        var json = JsonSerializer.Serialize(statisticsList, _options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public async Task<Statistics> LoadAsync(string userName)
    {
        //TODO: Fix this workaround
        if(userName == string.Empty)
        {
            userName = null;
        }

        if (!File.Exists(filePath))
        {
            return new Statistics();
        }

        var json = await File.ReadAllTextAsync(filePath);

        var statistics = JsonSerializer.Deserialize<List<Statistics>>(json, _options) ?? new List<Statistics>();

        return statistics.Find(s => s.UserName == userName) ?? new Statistics();
    }

}
