namespace Lyt.Chess.Workflow.Play;

using MinimalChess;

internal partial class PieceViewModel : ViewModel<PieceView>, IDragMovableViewModel
{
    private readonly Piece piece;
    private readonly BoardViewModel boardViewModel;

    private bool canBeClicked;
    private SquareViewModel squareViewModel;

    [ObservableProperty]
    private CroppedBitmap imageSource;

    [ObservableProperty]
    private double scaleFactor;

    public PieceViewModel(Piece piece, BoardViewModel boardViewModel, SquareViewModel squareViewModel)
    {
        this.piece = piece;
        this.boardViewModel = boardViewModel;
        this.squareViewModel = squareViewModel;
        this.canBeClicked = true;
        this.imageSource = PieceImageProvider.GetFromFen(piece.ToChar());
        this.squareViewModel.PlacePiece(this);
        this.ShowAsSelected(false);
    }

    internal SquareViewModel SquareViewModel => this.squareViewModel;

    internal Piece Piece => this.piece;

    internal void DisableClicks() => this.canBeClicked = false;

    // Invoked from view 
    public void OnClicked(bool _) => this.OnClicked();

    // Click on a Piece 
    // if ( board has a selected piece ) 
    //      // Same piece : Deselect Piece, Deselect Square,  Board has no selected piece any longer
    //      // Other piece: 
    //      //      // if legal capture : Capture + Deselect Piece, Deselect Square,  Board has no selected piece any longer
    //      //      // else             : Do nothing: Selections remain
    // else ( no selection ) 
    //      // Clicked piece becomes selection: shows as selected, show square as selected, update board 
    //
    // Can also be invoked from the square view model
    internal void OnClicked()
    {
        if (!this.canBeClicked)
        {
            Debug.WriteLine(" Click on Piece :  Disabled");
            return;
        }

        PlayerColor playerToMove = this.boardViewModel.SideToPlay; 
        var vm = this.squareViewModel;
        Debug.WriteLine(" Click on Piece at Square:  Rank: " + vm.Rank.ToString() + " File:  " + vm.File.ToString());

        if (this.boardViewModel.HasSelectedPiece)
        {
            var selectedSquare = this.boardViewModel.SelectedSquare;
            var selectedPieceViewModel = selectedSquare.PieceViewModel;
            if (selectedPieceViewModel == this)
            {
                this.boardViewModel.ClearSelection();
            }
            else
            {
                // Capture if legal 
                bool isLegalMove = this.boardViewModel.IsLegalMove(selectedSquare, this.squareViewModel);
                if (isLegalMove)
                {
                    // Move with capture,  From: selected square  To : this square 
                    var pieceViewModel = selectedSquare.PieceViewModel;
                    pieceViewModel.ShowAsSelected(selected: false);
                    this.boardViewModel.MoveWithCapture(from: selectedSquare, to: this.squareViewModel, capture: this);
                    this.boardViewModel.ClearSelection();
                }
                else
                {
                    // Set as new selected piece if piece is ours 
                    if (playerToMove == selectedPieceViewModel.Piece.Color())
                    {
                        this.boardViewModel.SetSelection(this);
                    } 
                }
            }
        }
        else
        {
            // Set as new selected piece if piece is ours 
            if (playerToMove == this.Piece.Color())
            {
                this.boardViewModel.SetSelection(this);
            }
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

    #region LATER : Drag and Drop 

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

    #endregion LATER : Drag and Drop 
}
