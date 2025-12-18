namespace Lyt.Chess.Workflow.Play;

internal partial class PieceViewModel : ViewModel<PieceView>
{
    private readonly char pieceKey;

    [ObservableProperty]
    private CroppedBitmap imageSource;

    [ObservableProperty]
    private double scaleFactor;

    public PieceViewModel(char pieceKey)
    {
        this.pieceKey = pieceKey;
        this.imageSource = PieceImageProvider.GetFromFen(pieceKey);
        var piece = Notation.ToPiece(pieceKey);
        this.scaleFactor = piece == Piece.BlackPawn || piece == Piece.WhitePawn ? 0.8 :  1.0;
    }
}
