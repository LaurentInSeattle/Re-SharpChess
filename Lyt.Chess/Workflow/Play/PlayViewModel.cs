namespace Lyt.Chess.Workflow.Play;

public sealed partial class PlayViewModel :
    ViewModel<PlayView>,
    IRecipient<ModelUpdatedMessage>,
    IRecipient<ToolbarCommandMessage>
{
    private readonly ChessModel chessModel;
    private readonly BoardViewModel boardViewModel;

    private bool boardCreated;

    public PlayViewModel(ChessModel chessModel)
    {
        this.chessModel = chessModel;
        this.boardViewModel = new BoardViewModel(this.chessModel);
        _ = this.boardViewModel.CreateViewAndBind();
        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<ModelUpdatedMessage>();
    }

    public override void Activate(object? _) => this.chessModel.GameIsActive(isActive: true);

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

    public void Receive(ModelUpdatedMessage message)
        => Dispatch.OnUiThread(() => { this.ReceiveOnUiThread(message); });

    public void ReceiveOnUiThread(ModelUpdatedMessage message)
    {
        Debug.WriteLine(" PlayViewModel Message: " + message.Hint.ToString() + ":  " + message.Parameter?.ToString());

        switch (message.Hint)
        {
            default:
            case UpdateHint.None:
                break;

            case UpdateHint.EngineReady:
                this.CreateEmptyBoard();
                break;

            case UpdateHint.IsCheckmate:
                if (message.Parameter is PlayerColor playerColorLoser)
                {
                    this.EndGame(isWin: true , playerColorLoser);
                }
                break;

            case UpdateHint.IsStalemate:
                if (message.Parameter is PlayerColor playerColorStale)
                {
                    this.EndGame(isWin: false, playerColorStale);
                }
                break;

        }
    }

    private void EndGame(bool isWin, PlayerColor playerColor)
    {
        Debug.WriteLine(" PlayViewModel End Game: " + isWin.ToString() + ":  " + playerColor.ToString());
        this.boardViewModel.DisableMoves () ;
    }

    internal void ResumeGame()
    {
        // TODO
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
            // TODO
        }, DispatcherPriority.Background);

    }

    private void CreateEmptyBoard()
    {
        if (!this.boardCreated)
        {
            this.boardViewModel.CreateOrEmpty(showForWhite: true);
            this.View.BoardViewbox.Child = this.boardViewModel.View;
            this.boardCreated = true;
        }
    }

    // public void Receive(LanguageChangedMessage message) => this.Localize();

    //private void Localize()
    //{
    //}
}
