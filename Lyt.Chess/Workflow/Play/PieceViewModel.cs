namespace Lyt.Chess.Workflow.Play;

using MinimalChess;

internal partial class PieceViewModel : ViewModel<PieceView>, IDragMovableViewModel
{
    private readonly Piece piece;
    private readonly BoardViewModel boardViewModel;

    private SquareViewModel squareViewModel;
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
        this.ShowAsSelected(false);
    }

    internal SquareViewModel SquareViewModel => this.squareViewModel;

    internal Piece Piece => this.piece;

    internal bool IsSelected => this.isSelected;

    internal void DisableClicks() => this.View.DisableClicks();


    //internal void Select(bool select, bool enforce = false)
    //{
    //    if (!enforce && this.isSelected == select)
    //    {
    //        return;
    //    }

    //    void SelfSelect()
    //    {
    //        bool selected = false;
    //        if (select)
    //        {
    //            if (this.boardViewModel.HasLegalMoves(this))
    //            {
    //                this.ScaleFactor = piece == Piece.BlackPawn || piece == Piece.WhitePawn ? 1.0 : 1.2;
    //                selected = true;
    //            }
    //        }
    //        else
    //        {
    //            this.ScaleFactor = piece == Piece.BlackPawn || piece == Piece.WhitePawn ? 0.7 : 1.0;
    //        }

    //        this.isSelected = selected;
    //        if (selected)
    //        {
    //            this.boardViewModel.OnPieceSelected(this.squareViewModel);
    //        }
    //    }

    //    if (this.boardViewModel.HasSelectedPiece)
    //    {
    //        var selectedSquare = this.boardViewModel.SelectedSquare;
    //        if (selectedSquare.IsEmpty)
    //        {
    //            // Check legal move 
    //            bool isLegalMove = this.boardViewModel.IsLegalMove(selectedSquare, this.squareViewModel);
    //            if (isLegalMove)
    //            {
    //                // Move with NO capture,  From: selected square  To : this square 
    //                this.boardViewModel.MoveNoCapture(from: selectedSquare, to: this.squareViewModel);
    //                this.boardViewModel.ClearSelection();
    //            }
    //        }
    //        else
    //        {
    //            PieceViewModel pieceViewModel = selectedSquare.PieceViewModel;
    //            if (pieceViewModel == this)
    //            {
    //                SelfSelect();
    //            }
    //            else
    //            {
    //                // Check legal move 
    //                bool isLegalMove = this.boardViewModel.IsLegalMove(selectedSquare, this.squareViewModel);
    //                if (isLegalMove)
    //                {
    //                    // Move with capture,  From: selected square  To : this square 
    //                    this.boardViewModel.MoveWithCapture(
    //                        from: selectedSquare, to: this.squareViewModel, capture: this);
    //                    this.boardViewModel.ClearSelection();
    //                }
    //            }
    //        }
    //    }
    //    else
    //    {
    //        SelfSelect();
    //    }
    //}

    // Click on a Piece 
    // if ( board has a selected piece ) 
    //      // Same piece : Deselect Piece, Deselect Square,  Board has no selected piece any longer
    //      // Other piece: 
    //      //      // if legal capture : Capture + Deselect Piece, Deselect Square,  Board has no selected piece any longer
    //      //      // else             : Do nothing: Selections remain
    // else ( no selection ) 
    //      // Clicked piece becomes selection: shows as selected, show square as selected, update board 
    public void OnClicked(bool _)
    {
        var vm = this.squareViewModel;
        Debug.WriteLine(" Click on Piece at Square:  Rank: " + vm.Rank.ToString() + " File:  " + vm.File.ToString());

        void ClearSelection ()
        {
            this.ShowAsSelected(selected: false);
            this.SquareViewModel.ShowAsSelected(false);
            this.boardViewModel.ClearSelection();
            this.boardViewModel.HideAllLegalMoves();
        }

        if (this.boardViewModel.HasSelectedPiece)
        {
            var selectedSquare = this.boardViewModel.SelectedSquare; 
            var selectedPieceViewModel = selectedSquare.PieceViewModel;
            if (selectedPieceViewModel == this)
            {
                ClearSelection();
            }
            else
            {
                // Capture if legal 
                bool isLegalMove = this.boardViewModel.IsLegalMove(selectedSquare, this.squareViewModel);
                if (isLegalMove)
                {
                    // Move with capture,  From: selected square  To : this square 
                    this.boardViewModel.MoveWithCapture(from: selectedSquare, to: this.squareViewModel, capture: this);
                    ClearSelection();
                }
                // else : nothing 
            }
        }
        else
        {
            // Set as new selected piece 
            this.ShowAsSelected(selected: true);
            this.SquareViewModel.ShowAsSelected(true);
            this.boardViewModel.SetSelection(this);
            this.boardViewModel.ShowLegalMoves(this.SquareViewModel);
        }
    }

    internal void ShowAsSelected(bool selected = true)
    {
        bool isPawn = piece == Piece.BlackPawn || piece == Piece.WhitePawn;
        this.ScaleFactor =
            isPawn ?
                (selected ? 1.0 : 0.7) :
                (selected ? 1.18 : 1.0);
    }

    internal void MoveToSquare(SquareViewModel moveToSquareViewModel) => this.squareViewModel = moveToSquareViewModel;

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
