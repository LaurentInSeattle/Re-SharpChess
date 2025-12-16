namespace Lyt.Chess.Workflow.Setup;

using static ApplicationMessagingExtensions;
using static ToolbarCommandMessage;

public sealed partial class SetupToolbarViewModel : ViewModel<SetupToolbarView>
{
#pragma warning disable CA1822 // Mark members as static

    [RelayCommand]
    public void OnRemoveFromCollection() => Command(ToolbarCommand.RemoveFromCollection);
    
    [RelayCommand]
    public void OnSaveToDesktop() => Command(ToolbarCommand.CollectionSaveToDesktop);

#pragma warning restore CA1822
}
