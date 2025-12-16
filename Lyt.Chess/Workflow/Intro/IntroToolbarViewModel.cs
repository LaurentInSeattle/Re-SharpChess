namespace Lyt.Chess.Workflow.Intro;

public sealed partial class IntroToolbarViewModel : ViewModel<IntroToolbarView>
{
#pragma warning disable CA1822 // Mark members as static
    [RelayCommand]
    public void OnNext()
#pragma warning restore CA1822 
    {
        var chessModel = App.GetRequiredService<ChessModel>();
        chessModel.IsFirstRun = false;
        chessModel.Save();

        ViewSelector<ActivatedView>.Select(ActivatedView.Setup);
    }
}
