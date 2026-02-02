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

    /// <summary>
    /// This method is part of the Blazor Lifecycle and is executed when parameters are set.
    /// Statistics are loaded and a new game is started.
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        Statistics = await _storageService.LoadAsync(Settings.CurrentUsername);

        await ResetGameAsync();
    }

    /// <summary>
    /// This method is part of the Blazor Lifecycle and is executed when parameters are set.
    /// Registers the JavaScript function for global keyboard input.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await _jsRuntime.InvokeVoidAsync("registerGlobalKeyboard", DotNetObjectReference.Create(this));
        }
    }

    /// <summary>
    /// Asynchronously resets the game state to start a new game session.
    /// </summary>
    /// <remarks>This method clears all previous guesses, resets the guess count, initializes the game grid,
    /// and retrieves a new mystery word based on the current settings. Call this method to begin a new game or to
    /// restart after a completed session.</remarks>
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

    /// <summary>
    /// Handles a global keyboard event by processing the specified key input according to the current game state.
    /// </summary>
    /// <remarks>The method ignores input if keyboard interaction is disabled or the maximum number of guesses has been
    /// reached.</remarks>
    /// <param name="key">The key value representing the user's input. This can be a single letter, "Backspace", or "Enter".</param>
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

    /// <summary>
    /// Processes the current guess, validates it as a word, updates hint information, and advances the game state
    /// accordingly.
    /// </summary>
    /// <remarks>If the guess is not a valid word, an error message is displayed and the game state is not
    /// updated. If the guess is correct or the maximum number of guesses is reached, the appropriate end-of-game logic
    /// is triggered. This method should be called in response to a user submitting a guess.</remarks>
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
            else
            {
                GuessCount++;

                if (GuessCount == Settings.MaxGuesses)
                {
                    await WordNotGuessed();
                }
            }
        }
        catch (Exception ex)
        {
            //TODO: Specify exception and handle error
            ShowMessage(ToastIntent.Error, ex.Message);
        }

        StateHasChanged();
    }

    /// <summary>
    /// Displays a Toast Message.
    /// </summary>
    /// <param name="intent">Intent of the toast e.g. Warning, Error or Success</param>
    /// <param name="message">Message to be displayed in the toast.</param>
    public void ShowMessage(ToastIntent intent, string message)
    {
        ToastService.ShowToast(intent, message);
    }

    /// <summary>
    /// Wordle succeded, user correctly guessed the mystery word.
    /// Updates Statistics and shows a success toast.
    /// </summary>
    public async Task MysteryWordGuessed()
    {
        var intent = ToastIntent.Success;
        ToastService.ShowToast(intent, "Herzlichen Glückwunsch! Sie haben das Wort erraten.");

        GuessCount++;
        await UpdateStatistics(true);
    }

    /// <summary>
    /// Wordle failed, user did not guess the mystery word.
    /// Updates Statistics and shows a fail toast.
    /// </summary>
    public async Task WordNotGuessed()
    {
        var intent = ToastIntent.Error;
        ToastService.ShowToast(intent, $"Keine Versuche mehr! Das Wort war: {MysteryWord}.");

        GuessCount++;
        await UpdateStatistics(false);
    }

    /// <summary>
    /// Updates the statistics based on the result of the Wordle game.
    /// </summary>
    /// <param name="won">Represents whether or not the mystery word was found.</param>
    public async Task UpdateStatistics(bool won)
    {
        var duration = DateTime.UtcNow - _gameStartTimeUtc;

        Statistics.RegisterGame(GuessCount, duration, won);

        await _storageService.SaveAsync(Statistics);
    }
}