using System;
using System.Collections.Generic;
using System.Text;
using Domain;

namespace Infrastructure.GameEngine;

public class WordService
{
    public async Task ValidateWord()
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetMysteryWord()
    {
        //TODO: Implement randomness ofc
        return "VEBOS";
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
                continue;

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
