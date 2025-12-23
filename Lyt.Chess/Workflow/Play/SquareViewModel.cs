namespace Lyt.Chess.Workflow.Play;

internal partial class SquareViewModel : ViewModel<SquareView>
{
    private readonly BoardViewModel boardViewModel;
    private PieceViewModel? pieceViewModel;

    [ObservableProperty]
    private SolidColorBrush background;

    [ObservableProperty]
    private bool isValidMove;

    [ObservableProperty]
    private bool isInvalidMove;

    [ObservableProperty]
    private bool isInCheck;

    public SquareViewModel(BoardViewModel boardViewModel, int rank, int file)
    {
        this.boardViewModel = boardViewModel;
        this.Rank = rank;
        this.File = file;
        this.pieceViewModel = null;

        // TODO: Use images textures for the squares
        // For now we use simple colors for the squares
        PlayerColor squareColor =
            (this.Rank + this.File) % 2 == 1 ?
                PlayerColor.White :
                PlayerColor.Black;
        this.Background =
            squareColor == PlayerColor.White ?
                new SolidColorBrush(Colors.BurlyWood) :
                new SolidColorBrush(Colors.SaddleBrown);
        this.IsValidMove = false;
        this.IsInvalidMove = false;
        this.IsInCheck = false;
    }

    internal int Index => this.Rank * 8 + this.File;

    internal int Rank { get; private set; }

    internal int File { get; private set; }

    internal bool IsEmpty => this.pieceViewModel is null;

    internal PieceViewModel PieceViewModel
        => this.pieceViewModel is not null ?
                this.pieceViewModel :
                throw new Exception("Should have checked IsEmpty property");

    internal void Select(bool select) => this.pieceViewModel?.Select(select);

    internal void ShowAsInCheck(bool inCheck = true) => this.IsInCheck = inCheck;

    internal void ShowAsLegal(bool legal = true) => this.IsValidMove = legal;

    internal bool OnClicked()
    {
        Debug.WriteLine(" Click on Square:  Rank: " + this.Rank.ToString() + " File:  " + this.File.ToString());

        if (this.IsEmpty)
        {
            if (this.boardViewModel.HasSelectedSquare)
            {
                // Click on empty square when there is a selection 
                var selectedSquare = this.boardViewModel.SelectedSquare;

                // TODO : Check legal move 
                bool isLegalMove = true;
                if (isLegalMove)
                {
                    this.boardViewModel.ClearSelection();

                    // Move without capture,  From: selected square  To : this square 
                    this.boardViewModel.MoveNoCapture(from: selectedSquare, to: this);
                }
            }
            // ELSE: Click on empty square when there is no selection: Do nothing  
        }
        else
        {
            if (this.boardViewModel.HasSelectedSquare)
            {
                var selectedSquare = this.boardViewModel.SelectedSquare;
                PieceViewModel selectedPieceViewModel = selectedSquare.PieceViewModel;
                if (selectedPieceViewModel == this.PieceViewModel)
                {
                    this.Select(!this.PieceViewModel.IsSelected);
                }
                else
                {
                    // TODO : Check legal move 
                    bool isLegalMove = true;
                    if (isLegalMove)
                    {
                        this.boardViewModel.ClearSelection();

                        // Move with capture,  From: selected square  To : this square 
                        this.boardViewModel.MoveWithCapture(
                            from: selectedSquare, to: this, capture: selectedPieceViewModel);
                    }
                }
            }
            else
            {
                // Click on occupied square when no selection: Becomes the new selection 
                this.Select(select: true);
            }
        }


        return true;
    }

    internal void PlacePiece(PieceViewModel pieceViewModel)
    {
        if (this.pieceViewModel is not null)
        {
            throw new Exception("Trying to place a piece on an already occupied square.");
        }

        this.pieceViewModel = pieceViewModel;
        this.pieceViewModel.MoveToSquare(this);
    }

    internal PieceViewModel RemovePiece()
    {
        if (this.pieceViewModel is null)
        {
            throw new Exception("Trying to remove the piece of an empty square.");
        }

        var vm = this.pieceViewModel;
        this.pieceViewModel = null;
        return vm;
    }

    internal void DisableMoves() 
    {
        this.View.DisableClicks();
        if (this.pieceViewModel is null)
        {
            return; 
        }

        this.pieceViewModel.DisableClicks() ;
    }
}
