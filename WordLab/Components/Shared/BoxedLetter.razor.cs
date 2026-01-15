using Domain;
using Microsoft.AspNetCore.Components;

namespace WordLab.Components.Shared;

public partial class BoxedLetter
{
    [Parameter]
    public LetterHint LetterHint { get; set; }

    private string HintClass => LetterHint.HintType == LetterHintType.Correct ? "correct-letter"
        : LetterHint.HintType == LetterHintType.Present ? "present-letter"
        : LetterHint.HintType == LetterHintType.Absent ? "absent-letter"
        : "empty-letter";
}