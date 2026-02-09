using Domain;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace WordLab.Components.Dialogs;

public partial class StatisticsDialog
{
    [Parameter]
    public Statistics Content { get; set; } = default!;

    [CascadingParameter]
    public required FluentDialog Dialog { get; set; }

    private async Task SaveAsync()
    {
        await Dialog.CloseAsync(Content).ConfigureAwait(false);
    }

    private async Task CloseAsync()
    {
        await Dialog.CancelAsync().ConfigureAwait(ConfigureAwaitOptions.None);
    }
}