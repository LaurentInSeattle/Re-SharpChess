namespace Lyt.Chess.Workflow.Play;

internal class BoardViewModel(ChessModel chessModel) : ViewModel<BoardView>
{
    private readonly ChessModel chessModel = chessModel;
    private readonly SquareViewModel[] squareViewModels = new SquareViewModel[64];
    private readonly Dictionary<SquareViewModel, List<Move>> legalMoves = new(32);

    private SquareViewModel? selectedSquare; // Can be null 

    internal bool HasSelectedSquare => this.selectedSquare is not null;

    internal SquareViewModel SelectedSquare
        => this.selectedSquare is not null ?
            this.selectedSquare :
            throw new Exception("Should have checked HasSelectedSquare property");

    internal SquareViewModel SquareAt(int rank, int file) => this.squareViewModels[rank * 8 + file];

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
        if (squareViewModel.IsEmpty)
        {
            return;
        }

        PieceViewModel pieceViewModel = squareViewModel.PieceViewModel;
        this.View.RemovePieceView(pieceViewModel);
    }

    internal void UpdateBoard(Move move)
    {
        Debug.WriteLine("UpdateBoard: " + move.ToString());

        SquareViewModel fromSquareViewModel = this.SquareAt(move.FromSquare);
        if (fromSquareViewModel.IsEmpty)
        {
            if (Debugger.IsAttached) { Debugger.Break(); }
            return;
        }

        SquareViewModel toSquareViewModel = this.SquareAt(move.ToSquare);
        if (!toSquareViewModel.IsEmpty)
        {
            // Capture 
            PieceViewModel capturedPieceViewModel = toSquareViewModel.RemovePiece();
            Debug.WriteLine("UpdateBoard: Capture: " + capturedPieceViewModel.Piece.ToString());
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

        this.ClearLegalMoves(); 
    }

    internal void ClearLegalMoves()
    {
        foreach (var square in this.squareViewModels)
        {
            square.ShowAsLegal(legal: false);
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

    internal void UpdateCheckedStatus(PlayerColor playerColor)
    {
        // Remove In Check flag on all squares
        foreach (var square in this.squareViewModels)
        {
            square.ShowAsInCheck(inCheck: false);
        }

        if (playerColor != PlayerColor.None)
        {
            for (int index = 0; index < 64; index++)
            {
                var square = this.squareViewModels[index];
                if (square.IsEmpty)
                {
                    continue;
                }

                var piece = square.PieceViewModel.Piece;
                if ((piece.Color() == playerColor) &&
                    (piece == Piece.WhiteKing || piece == Piece.BlackKing))
                {
                    // In check !
                    square.ShowAsInCheck();
                }
            }
        } 
    }

    internal void SaveLegalMoves(LegalMoves updatedLegalMoves)
    {
        this.legalMoves.Clear();
        foreach (var move in updatedLegalMoves)
        {
            var vm = this.SquareAt(move.FromSquare);
            if (this.legalMoves.TryGetValue(vm, out List<Move>? newLegalMoves) && newLegalMoves is not null)
            {
                newLegalMoves.Add(move);
            }
            else
            {
                this.legalMoves.Add(vm, [move]);
            }
        }
    }

    private void ShowLegalMoves(SquareViewModel squareViewModel)
    {
        bool hasLegalMoves =
            this.legalMoves.TryGetValue(squareViewModel, out List<Move>? squareLegalMoves) &&
            squareLegalMoves is not null &&
            squareLegalMoves.Count > 0;
        if (!hasLegalMoves || squareLegalMoves is null)
        {
            return;
        }

        foreach (Move move in squareLegalMoves)
        {
            SquareViewModel legalSquare = this.SquareAt(move.ToSquare);
            legalSquare.ShowAsLegal();
        }
    }

    /// <summary> Returns true is the provided square has any legal move </summary>
    internal bool HasLegalMoves(SquareViewModel squareViewModel)
        => this.legalMoves.TryGetValue(squareViewModel, out List<Move>? squareLegalMoves) &&
           squareLegalMoves is not null &&
           squareLegalMoves.Count > 0;

    /// <summary> Returns true is the provided piece has any legal move </summary>
    internal bool HasLegalMoves(PieceViewModel pieceViewModel)
        => this.HasLegalMoves(pieceViewModel.SquareViewModel);


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