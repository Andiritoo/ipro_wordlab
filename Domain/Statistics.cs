using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain;

public class Statistics
{
    public string UserName { get; set; }


    public int GamesPlayed { get; set; }

    public int GamesWon { get; set; }

    public int GamesLost => GamesPlayed - GamesWon;

    public double WinRate => GamesPlayed == 0 ? 0 : (double)GamesWon / GamesPlayed * 100;



    public int MaxStreak { get; set; }

    public int CurrentStreak { get; set; }

    public Dictionary<int, int> GuessDistribution { get; set; } = new();


    public int TotalGuesses { get; set; }

    public double AvgGuesses => GamesWon == 0 ? 0 : (double)TotalGuesses / GamesWon;


    public TimeSpan? BestTime { get; set; }

    public TimeSpan TotalDuration { get; set; }

    public TimeSpan AvgDuration => GamesPlayed == 0 ? TimeSpan.Zero : TimeSpan.FromTicks(TotalDuration.Ticks / GamesPlayed);

    public void RegisterGame(int guesses, TimeSpan duration, bool won)
    {
        GamesPlayed++;
        TotalDuration += duration;


        if (won)
        {
            GamesWon++;
            TotalGuesses += guesses;

            CurrentStreak++;
            if (CurrentStreak > MaxStreak)
            {
                MaxStreak = CurrentStreak;
            }

            // Ensure all lower guess buckets exist
            for (int i = 1; i <= guesses; i++)
            {
                if (!GuessDistribution.ContainsKey(i))
                {
                    GuessDistribution[i] = 0;
                }
            }

            GuessDistribution[guesses]++;

            if (BestTime == null || duration < BestTime)
            {
                BestTime = duration;
            }
        }
        else
        {
            CurrentStreak = 0;
        }
    }
}
