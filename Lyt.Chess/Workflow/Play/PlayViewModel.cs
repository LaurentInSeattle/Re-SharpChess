namespace Lyt.Chess.Workflow.Play;

public sealed partial class PlayViewModel : ViewModel<PlayView>,
    IRecipient<ToolbarCommandMessage>
{
    private readonly ChessModel chessModel;

    public WriteableBitmap? Image;

    [ObservableProperty]
    private double canvasWidth;

    [ObservableProperty]
    private double canvasHeight;

    [ObservableProperty]
    private double zoomFactor;

    public PlayViewModel(ChessModel chessModel)
    {
        this.chessModel = chessModel;
        this.Subscribe<ToolbarCommandMessage>();
    }

    public override void Activate(object? _)
        => this.chessModel.GameIsActive(isActive: true);

    public override void Deactivate()
    {
        // Force a full save on deactivation
        this.chessModel.PausePlaying();
        this.chessModel.SaveGame();
        this.chessModel.GameIsActive(isActive: false);
    }

    public void Receive(ToolbarCommandMessage message)
    {
        if ((message.Command == ToolbarCommandMessage.ToolbarCommand.PlayFullscreen) ||
            (message.Command == ToolbarCommandMessage.ToolbarCommand.PlayWindowed))
        {
        }
    }


    internal void ResumeGame()
    {
    }

    internal void StartNewGame()
    {
        this.chessModel.SaveGame();

        this.UpdateToolbarAndGameState();
    }

    private void UpdateToolbarAndGameState()
    {
        this.chessModel.GameIsActive();
        this.chessModel.ResumePlaying();
        Schedule.OnUiThread(50, () =>
        {
            //new PuzzleChangedMessage(PuzzleChange.Start).Publish();
            //new PuzzleChangedMessage(PuzzleChange.Progress, this.chessModel.GetPuzzleProgress()).Publish();
        }, DispatcherPriority.Background);

    }

    // public void Receive(LanguageChangedMessage message) => this.Localize();

    //private void Localize()
    //{
    //}
}
