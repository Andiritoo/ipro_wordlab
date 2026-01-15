using Domain;
using Infrastructure.GameEngine;

namespace WordLabTests;

public class WordleHintTests
{
    private readonly WordService _service = new();

    [Fact]
    public void AllLettersCorrect()
    {
        var result = _service.GetHintsFromGuess("APPLE", "APPLE");

        Assert.All(result, h => Assert.Equal(LetterHintType.Correct, h.HintType));
    }

    [Fact]
    public void SomeLettersPresent()
    {
        var result = _service.GetHintsFromGuess("APPLE", "ALLEY");

        var expected = new[]
        {
            LetterHintType.Correct,
            LetterHintType.Present,
            LetterHintType.Absent,
            LetterHintType.Present,
            LetterHintType.Absent
        };

        Assert.Equal(expected, result.Select(r => r.HintType));
    }

    [Fact]
    public void GuessHasManyDuplicates()
    {
        var result = _service.GetHintsFromGuess("APPLE", "PPPPP");

        var expected = new[]
        {
            LetterHintType.Absent,
            LetterHintType.Correct,
            LetterHintType.Correct,
            LetterHintType.Absent,
            LetterHintType.Absent
        };

        Assert.Equal(expected, result.Select(r => r.HintType));
    }

    [Fact]
    public void MysteryWordHasDuplicates()
    {
        var result = _service.GetHintsFromGuess("LLAMA", "ALARM");

        var expected = new[]
        {
            LetterHintType.Present,
            LetterHintType.Correct,
            LetterHintType.Correct,
            LetterHintType.Absent,
            LetterHintType.Present
        };

        Assert.Equal(expected, result.Select(r => r.HintType));
    }

    [Fact]
    public void DuplicateHandlingIsCorrect()
    {
        var result = _service.GetHintsFromGuess("BALLS", "LLAMA");

        var expected = new[]
        {
            LetterHintType.Present,
            LetterHintType.Present,
            LetterHintType.Present,
            LetterHintType.Absent,
            LetterHintType.Absent
        };

        Assert.Equal(expected, result.Select(r => r.HintType));
    }

    [Fact]
    public void AllLettersAbsent()
    {
        var result = _service.GetHintsFromGuess("WORLD", "XXXXX");

        Assert.All(result, h => Assert.Equal(LetterHintType.Absent, h.HintType));
    }

    [Fact]
    public void LengthMismatchThrows()
    {
        Assert.Throws<ArgumentException>(() =>
            _service.GetHintsFromGuess("APPLE", "APP"));
    }
}