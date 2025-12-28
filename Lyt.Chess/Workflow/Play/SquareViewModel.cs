namespace Lyt.Chess.Workflow.Play;

internal partial class SquareViewModel : ViewModel<SquareView>
{
    private readonly BoardViewModel boardViewModel;
    private PieceViewModel? pieceViewModel;
    private bool canBeClicked;

    [ObservableProperty]
    private SolidColorBrush background;

    [ObservableProperty]
    private bool isSelected;

    [ObservableProperty]
    private bool isLastMove;

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

        this.Clear();
    }

    internal void Clear()
    {
        this.pieceViewModel = null;
        this.canBeClicked = true;
        this.IsSelected = false;
        this.IsValidMove = false;
        this.IsLastMove = false;
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

    internal void ShowAsSelected(bool select)
    {
        this.IsSelected = select;

        // Can be null ! 
        this.pieceViewModel?.ShowAsSelected(select);
    }

    internal void ShowAsInCheck(bool inCheck = true) => this.IsInCheck = inCheck;

    internal void ShowAsLegal(bool legal = true) => this.IsValidMove = legal;

    // Invoked from view 
    internal bool OnClicked()
    {
        if (!this.canBeClicked)
        {
            Debug.WriteLine(" Click on Square: Disabled");
            return false;
        }

        Debug.WriteLine(" Click on Square:  Rank: " + this.Rank.ToString() + " File:  " + this.File.ToString());

        if (this.IsEmpty)
        {
            if (this.boardViewModel.HasSelectedPiece)
            {
                // Click on empty square when there is a selection 
                var selectedSquare = this.boardViewModel.SelectedSquare;

                // Check legal move
                bool isLegalMove = this.boardViewModel.IsLegalMove(selectedSquare, this);
                if (isLegalMove)
                {
                    // Move without capture,  From: selected square  To : this square 
                    var pieceViewModel = selectedSquare.PieceViewModel;
                    pieceViewModel.ShowAsSelected(selected:false);
                    this.boardViewModel.MoveNoCapture(from: selectedSquare, to: this);
                    this.boardViewModel.ClearSelection();
                }
            }
            // ELSE: Click on empty square when there is no selection: Do nothing  
        }
        else
        {
            // Click on occupied square : Must be same as clicking the piece
            this.PieceViewModel.OnClicked();
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
        this.canBeClicked = false; 
        this.pieceViewModel?.DisableClicks();
    }
}
