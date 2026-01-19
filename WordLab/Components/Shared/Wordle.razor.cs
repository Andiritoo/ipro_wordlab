using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Domain;
using Infrastructure.GameEngine;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.JSInterop;

namespace WordLab.Components.Shared;

public partial class Wordle
{
    [Parameter]
    public int? WordLength { get; set; } = 5;

    [Parameter]
    public int? MaxGuesses { get; set; } = 6;

    [Inject]
    public WordService _wordService { get; set; }

    [Inject]
    public IJSRuntime _jsRuntime { get; set; }

    [Inject]
    public IToastService ToastService { get; set; }

    public string MysteryWord { get; set; }

    public LetterHint[][] Guesses { get; set; }

    // For Keyboard display
    public List<LetterHint> AllHints { get; set; } = new List<LetterHint>();

    public int GuessCount { get; set; }

    private ElementReference KeyboardDiv;

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

        MysteryWord = await _wordService.GetMysteryWord(WordLength.Value);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime.InvokeVoidAsync("registerGlobalKeyboard", DotNetObjectReference.Create(this));
        }
    }


    [JSInvokable]
    public void OnGlobalKey(string key)
    {
        var currentWord = Guesses[GuessCount];

        if (!string.IsNullOrEmpty(key) && key.Length == 1 && char.IsLetter(key[0]))
        {
            currentWord.FirstOrDefault(x => x.Letter == null)?.Letter = key[0];
        }
        else if (key == "Backspace")
        {
            int lastIndex = Array.FindLastIndex(currentWord, x => x.Letter != null);

            if (lastIndex >= 0)
            {
                currentWord[lastIndex].Letter = null;
            }
        }
        else if (key == "Enter")
        {
            if (currentWord.Any(x => x.Letter == null))
            {
                ShowErrorMessage($"Ungültige Eingabe: Das Wort muss {WordLength} Buchstaben lang sein.");
            }
            else
            {
                SubmitGuess();
            }
        }
        else
        {
            //TODO: Handle invalid key
        }

        StateHasChanged();
    }

    private async Task SubmitGuess()
    {
        var guess = string.Concat(Guesses[GuessCount].Select(x => x.Letter)).ToUpper();

        if (!await _wordService.ValidateWord(guess))
        {
            ShowErrorMessage("Dieses Wort existiert nicht.");
            return;
        }

        try
        {
            Guesses[GuessCount] = _wordService.GetHintsFromGuess(MysteryWord, guess).ToArray();

            AllHints = Guesses.Reverse().SelectMany(x => x).ToList();

            if (Guesses[GuessCount].All(x => x.HintType == LetterHintType.Correct))
            {
                MysteryWordGuessed();
            }

            GuessCount++;

            if(GuessCount == MaxGuesses)
            {
                WordNotGuessed();
            }
        }
        catch (Exception ex)
        {
            //TODO: Specify exception and handle error
            ShowErrorMessage(ex.Message);
        }

        StateHasChanged();
    }

    public void UpdateBoxedLetters(char letter)
    {
        var currentWord = Guesses[GuessCount];

        currentWord.FirstOrDefault(lh => lh.Letter == null)?.Letter = letter;
        StateHasChanged();
    }

    public void ShowErrorMessage(string errorMessage)
    {
        var intent = ToastIntent.Error;
        ToastService.ShowToast(intent, errorMessage);
    }

    public void MysteryWordGuessed()
    {
        var intent = ToastIntent.Success;
        ToastService.ShowToast(intent, "Herzlichen Glückwunsch! Sie haben das Wort erraten.");
    }

    public void WordNotGuessed()
    {
        var intent = ToastIntent.Error;
        ToastService.ShowToast(intent, $"Keine Versuche mehr! Das Wort war: {MysteryWord}.");
    }
}