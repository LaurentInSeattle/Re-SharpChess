namespace Lyt.Chess.Workflow.Play;

public sealed partial class ScoreViewModel :
    ViewModel<ScoreView>,
    IRecipient<ModelUpdatedMessage>
{
    private readonly ChessModel chessModel;

    [ObservableProperty]
    private string clockTop = string.Empty;

    [ObservableProperty]
    private string clockBottom = string.Empty;

    [ObservableProperty]
    private string captureTop = string.Empty;

    [ObservableProperty]
    private string captureBottom = string.Empty;

    [ObservableProperty]
    private string scoreOrEndGame = string.Empty;

    public ScoreViewModel(ChessModel chessModel)
    {
        this.chessModel = chessModel;
        this.Subscribe<ModelUpdatedMessage>();
        this.Clear();
    }

    internal PlayerColor SideToPlay => this.chessModel.Engine.SideToMove;

    public void Receive(ModelUpdatedMessage message)
        => Dispatch.OnUiThread(() => { this.ReceiveOnUiThread(message); });

    public void ReceiveOnUiThread(ModelUpdatedMessage message)
    {
        Debug.WriteLine(" BoardViewModel Message: " + message.Hint.ToString() + ":  " + message.Parameter?.ToString());

        var game = this.chessModel.GameInProgress;
        if (game is null)
        {
            Debug.WriteLine(" BoardViewModel Message: No Game!");
            return;
        }

        switch (message.Hint)
        {
            default:
            case UpdateHint.None:
                break;

            case UpdateHint.NewGame:
                if (message.Parameter is Board _)
                {
                    this.Clear();
                }
                break;

            case UpdateHint.IsCheckmate:
                if (message.Parameter is PlayerColor playerColorIsCheckmate)
                {
                    this.ScoreOrEndGame = string.Format("{0}: Checkmate", playerColorIsCheckmate);
                }

                break;

            case UpdateHint.IsStalemate:
                if (message.Parameter is PlayerColor _)
                {
                    this.ScoreOrEndGame = string.Format("Draw: Stalemate");
                }

                break;

            case UpdateHint.IsChecked:
                if ((message.Parameter is PlayerColor playerColorIsChecked) &&
                    (playerColorIsChecked == this.SideToPlay))
                {
                    this.ScoreOrEndGame = string.Format("{0}: Check" , playerColorIsChecked);
                }
                else
                {
                    this.ScoreOrEndGame = string.Empty;
                }

                break;

            case UpdateHint.Capture:
                if (message.Parameter is byte square)
                {
                }
                break;
        }
    }

    private void Clear()
    {
        this.ClockTop = string.Empty;
        this.ClockBottom = string.Empty;
        this.CaptureTop = string.Empty;
        this.CaptureBottom = string.Empty;
        this.ScoreOrEndGame = string.Empty;
    }
}