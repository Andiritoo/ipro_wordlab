using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Domain;

namespace Infrastructure.GameEngine;

public class WordService
{
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// Checks if a word exists in a dictionary. Uses a Dictionary API.
    /// </summary>
    /// <param name="word">The word to be checked</param>
    /// <returns>Returns true if the word exists in the dictionary and false if it doesn't</returns>
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

    /// <summary>
    /// Determines whether the specified word is sufficiently frequent in English according to the Datamuse API.
    /// Ensures the mystery word is a word that the average english speaker should know
    /// </summary>
    /// <param name="word">The word to validate for frequency. Cannot be null, empty, or consist only of white-space characters.</param>
    /// <returns>true if the word exists and its frequency is at least 2.5... otherwise false.</returns>
    public async Task<bool> ValidateWordFrequency(string word)
    {
        if (string.IsNullOrWhiteSpace(word))
        {
            return false;
        }

        var url = $"https://api.datamuse.com/words?sp={word.ToLower()}&md=f&max=1";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<DatamuseWord>>(url);

            // Word does not exist
            if (response == null || response.Count == 0)
            {
                return false;
            }

            // For Mysteryword Check how frequent the word is used in English
            var tags = response[0].Tags;
            var freqTag = tags?.FirstOrDefault(t => t.StartsWith("f:"));

            if (freqTag == null)
            {
                return false;
            }

            var frequency = double.Parse(freqTag[2..], CultureInfo.InvariantCulture);

            return frequency >= 2.5;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Asynchronously retrieves a random English word of the specified length from an external API, ensuring the word
    /// meets frequency criteria.
    /// </summary>
    /// <remarks>The method may make multiple requests to the external API until a word meeting frequency
    /// requirements is found. The returned word is always converted to uppercase.</remarks>
    /// <param name="length">The desired length of the mystery word to retrieve. Must be a positive integer.</param>
    /// <returns>A string containing the randomly selected word in uppercase letters. The word will have the specified length.</returns>
    /// <exception cref="Exception">Thrown if the external API request fails, returns no words, or if a valid word cannot be retrieved.</exception>
    public async Task<string> GetMysteryWord(int length)
    {
        var url = $"https://random-word-api.herokuapp.com/word?length={length}";

        try
        {
            bool wordIsValid = false;
            string word = null;

            while (!wordIsValid)
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("There was an Error trying to get a mystery word.");
                }

                var json = await response.Content.ReadAsStringAsync();

                var words = JsonSerializer.Deserialize<string[]>(json);

                if (words == null || words.Length == 0)
                {
                    throw new Exception("API returned no words.");
                }

                wordIsValid = await ValidateWordFrequency(words[0]);

                if(wordIsValid)
                {
                    word = words[0];
                }
            }

            if(word is null)
            {
                throw new Exception("The API didn't return a valid word, try again.");
            }

            return word.ToUpper();
        }
        catch
        {
            throw new Exception("There was an Error trying to get a mystery word.");
        }
    }

    /// <summary>
    /// This is the "Wordle logic" gets the hints for the words.
    /// Generates a list of letter hints indicating the correctness of each letter in a guess compared to the mystery
    /// word.
    /// </summary>
    /// <remarks>Hints are determined by comparing each letter in the guess to the corresponding letter in the
    /// mystery word. Letters at the correct position are Correct/Green and letters that are present in the word but at the wrong place
    /// are marked as Yellow/Present.</remarks>
    /// <param name="mysteryWord">The word to be guessed.</param>
    /// <param name="guess">The guessed word to evaluate against the mystery word.</param>
    /// <returns>A list of <see cref="LetterHint"/> objects, each representing the hint for the corresponding letter in the
    /// guess. Each hint indicates whether the letter is correct, present in a different position, or absent from the
    /// mystery word.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="mysteryWord"/> and <paramref name="guess"/> do not have the same length.</exception>
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
