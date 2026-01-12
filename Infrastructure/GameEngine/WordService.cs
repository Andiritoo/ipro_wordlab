using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace Infrastructure.GameEngine;

public class WordService
{
    public void ValidateWord()
    {
        throw new NotImplementedException();
    }

    public string GetMysteryWord()
    {
        //TODO: Implement something else for now just a small list
        var words = new List<string>
        {
            "APPLE",
            "BRAVE",
            "CHAIR",
            "PLANT",
            "STONE",
            "TRAIN",
            "SMILE",
            "WATER"
        };

        var random = new Random();
        var selectedWord = words[random.Next(words.Count)];

        return selectedWord;
    }

    public List<LetterHint> GetHintsFromGuess(string mysteryWord, string guess)
    {
        if (mysteryWord.Length != guess.Length)
        {
            throw new ArgumentException("Die Länge des eingegebenen Wortes entspricht nicht der Länge des Lösungswortes.");
        }

        var result = new List<LetterHint>();
        var mysteryChars = mysteryWord.ToCharArray();
        var guessChars = guess.ToCharArray();

        var used = new bool[mysteryChars.Length];

        // Check for correct letters
        for (int i = 0; i < guessChars.Length; i++)
        {
            if (guessChars[i] == mysteryChars[i])
            {
                result.Add(new LetterHint(guessChars[i], LetterHintType.Correct));
                used[i] = true;
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
            for (int j = 0; j < mysteryChars.Length; j++)
            {
                if (!used[j] && guessChars[i] == mysteryChars[j])
                {
                    result[i].HintType = LetterHintType.Present;
                    used[j] = true;
                    break;
                }
            }
        }

        return result;
    }
}
