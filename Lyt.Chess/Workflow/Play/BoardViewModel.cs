namespace Lyt.Chess.Workflow.Play;

internal class BoardViewModel : ViewModel<BoardView>
{
    private readonly SquareViewModel[] squareViewModels;
    private SquareViewModel? selectedSquare; 

    public BoardViewModel()
    {
        this.squareViewModels = new SquareViewModel[64];
    }

    internal bool HasSelectedSquare => this.selectedSquare is not null;

    internal SquareViewModel SelectedSquare 
        => this.selectedSquare is not null ? 
            this.selectedSquare : 
            throw new Exception("Should have checked HasSelectedSquare property");

    internal SquareViewModel SquareAt(int rank, int file)=> this.squareViewModels[rank * 8 + file];

    internal void CreateEmpty()
    {
        PieceImageProvider.Inititalize();

        // Initialize square view models
        for (int index = 0; index < 64; index++)
        {
            int rank = index / 8;
            int file = index % 8;
            var squareViewModel = new SquareViewModel(this, rank, file);
            _ = squareViewModel.CreateViewAndBind();
            this.squareViewModels[index] = squareViewModel;
            this.View.AddSquareView(squareViewModel);
        }

        // Initialize checker labels for ranks and files 
        for (int index = 0; index < 8; index++)
        {
            this.View.AddRankFileTextBoxes(index);
        }
    }

    // Initialize piece view models
    internal void Populate(Board board)
    {
        for (int index = 0; index < 64; index++)
        {
            Piece piece = board[index];
            if (piece == Piece.None)
            {
                // empty square
                continue;
            } 

            int rank = index / 8;
            int file = index % 8;

            var pieceViewModel =
                new PieceViewModel(Notation.ToChar(piece), this, this.SquareAt(rank, file));
            _ = pieceViewModel.CreateViewAndBind();
            this.View.AddPieceView(pieceViewModel, rank, file);
        }
    } 

    internal void ClearSelection()
    {
        this.selectedSquare = null;

        // Deselect all other squares and pieces
        foreach (var square in this.squareViewModels)
        {
            square.Select(select: false);
        }
    }

    internal void OnPieceSelected(SquareViewModel squareViewModel)
    {
        this.selectedSquare = squareViewModel;

        // Deselect all other squares and pieces
        foreach (var square in this.squareViewModels)
        {
            if (square.IsEmpty || square == squareViewModel)
            {
                continue;
            }

            square.Select(select: false);
        }

        this.ShowLegalMoves(squareViewModel);
    }

    private void ShowLegalMoves(SquareViewModel squareViewModel)
    {
        // TODO 
    }

    /// <summary> Returns true is the provided piece has any legal move </summary>
    internal bool HasLegalMoves(PieceViewModel pieceViewModel)
    {
        // TODO
        return true;
    }
} 