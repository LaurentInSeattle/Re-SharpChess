namespace Lyt.Chess.Workflow.Play;

public sealed partial class ScoreViewModel :
    ViewModel<ScoreView>,
    IRecipient<ModelUpdatedMessage>
{
    private readonly ChessModel chessModel;

    [ObservableProperty]
    private string clockTop;

    [ObservableProperty]
    private string clockBottom;

    [ObservableProperty]
    private string captureTop;

    [ObservableProperty]
    private string captureBottom;

    [ObservableProperty]
    private string scoreOrEndGame;

    public ScoreViewModel(ChessModel chessModel)
    {
        this.chessModel = chessModel;
        this.Subscribe<ModelUpdatedMessage>();

        this.ClockTop = string.Empty;
        this.ClockBottom = string.Empty;
        this.CaptureTop = string.Empty;
        this.CaptureBottom = string.Empty;
        this.ScoreOrEndGame = string.Empty;
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
                if (message.Parameter is Board boardNew)
                {
                }
                break;

            case UpdateHint.IsCheckmate:
                //if (message.Parameter is Board boardNew)
                //{
                //}
                break;

            case UpdateHint.IsStalemate:
                //if (message.Parameter is Board boardNew)
                //{
                //}
                break;

            case UpdateHint.IsChecked:
                if (message.Parameter is PlayerColor playerColor)
                {
                    // this.UpdateCheckedStatus(playerColor);
                }
                break;

            case UpdateHint.Capture:
                if (message.Parameter is byte square)
                {
                }
                break;
        }
    }
}