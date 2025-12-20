namespace Lyt.Chess.Workflow.Play;

public sealed partial class PlayViewModel :
    ViewModel<PlayView>,
    IRecipient<ModelUpdatedMessage>,
    IRecipient<ToolbarCommandMessage>
{
    private readonly ChessModel chessModel;

    private BoardViewModel boardViewModel;
    private bool boardCreated;

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
        this.boardViewModel = new BoardViewModel();
        _ = this.boardViewModel.CreateViewAndBind();
        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<ModelUpdatedMessage>();
    }

    public override void Activate(object? _)
    {
        Task.Run(async () =>
        {
            bool ready = await this.chessModel.InitializeEngine();
            if (ready)
            {

                new ModelUpdatedMessage(UpdateHint.EngineReady, ready).Publish();
                this.chessModel.NewGame();
                this.chessModel.GameIsActive(isActive: true);
            }
            else
            {
                // TODO 
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
        });
    }

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
        => Dispatch.OnUiThread(() => { this.ReceiveOnUiTHread(message); });

    public void ReceiveOnUiTHread(ModelUpdatedMessage message)
    {
        Debug.WriteLine(message.Hint.ToString() + ":  " + message.Parameter?.ToString());

        switch (message.Hint)
        {
            default:
            case UpdateHint.None:
                break;

            case UpdateHint.EngineReady:
                this.CreateEmptyBoard();
                break;

            case UpdateHint.NewGame:
                if (message.Parameter is Board board)
                {
                    this.PopulateBoard(board);
                }
                break;

            case UpdateHint.EnginePlayed:
                if (message.Parameter is Move move)
                {
                    // this.AutoPlay(move);
                }
                break;

            case UpdateHint.LegalMoves:
                if (message.Parameter is LegalMoves legalMoves)
                {
                    // this.SaveLegalMoves(legalMoves);
                }
                break;

            case UpdateHint.CapturedPiece:
                if (message.Parameter is Piece piece)
                {
                    // this.CapturedPiece(piece);
                }
                break;
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

    private void CreateEmptyBoard()
    {
        if (!this.boardCreated)
        {
            this.boardViewModel.CreateEmpty();
            this.View.BoardViewbox.Child = this.boardViewModel.View;
            this.boardCreated = true;
        }
    }

    private void PopulateBoard(Board board)
    {
        this.boardViewModel.Populate(board);
    }

    // public void Receive(LanguageChangedMessage message) => this.Localize();

    //private void Localize()
    //{
    //}
}
