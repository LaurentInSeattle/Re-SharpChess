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
    private string score = string.Empty;

    [ObservableProperty]
    private string endGame = string.Empty;

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
        Debug.WriteLine(" ScoreViewModel Message: " + message.Hint.ToString() + ":  " + message.Parameter?.ToString());

        var game = this.chessModel.GameInProgress;
        if (game is null)
        {
            Debug.WriteLine(" ScoreViewModel Message: No Game!");
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
                    this.EndGame = string.Format("{0}: Checkmate", playerColorIsCheckmate);
                }

                break;

            case UpdateHint.IsStalemate:
                if (message.Parameter is PlayerColor _)
                {
                    this.EndGame = string.Format("Draw: Stalemate");
                }

                break;

            case UpdateHint.IsChecked:
                if ((message.Parameter is PlayerColor playerColorIsChecked) &&
                    (playerColorIsChecked == this.SideToPlay))
                {
                    this.EndGame = string.Format("{0}: Check" , playerColorIsChecked);
                }
                else
                {
                    this.EndGame = string.Empty;
                }

                break;

            case UpdateHint.Capture:
                this.UpdateScores(); 
                break;
        }
    }

    private void UpdateScores()
    {
        var game = this.chessModel.GameInProgress;
        if (game is null)
        {
            this.CaptureTop = string.Empty;
            this.CaptureBottom = string.Empty;
            return;
        }

        var match = game.Match;
        int whiteScore = match.WhiteScore;
        int blackScore = match.BlackScore;
        bool isPlayingWhite = match.IsPlayingWhite; 
        this.CaptureTop = isPlayingWhite ? blackScore.ToString() : whiteScore.ToString();
        this.CaptureBottom = ! isPlayingWhite ? blackScore.ToString() : whiteScore.ToString();
        if (match.IsTied)
        {
            this.Score = "Tied";
        }
        else
        {
            string lead = match.Lead.ToString();
            if (match.IsLeading)
            {
                this.Score = "Leading: +" + lead;
            }
            else
            {
                this.Score = "Trailing: -" + lead;
            }
        } 
    }

    private void Clear()
    {
        this.ClockTop = string.Empty;
        this.ClockBottom = string.Empty;
        this.CaptureTop = string.Empty;
        this.CaptureBottom = string.Empty;
        this.Score = string.Empty;
        this.EndGame = string.Empty;
    }
}