using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Domain;

namespace Infrastructure.GameEngine;

public class WordService
{
    private static readonly HttpClient _httpClient = new();

    public async Task<bool> ValidateWord(string word)
    {
        var url = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word.ToLower()}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<string> GetMysteryWord(int length)
    {
        var url = $"https://random-word-api.herokuapp.com/word?length={length}";

        try
        {
            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("There was an Error trying to get a mystery word.");
            }

            var json = await response.Content.ReadAsStringAsync();

            // Deserialize ["cookeys"] → string[]
            var words = JsonSerializer.Deserialize<string[]>(json);

            if (words == null || words.Length == 0)
            {
                throw new Exception("API returned no words.");
            }

            return words[0].ToUpper();
        }
        catch
        {
            throw new Exception("There was an Error trying to get a mystery word.");
        }
    }

    public List<LetterHint> GetHintsFromGuess(string mysteryWord, string guess)
    {
        if (mysteryWord.Length != guess.Length)
        {
            throw new ArgumentException("Die Länge des eingegebenen Wortes entspricht nicht der Länge des Lösungswortes.");
        }

        var result = new List<LetterHint>();
        var guessChars = guess.ToCharArray();

        (char Letter, bool WasUsed)[] mystery = mysteryWord.Select(c => (c, false)).ToArray();

        // Check for correct letters
        for (int i = 0; i < guessChars.Length; i++)
        {
            if (guessChars[i] == mystery[i].Letter)
            {
                result.Add(new LetterHint(guessChars[i], LetterHintType.Correct));
                mystery[i].WasUsed = true;
            }
            else
            {
                // placeholder
                result.Add(new LetterHint(guessChars[i], LetterHintType.Absent));
            }
        }

        // Check for Present Letters
        for (int i = 0; i < guessChars.Length; i++)
        {
            if (result[i].HintType == LetterHintType.Correct)
            {
                continue;
            }

            // loops through the mystery word and checks if the letter is present while considering the duplicates
            for (int j = 0; j < mystery.Length; j++)
            {
                if (!mystery[j].WasUsed && guessChars[i] == mystery[j].Letter)
                {
                    result[i].HintType = LetterHintType.Present;
                    mystery[j].WasUsed = true;
                    break;
                }
            }
        }

        return result;
    }
}
