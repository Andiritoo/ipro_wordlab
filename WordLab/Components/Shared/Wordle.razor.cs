using Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.JSInterop;

namespace WordLab.Components.Shared;

public partial class Wordle
{
    [Parameter]
    public int? WordLength { get; set; } = 5;

    [Parameter]
    public int? MaxGuesses { get; set; } = 6;

    public LetterHint[][] Guesses { get; set; }

    public int WordsGuessed { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        // Initiate Grid with "Empty" Values
        Guesses = new LetterHint[MaxGuesses.Value][];
        for (int i = 0; i < MaxGuesses; i++)
        {
            Guesses[i] = new LetterHint[WordLength.Value];

            for (int j = 0; j < WordLength.Value; j++)
            {
                Guesses[i][j] = new LetterHint();
            }
        }
    }

    [JSInvokable]
    public void OnGlobalKey(string key)
    {
        var currentWord = Guesses[WordsGuessed];

        currentWord.FirstOrDefault(lh => lh.Letter == null)?.Letter = key.ToCharArray()[0];
        StateHasChanged();
    }
}