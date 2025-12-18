namespace Lyt.Chess.Workflow.Play;

internal class BoardViewModel : ViewModel<BoardView>
{
    public void CreateBoard()
    {
        PieceImageProvider.Inititalize();

        // Initialize square view models
        for (int index = 0; index < 64; index++)
        {
            int rank = index / 8;
            int file = index % 8;
            var squareViewModel = new SquareViewModel(rank, file);
            _ = squareViewModel.CreateViewAndBind();
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
            var whitePieceViewModel = new PieceViewModel(Notation.ToChar(rank0[file]));
            _ = whitePieceViewModel.CreateViewAndBind();
            this.View.AddPieceView(whitePieceViewModel, rank:0, file);
            var whitePawnViewModel = new PieceViewModel(Notation.ToChar(Piece.WhitePawn));
            _ = whitePawnViewModel.CreateViewAndBind();
            this.View.AddPieceView(whitePawnViewModel, rank: 1, file);

            var blackPieceViewModel = new PieceViewModel(Notation.ToChar(rank7[file]));
            _ = blackPieceViewModel.CreateViewAndBind();
            this.View.AddPieceView(blackPieceViewModel, rank:7, file);
            var blackPawnViewModel = new PieceViewModel(Notation.ToChar(Piece.BlackPawn));
            _ = blackPawnViewModel.CreateViewAndBind();
            this.View.AddPieceView(blackPawnViewModel, rank: 6, file);
        }
    }
}