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

    internal void CreateBoard()
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

        // Initialize piece view models
        Piece[] rank0 =
        [
            Piece.WhiteRook, Piece.WhiteKnight, Piece.WhiteBishop, Piece.WhiteKing,
            Piece.WhiteQueen, Piece.WhiteBishop, Piece.WhiteKnight, Piece.WhiteRook
        ];
        Piece[] rank7 =
        [
            Piece.BlackRook, Piece.BlackKnight, Piece.BlackBishop, Piece.BlackQueen,
            Piece.BlackKing, Piece.BlackBishop, Piece.BlackKnight, Piece.BlackRook
        ];

        // Place pieces on the board
        for (int file = 0; file < rank0.Length; file++)
        {
            // White pieces
            var whitePieceViewModel = 
                new PieceViewModel(Notation.ToChar(rank0[file]), this, this.SquareAt(rank:0, file));
            _ = whitePieceViewModel.CreateViewAndBind();
            this.View.AddPieceView(whitePieceViewModel, rank: 0, file);

            // White pawns
            var whitePawnViewModel = 
                new PieceViewModel(Notation.ToChar(Piece.WhitePawn),this, this.SquareAt(rank: 1, file));
            _ = whitePawnViewModel.CreateViewAndBind();
            this.View.AddPieceView(whitePawnViewModel, rank: 1, file);

            // Black pieces
            var blackPieceViewModel = 
                new PieceViewModel(Notation.ToChar(rank7[file]), this, this.SquareAt(rank: 7, file));
            _ = blackPieceViewModel.CreateViewAndBind();
            this.View.AddPieceView(blackPieceViewModel, rank: 7, file);

            // Black pawns
            var blackPawnViewModel = 
                new PieceViewModel(Notation.ToChar(Piece.BlackPawn), this, this.SquareAt(rank: 6, file));
            _ = blackPawnViewModel.CreateViewAndBind();
            this.View.AddPieceView(blackPawnViewModel, rank: 6, file);
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