// See https://aka.ms/new-console-template for more information

using System.Threading.Tasks;
using Domain;
using Infrastructure.GameEngine;


class Program
{
    static async Task Main()
    {
        int wordLength = 5;

        var wordService = new WordService();
        var mysteryWord = await wordService.GetMysteryWord(wordLength);

        Console.WriteLine($"Ein geheimes Wort wurde ausgewählt. Errate das Wort mit {mysteryWord.Length} Buchstaben.");
        Console.WriteLine("Du hast 6 Versuche. Richtige Buchstaben sind in [] eingerahmt und Buchstaben die vorkommen in ()\n");

        const int maxAttempts = 6;

        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            Console.Write($"Versuch {attempt}: ");
            var guess = Console.ReadLine()?.Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(guess) || guess.Length != mysteryWord.Length)
            {
                Console.WriteLine("Ungültige Eingabe.\n");
                attempt--;
                continue;
            }
            else if(!await wordService.ValidateWord(guess))
            {
                Console.WriteLine("Dieses Wort existiert nicht.\n");
                attempt--;
                continue;
            }

            var hints = wordService.GetHintsFromGuess(mysteryWord, guess);
            PrintHints(hints);

            if (guess == mysteryWord)
            {
                Console.WriteLine("\nGewonnen!");
                return;
            }
        }

        Console.WriteLine($"\nVerloren! Das Wort war: {mysteryWord}");
    }

    static void PrintHints(List<LetterHint> hints)
    {
        foreach (var hint in hints)
        {
            switch(hint.HintType)
            {
                case LetterHintType.Correct:
                    Console.Write($" [{hint.Letter}]");
                    break;

                case LetterHintType.Present:
                    Console.Write($" ({hint.Letter})");
                    break;

                default:
                    Console.Write($" {hint.Letter}");
                break;
            }
        }
        Console.WriteLine("\n");
    }
}