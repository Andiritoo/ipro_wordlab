using Domain;
using Infrastructure.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using WordLab.Components.Dialogs;

namespace WordLab.Components.Layout;

public partial class NavMenu
{
    [Inject]
    public IDialogService _dialogService { get; set; }

    [Inject]
    public NavigationManager _navigationManager { get; set; }

    [Inject]
    public StatisticStorageService _storageService { get; set; }

    [CascadingParameter(Name = "Settings")]
    public WordleSettings? Settings { get; set; }

    /// <summary>
    /// Opens the dialog that shows the Statistics
    /// </summary>
    public async Task OpenStatisticsDialog()
    {
        Settings.IsKeyboardActive = false;

        var statistics = await _storageService.LoadAsync(Settings?.CurrentUsername ?? string.Empty);

        DialogParameters parameters = new()
        {
            Width = "500px",
            TrapFocus = true,
            Modal = true,
            PreventDismissOnOverlayClick = true,
            PreventScroll = true,
        };

        IDialogReference dialog = await _dialogService.ShowDialogAsync<StatisticsDialog>(statistics, parameters);
        DialogResult? result = await dialog.Result;

        Settings.IsKeyboardActive = true;

        if (result.Data is Statistics stats)
        {
            _navigationManager.NavigateTo($"/?reload={Guid.NewGuid()}");
        }
    }

    /// <summary>
    /// Opens the dialog to configure the settings
    /// Updates the GlobalCascadingParameter if the Settings changed and reloads the page
    /// </summary>
    public async Task OpenSettingsDialog()
    {
        Settings.IsKeyboardActive = false;

        DialogParameters parameters = new()
        {
            Width = "500px",
            TrapFocus = true,
            Modal = true,
            PreventDismissOnOverlayClick = true,
            PreventScroll = true,
        };

        IDialogReference dialog = await _dialogService.ShowDialogAsync<SettingsDialog>(Settings, parameters);
        DialogResult? result = await dialog.Result;

        Settings.IsKeyboardActive = true;

        if (result.Data is WordleSettings settings)
        {
            Settings = settings; 
            _navigationManager.NavigateTo($"/?reload={Guid.NewGuid()}");
        }

        StateHasChanged();
    }
}