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

    /// <summary>
    /// Asynchronously saves the specified statistics to persistent storage, replacing any existing entry for the same
    /// user.
    /// </summary>
    /// <param name="statistics">The statistics object to save. Cannot be null. The user's name is used to identify and replace any existing
    /// entry.</param>
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

    /// <summary>
    /// Loads the Statistics from the .json file at the specified filepath
    /// </summary>
    /// <param name="userName">The username of which the statistic should be loaded</param>
    /// <returns>A Statistics Object filled with the stats of the user passed in the username Parameter</returns>
    public async Task<Statistics> LoadAsync(string userName)
    {
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
