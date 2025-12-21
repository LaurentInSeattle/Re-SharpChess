namespace Lyt.Chess.Workflow.Play;

internal partial class PieceViewModel : ViewModel<PieceView>, IDragMovableViewModel
{
    private readonly Piece piece;
    private readonly BoardViewModel boardViewModel;
    private readonly SquareViewModel squareViewModel;

    private bool isSelected;

    [ObservableProperty]
    private CroppedBitmap imageSource;

    [ObservableProperty]
    private double scaleFactor;

    public PieceViewModel(Piece piece, BoardViewModel boardViewModel, SquareViewModel squareViewModel)
    {
        this.piece = piece;
        this.boardViewModel = boardViewModel;
        this.squareViewModel = squareViewModel;
        this.imageSource = PieceImageProvider.GetFromFen(Notation.ToChar(piece));
        this.squareViewModel.PlacePiece(this);
        this.Select(select: false, enforce: true);
    }

    internal bool IsSelected => this.isSelected;

    internal void Select(bool select, bool enforce = false)
    {
        if (!enforce && this.isSelected == select)
        {
            return;
        }

        void SelfSelect ()
        {
            bool selected = false;
            if (select)
            {
                if (this.boardViewModel.HasLegalMoves(this))
                {
                    this.ScaleFactor = piece == Piece.BlackPawn || piece == Piece.WhitePawn ? 1.0 : 1.2;
                    selected = true;
                }
            }
            else
            {
                this.ScaleFactor = piece == Piece.BlackPawn || piece == Piece.WhitePawn ? 0.7 : 1.0;
            }

            this.isSelected = selected;
            if (selected)
            {
                this.boardViewModel.OnPieceSelected(this.squareViewModel);
            }
        }

        if (this.boardViewModel.HasSelectedSquare)
        {
            var selectedSquare = this.boardViewModel.SelectedSquare;
            if (selectedSquare.IsEmpty)
            {
                // TODO : Check legal move 
                bool isLegalMove = false;
                if (isLegalMove)
                {
                    this.boardViewModel.ClearSelection();

                    // Move with NO capture,  From: selected square  To : this square 
                    this.boardViewModel.MoveNoCapture(from: selectedSquare, to: this.squareViewModel);
                }
            }
            else
            {
                PieceViewModel pieceViewModel = selectedSquare.PieceViewModel;
                if (pieceViewModel == this)
                {
                    SelfSelect();
                }
                else
                {
                    // TODO : Check legal move 
                    bool isLegalMove = false;
                    if (isLegalMove)
                    {
                        this.boardViewModel.ClearSelection();

                        // Move with capture,  From: selected square  To : this square 
                        this.boardViewModel.MoveWithCapture(
                            from: selectedSquare, to: this.squareViewModel, capture: this);
                    }
                }
            } 
        }
        else
        {
            SelfSelect ();
        }
    }

    public void OnClicked(bool _) 
    {
        var vm = this.squareViewModel; 
        Debug.WriteLine(" Click on Piece at Square:  Rank: " + vm.Rank.ToString() + " File:  " + vm.File.ToString());
        this.Select(select: !this.isSelected);
    } 

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
