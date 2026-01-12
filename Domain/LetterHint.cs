namespace Domain;

public class LetterHint
{
    public LetterHint(char letter, LetterHintType hintType)
    {
        Letter = letter;
        HintType = hintType;
    }

    public char Letter { get; set; }

    public LetterHintType HintType { get; set; }
}
