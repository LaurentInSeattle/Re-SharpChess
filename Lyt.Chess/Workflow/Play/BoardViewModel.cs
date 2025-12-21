namespace Lyt.Chess.Workflow.Play;

using System;

internal class BoardViewModel : ViewModel<BoardView>
{
    private readonly ChessModel chessModel;
    private readonly SquareViewModel[] squareViewModels;

    private SquareViewModel? selectedSquare; // Can be null 

    public BoardViewModel(ChessModel chessModel)
    {
        this.chessModel = chessModel;
        this.squareViewModels = new SquareViewModel[64];
    }

    internal bool HasSelectedSquare => this.selectedSquare is not null;

    internal SquareViewModel SelectedSquare 
        => this.selectedSquare is not null ? 
            this.selectedSquare : 
            throw new Exception("Should have checked HasSelectedSquare property");

    internal SquareViewModel SquareAt(int rank, int file)=> this.squareViewModels[rank * 8 + file];

    internal SquareViewModel SquareAt(byte square) => this.squareViewModels[square];

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
                new PieceViewModel(piece, this, this.SquareAt(rank, file));
            _ = pieceViewModel.CreateViewAndBind();
            this.View.AddPieceView(pieceViewModel, rank, file);
        }
    }

    internal void CapturePiece(byte square)
    {
        SquareViewModel squareViewModel = this.SquareAt(square);
        if ( squareViewModel.IsEmpty )
        {
            return; 
        }

        PieceViewModel pieceViewModel = squareViewModel.PieceViewModel;
        this.View.RemovePieceView(pieceViewModel);
    }

    internal void UpdateBoard(Move move)
    {
        SquareViewModel fromSquareViewModel = this.SquareAt(move.FromSquare);
        if (fromSquareViewModel.IsEmpty)
        {
            if (Debugger.IsAttached) { Debugger.Break(); }
            return;
        }

        SquareViewModel toSquareViewModel = this.SquareAt(move.ToSquare);
        if (!toSquareViewModel.IsEmpty)
        {
            if (Debugger.IsAttached) { Debugger.Break(); }
            return;
        }

        PieceViewModel pieceViewModel = fromSquareViewModel.RemovePiece();
        toSquareViewModel.PlacePiece(pieceViewModel);
        int index = move.ToSquare;
        int rank = index / 8;
        int file = index % 8;
        this.View.MovePieceView(pieceViewModel, rank, file);
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

    internal void MoveWithCapture(SquareViewModel from, SquareViewModel to, PieceViewModel capture)
        => this.MoveCaptureOrNot(from, to, capture);

    internal void MoveNoCapture(SquareViewModel from, SquareViewModel to)
        => this.MoveCaptureOrNot(from, to, null);

    private void MoveCaptureOrNot(SquareViewModel from, SquareViewModel to, PieceViewModel? capture)
    {
        // TODO: Promotion Dialog
        // TODO: Promotion Flag - false for now 
        var move = new Move(from.Index, to.Index);

        if (capture is not null)
        {
            // TODO: Handle captured piece 
        }

        // Fire and forget 
        this.chessModel.Play(move);
    }
}