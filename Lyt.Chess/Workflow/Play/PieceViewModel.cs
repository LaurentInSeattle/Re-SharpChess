namespace Lyt.Chess.Workflow.Play;

internal partial class PieceViewModel : ViewModel<PieceView>, IDragMovableViewModel
{
    private readonly char pieceKey;
    private readonly Piece piece;
    private readonly BoardViewModel boardViewModel;

    private SquareViewModel squareViewModel;
    private bool isSelected;

    [ObservableProperty]
    private CroppedBitmap imageSource;

    [ObservableProperty]
    private double scaleFactor;

    public PieceViewModel(char pieceKey, BoardViewModel boardViewModel, SquareViewModel squareViewModel)
    {
        this.pieceKey = pieceKey;
        this.piece = Notation.ToPiece(pieceKey);
        this.boardViewModel = boardViewModel;
        this.squareViewModel = squareViewModel;
        this.imageSource = PieceImageProvider.GetFromFen(pieceKey);
        this.squareViewModel.PlacePiece(this);
        this.Select(select:false, enforce: true);
    }

    public void Select(bool select, bool enforce = false)
    {
        if (!enforce && this.isSelected == select)
        {
            return;
        }

        if ( select)
        {
            this.ScaleFactor = piece == Piece.BlackPawn || piece == Piece.WhitePawn ? 1.0 : 1.2;
        }
        else
        {
            this.ScaleFactor = piece == Piece.BlackPawn || piece == Piece.WhitePawn ? 0.7 : 1.0;
        }

        this.isSelected = select;
        this.boardViewModel.OnPieceSelected(this.squareViewModel);
    }

    public void OnClicked(bool isRightClick) => this.Select(select: !this.isSelected);

    public bool OnBeginMove(Point fromPoint)
    {
        // TODO 
        return true;
    }

    // TODO 
    public void OnMove(Point fromPoint, Point toPoint) { }

    // TODO 
    public void OnEndMove(Point fromPoint, Point toPoint)
    {
    }

    // For interface compliance, should do nothing
    public void OnEntered() { }

    // For interface compliance, should do nothing
    public void OnExited() { }

    // For interface compliance, should do nothing
    public void OnLongPress() { }

}
