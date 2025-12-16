namespace Lyt.Chess.Workflow.Play;

public sealed partial class PlayToolbarViewModel: ViewModel<PlayToolbarView>
    // , IRecipient<PuzzleChangedMessage>
{
    private readonly ChessModel chessModel;

    private bool showEdges;

    [ObservableProperty]
    private double backgroundSliderValue;

    [ObservableProperty]
    private string progress = "-" ;

    public PlayToolbarViewModel(ChessModel chessModel)
    {
        this.chessModel = chessModel;
        // this.Subscribe<PuzzleChangedMessage>();    
    }

    //public void Receive(PuzzleChangedMessage message)
    //{
    //    switch (message.Change)
    //    {
    //        default:
    //            return;

    //        case PuzzleChange.Start:
    //            this.View.ZoomController.SetMin(); 
    //            break;

    //        case PuzzleChange.Progress:
    //            this.Progress = string.Format( "{0:D} %", (int) message.Parameter);
    //            break;
    //    }
    //}

#pragma warning disable CA1822 
    // Mark members as static
    // Relay commands cannot be static
    
    [RelayCommand]
    public void OnFullscreen() =>
        new ToolbarCommandMessage(ToolbarCommandMessage.ToolbarCommand.PlayFullscreen).Publish();

#pragma warning restore CA1822 // Mark members as static
}
