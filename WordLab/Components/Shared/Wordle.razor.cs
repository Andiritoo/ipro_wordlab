using System.Diagnostics.Metrics;
using System.Threading.Tasks;
using Domain;
using Infrastructure.GameEngine;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.IdentityModel.Abstractions;
using Microsoft.JSInterop;

namespace WordLab.Components.Shared;

public partial class Wordle
{
    [Inject]
    public WordService _wordService { get; set; }

    [Inject]
    public StatisticStorageService _storageService { get; set; }

    [Inject]
    public IJSRuntime _jsRuntime { get; set; }

    [Inject]
    public IToastService ToastService { get; set; }

    [CascadingParameter(Name = "Settings")]
    public WordleSettings? Settings { get; set; } = new();

    [Parameter]
    [SupplyParameterFromQuery]
    public string? Reload { get; set; }

    public string MysteryWord { get; set; }

    public LetterHint[][] Guesses { get; set; }

    // For Keyboard display
    public List<LetterHint> AllHints { get; set; } = new List<LetterHint>();

    public Statistics Statistics { get; set; }

    public int GuessCount { get; set; }

    private ElementReference KeyboardDiv;

    private DateTime _gameStartTimeUtc;

    protected override async Task OnParametersSetAsync()
    {
        Statistics = await _storageService.LoadAsync(Settings.CurrentUsername);

        await ResetGameAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime.InvokeVoidAsync("registerGlobalKeyboard", DotNetObjectReference.Create(this));
        }
    }

    public async Task ResetGameAsync()
    {
        AllHints = new List<LetterHint>();
        GuessCount = 0;
        _gameStartTimeUtc = DateTime.UtcNow;

        // Initiate Grid with "Empty" Values
        Guesses = new LetterHint[Settings.MaxGuesses][];
        for (int i = 0; i < Settings.MaxGuesses; i++)
        {
            Guesses[i] = new LetterHint[Settings.WordLength];

            for (int j = 0; j < Settings.WordLength; j++)
            {
                Guesses[i][j] = new LetterHint();
            }
        }

        MysteryWord = await _wordService.GetMysteryWord(Settings.WordLength);
    }

    [JSInvokable]
    public void OnGlobalKey(string key)
    {
        if (!Settings.IsKeyboardActive || GuessCount >= Settings.MaxGuesses)
        {
            return;
        }

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
                ShowMessage(ToastIntent.Error, $"Ungültige Eingabe: Das Wort muss {Settings.WordLength} Buchstaben lang sein.");
            }
            else
            {
                SubmitGuess();
            }
        }
        else
        {
            ShowMessage(ToastIntent.Warning, "Ungültiger Buchstabe!");
        }

        StateHasChanged();
    }

    private async Task SubmitGuess()
    {
        var guess = string.Concat(Guesses[GuessCount].Select(x => x.Letter)).ToUpper();

        if (!await _wordService.ValidateWord(guess))
        {
            ShowMessage(ToastIntent.Error, "Dieses Wort existiert nicht.");
            return;
        }

        try
        {
            Guesses[GuessCount] = _wordService.GetHintsFromGuess(MysteryWord, guess).ToArray();

            AllHints = Guesses.Reverse().SelectMany(x => x).ToList();

            if (Guesses[GuessCount].All(x => x.HintType == LetterHintType.Correct))
            {
                await MysteryWordGuessed();
            }

            GuessCount++;

            if(GuessCount == Settings.MaxGuesses)
            {
                await WordNotGuessed();
            }
        }
        catch (Exception ex)
        {
            //TODO: Specify exception and handle error
            ShowMessage(ToastIntent.Error, ex.Message);
        }

        StateHasChanged();
    }

    public void UpdateBoxedLetters(char letter)
    {
        var currentWord = Guesses[GuessCount];

        currentWord.FirstOrDefault(lh => lh.Letter == null)?.Letter = letter;
        StateHasChanged();
    }

    public void ShowMessage(ToastIntent intent, string errorMessage)
    {
        ToastService.ShowToast(intent, errorMessage);
    }

    public async Task MysteryWordGuessed()
    {
        var intent = ToastIntent.Success;
        ToastService.ShowToast(intent, "Herzlichen Glückwunsch! Sie haben das Wort erraten.");

        await UpdateStatistics(true);
    }

    public async Task WordNotGuessed()
    {
        var intent = ToastIntent.Error;
        ToastService.ShowToast(intent, $"Keine Versuche mehr! Das Wort war: {MysteryWord}.");

        GuessCount++;
        await UpdateStatistics(false);
    }

    public async Task UpdateStatistics(bool won)
    {
        var duration = DateTime.UtcNow - _gameStartTimeUtc;

        Statistics.RegisterGame(
            guesses: GuessCount,
            duration: duration,
            won: true
        );

        await _storageService.SaveAsync(Statistics);
    }
}