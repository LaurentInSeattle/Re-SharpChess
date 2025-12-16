namespace MinimalChess; 

//    A  B  C  D  E  F  G  H        BLACK
// 8  56 57 58 59 60 61 62 63  8
// 7  48 49 50 51 52 53 54 55  7
// 6  40 41 42 43 44 45 46 47  6
// 5  32 33 34 35 36 37 38 39  5
// 4  24 25 26 27 28 29 30 31  4
// 3  16 17 18 19 20 21 22 23  3
// 2  08 09 10 11 12 13 14 15  2
// 1  00 01 02 03 04 05 06 07  1
//    A  B  C  D  E  F  G  H        WHITE

public sealed class Board
{
    private static readonly int BlackKingSquare = Notation.ToSquare("e8");
    private static readonly int WhiteKingSquare = Notation.ToSquare("e1");
    private static readonly int BlackQueensideRookSquare = Notation.ToSquare("a8");
    private static readonly int BlackKingsideRookSquare = Notation.ToSquare("h8");
    private static readonly int WhiteQueensideRookSquare = Notation.ToSquare("a1");
    private static readonly int WhiteKingsideRookSquare = Notation.ToSquare("h1");

    public const string STARTING_POS_FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

    [Flags]
    public enum CastlingRights
    {
        None = 0,
        WhiteKingside = 1,
        WhiteQueenside = 2,
        BlackKingside = 4,
        BlackQueenside = 8,
        All = 15
    }

    private readonly Piece[] state = new Piece[64];

    private CastlingRights castlingRights = CastlingRights.All;

    private Color sideToMove = Color.White;

    private int enPassantSquare = -1;

    private ulong zobristHash = 0;

    private Evaluation.Eval eval;

    public int Score => eval.Score;

    public ulong ZobristHash => zobristHash;

    public Color SideToMove
    {
        get => sideToMove;

        private set 
        {
            zobristHash ^= Zobrist.SideToMove(sideToMove);
            sideToMove = value;
            zobristHash ^= Zobrist.SideToMove(sideToMove);
        }
    }

    public Board() { }

    public Board(string fen) => this.SetupPosition(fen);

    public Board(Board board) => this.DeepCopy(board);

    public Board(Board board, Move move)
    {
        this.DeepCopy(board);
        this.Play(move);
    }

    public void DeepCopy(Board board)
    {
        board.state.AsSpan().CopyTo(state.AsSpan());
        sideToMove = board.sideToMove;
        enPassantSquare = board.enPassantSquare;
        castlingRights = board.castlingRights;
        zobristHash = board.zobristHash;
        eval = board.eval;
    }

    public Piece this[int square]
    {
        get => state[square];
        private set
        {
            eval.Update(state[square], value, square);
            zobristHash ^= Zobrist.PieceSquare(state[square], square);
            state[square] = value;
            zobristHash ^= Zobrist.PieceSquare(state[square], square);
            Debug.Assert(this.Score == new Evaluation.Eval(this).Score);
        }
    }

    // Rank - the eight horizontal rows of the chess board are called ranks.
    // File - the eight vertical columns of the chess board are called files.
    public Piece this[int rank, int file] => state[rank * 8 + file];

    public void SetupPosition(string fen)
    {
        //Startpos in FEN looks like this: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
        //https://en.wikipedia.org/wiki/Forsyth%E2%80%93Edwards_Notation
        string[] fields = fen.Split();
        if (fields.Length < 4)
        {
            throw new ArgumentException($"FEN needs at least 4 fields. Has only {fields.Length} fields.");
        }

        // Place pieces on board
        Array.Clear(state, 0, 64);
        string[] fenPosition = fields[0].Split('/');
        int rank = 7;
        foreach (string row in fenPosition)
        {
            int file = 0;
            foreach (char piece in row)
            {
                if (char.IsNumber(piece))
                {
                    int emptySquares = (int)char.GetNumericValue(piece);
                    file += emptySquares;
                }
                else
                {
                    state[rank * 8 + file] = Notation.ToPiece(piece);
                    file++;
                }
            }

            rank--;
        }

        //Set side to move
        this.sideToMove = fields[1].Equals("w", StringComparison.CurrentCultureIgnoreCase) ? Color.White : Color.Black;

        //Set castling rights
        this.SetCastlingRights(CastlingRights.WhiteKingside, fields[2].IndexOf('K') > -1);
        this.SetCastlingRights(CastlingRights.WhiteQueenside, fields[2].IndexOf('Q') > -1);
        this.SetCastlingRights(CastlingRights.BlackKingside, fields[2].IndexOf('k') > -1);
        this.SetCastlingRights(CastlingRights.BlackQueenside, fields[2].IndexOf('q') > -1);

        //Set en-passant square
        this.enPassantSquare = fields[3] == "-" ? -1 : Notation.ToSquare(fields[3]);

        //Init incremental eval
        this.eval = new Evaluation.Eval(this);

        //Initialze Hash
        this.InitZobristHash();
    }

    //** PLAY MOVES ***

    public void PlayNullMove()
    {
        this.SideToMove = Pieces.Flip(sideToMove);
        //Clear en passent
        this.zobristHash ^= Zobrist.EnPassant(enPassantSquare);
        this.enPassantSquare = -1;
    }

    public void Play(Move move)
    {
        Piece movingPiece = this[move.FromSquare];
        if (move.Promotion != Piece.None)
        {
            movingPiece = move.Promotion.OfColor(sideToMove);
        }

        //move the correct piece to the target square
        this[move.ToSquare] = movingPiece;

        //...and clear the square it was previously located
        this[move.FromSquare] = Piece.None;

        if (this.IsEnPassant(movingPiece, move, out int captureIndex))
        {
            //capture the pawn
            this[captureIndex] = Piece.None;
        }

        //handle castling special case
        if (IsCastling(movingPiece, move, out Move rookMove))
        {
            //move the rook to the target square and clear from square
            this[rookMove.ToSquare] = this[rookMove.FromSquare];
            this[rookMove.FromSquare] = Piece.None;
        }

        //update board state
        this.UpdateEnPassant(move);
        this.UpdateCastlingRights(move.FromSquare);
        this.UpdateCastlingRights(move.ToSquare);

        //toggle active color!
        this.SideToMove = Pieces.Flip(sideToMove);
    }

    private void UpdateCastlingRights(int square)
    {
        //any move from or to king or rook squares will effect castling right
        if (square == WhiteKingSquare || square == WhiteQueensideRookSquare)
        {
            this.SetCastlingRights(CastlingRights.WhiteQueenside, false);
        }

        if (square == WhiteKingSquare || square == WhiteKingsideRookSquare)
        {
            this.SetCastlingRights(CastlingRights.WhiteKingside, false);
        }

        if (square == BlackKingSquare || square == BlackQueensideRookSquare)
        {
            this.SetCastlingRights(CastlingRights.BlackQueenside, false);
        }

        if (square == BlackKingSquare || square == BlackKingsideRookSquare)
        {
            this.SetCastlingRights(CastlingRights.BlackKingside, false);
        }
    }

    private void UpdateEnPassant(Move move)
    {
        this.zobristHash ^= Zobrist.EnPassant(enPassantSquare);

        int to = move.ToSquare;
        int from = move.FromSquare;
        Piece movingPiece = state[to];

        //movingPiece needs to be either a BlackPawn...
        if (movingPiece == Piece.BlackPawn && Rank(to) == Rank(from) - 2)
        {
            enPassantSquare = Down(from);
        }
        else if (movingPiece == Piece.WhitePawn && Rank(to) == Rank(from) + 2)
        {
            enPassantSquare = Up(from);
        }
        else
        {
            enPassantSquare = -1;
        }

        zobristHash ^= Zobrist.EnPassant(enPassantSquare);
    }

    private bool IsEnPassant(Piece movingPiece, Move move, out int captureIndex)
    {
        if (movingPiece == Piece.BlackPawn && move.ToSquare == enPassantSquare)
        {
            captureIndex = Up(enPassantSquare);
            return true;
        }
        else if (movingPiece == Piece.WhitePawn && move.ToSquare == enPassantSquare)
        {
            captureIndex = Down(enPassantSquare);
            return true;
        }

        // not en passant
        captureIndex = -1;
        return false;
    }

    private static bool IsCastling(Piece moving, Move move, out Move rookMove)
    {
        if (moving == Piece.BlackKing && move == Move.BlackCastlingLong)
        {
            rookMove = Move.BlackCastlingLongRook;
            return true;
        }
        if (moving == Piece.BlackKing && move == Move.BlackCastlingShort)
        {
            rookMove = Move.BlackCastlingShortRook;
            return true;
        }
        if (moving == Piece.WhiteKing && move == Move.WhiteCastlingLong)
        {
            rookMove = Move.WhiteCastlingLongRook;
            return true;
        }
        if (moving == Piece.WhiteKing && move == Move.WhiteCastlingShort)
        {
            rookMove = Move.WhiteCastlingShortRook;
            return true;
        }

        //not castling
        rookMove = default;
        return false;
    }

    //** MOVE GENERATION ***

    public bool IsPlayable(Move move)
    {
        bool found = false;
        this.CollectMoves(move.FromSquare, m => found |= (m == move));
        return found;
    }

    public void CollectMoves(Action<Move> moveHandler)
    {
        for (int square = 0; square < 64; square++)
        {
            this.CollectMoves(square, moveHandler);
        }
    }

    public void CollectQuiets(Action<Move> moveHandler)
    {
        for (int square = 0; square < 64; square++)
        {
            this.CollectQuiets(square, moveHandler);
        }
    }

    public void CollectCaptures(Action<Move> moveHandler)
    {
        for (int square = 0; square < 64; square++)
        {
            this.CollectCaptures(square, moveHandler);
        }
    }

    public void CollectMoves(int square, Action<Move> moveHandler)
    {
        this.CollectQuiets(square, moveHandler);
        this.CollectCaptures(square, moveHandler);
    }

    public void CollectQuiets(int square, Action<Move> moveHandler)
    {
        if (this.IsActivePiece(state[square]))
        {
            this.AddQuiets(square, moveHandler);
        }
    }

    public void CollectCaptures(int square, Action<Move> moveHandler)
    {
        if (this.IsActivePiece(state[square]))
        {
            this.AddCaptures(square, moveHandler);
        }
    }

    private void AddQuiets(int square, Action<Move> moveHandler)
    {
        switch (state[square])
        {
            case Piece.BlackPawn:
                this.AddBlackPawnMoves(moveHandler, square);
                break;
            case Piece.WhitePawn:
                this.AddWhitePawnMoves(moveHandler, square);
                break;
            case Piece.BlackKing:
                this.AddBlackCastlingMoves(moveHandler);
                this.AddMoves(moveHandler, square, Attacks.King);
                break;
            case Piece.WhiteKing:
                this.AddWhiteCastlingMoves(moveHandler);
                this.AddMoves(moveHandler, square, Attacks.King);
                break;
            case Piece.BlackKnight:
            case Piece.WhiteKnight:
                this.AddMoves(moveHandler, square, Attacks.Knight);
                break;
            case Piece.BlackRook:
            case Piece.WhiteRook:
                this.AddMoves(moveHandler, square, Attacks.Rook);
                break;
            case Piece.BlackBishop:
            case Piece.WhiteBishop:
                this.AddMoves(moveHandler, square, Attacks.Bishop);
                break;
            case Piece.BlackQueen:
            case Piece.WhiteQueen:
                this.AddMoves(moveHandler, square, Attacks.Queen);
                break;
        }
    }

    private void AddCaptures(int square, Action<Move> moveHandler)
    {
        switch (state[square])
        {
            case Piece.BlackPawn:
                this.AddBlackPawnAttacks(moveHandler, square);
                break;
            case Piece.WhitePawn:
                this.AddWhitePawnAttacks(moveHandler, square);
                break;
            case Piece.BlackKing:
            case Piece.WhiteKing:
                this.AddCaptures(moveHandler, square, Attacks.King);
                break;
            case Piece.BlackKnight:
            case Piece.WhiteKnight:
                this.AddCaptures(moveHandler, square, Attacks.Knight);
                break;
            case Piece.BlackRook:
            case Piece.WhiteRook:
                this.AddCaptures(moveHandler, square, Attacks.Rook);
                break;
            case Piece.BlackBishop:
            case Piece.WhiteBishop:
                this.AddCaptures(moveHandler, square, Attacks.Bishop);
                break;
            case Piece.BlackQueen:
            case Piece.WhiteQueen:
                this.AddCaptures(moveHandler, square, Attacks.Queen);
                break;
        }
    }

    private static void AddMove(Action<Move> moveHandler, int from, int to) 
        => moveHandler(new Move(from, to));

    private static void AddPromotion(Action<Move> moveHandler, int from, int to, Piece promotion) 
        => moveHandler(new Move(from, to, promotion));

    //** CHECK TEST ***

    public bool IsChecked(Color color)
    {
        Piece king = Piece.King.OfColor(color);
        for (int square = 0; square < 64; square++)
        {
            if (state[square] == king)
            {
                return this.IsSquareAttackedBy(square, Pieces.Flip(color));
            }
        }

        throw new Exception($"No {color} King found!");
    }

    public bool IsSquareAttackedBy(int square, Color color)
    {
        //1. Pawns? (if attacker is white, pawns move up and the square is attacked from below. squares below == Attacks.BlackPawn)
        byte[][] pawnAttacks = color == Color.White ? Attacks.BlackPawn : Attacks.WhitePawn;
        foreach (int target in pawnAttacks[square])
        {
            if (state[target] == Piece.Pawn.OfColor(color))
            {
                return true;
            }
        }

        //2. Knight
        foreach (int target in Attacks.Knight[square])
        {
            if (state[target] == Piece.Knight.OfColor(color))
            {
                return true;
            }
        }

        //3. Queen or Bishops on diagonals lines
        for (int dir = 0; dir < 4; dir++)
        {
            foreach (int target in Attacks.Bishop[square][dir])
            {
                if (state[target] == Piece.Bishop.OfColor(color) || state[target] == Piece.Queen.OfColor(color))
                {
                    return true;
                }

                if (state[target] != Piece.None)
                {
                    break;
                }
            }
        }

        //4. Queen or Rook on straight lines
        for (int dir = 0; dir < 4; dir++)
        {
            foreach (int target in Attacks.Rook[square][dir])
            {
                if (state[target] == Piece.Rook.OfColor(color) || state[target] == Piece.Queen.OfColor(color))
                {
                    return true;
                }

                if (state[target] != Piece.None)
                {
                    break;
                }
            }
        }

        //5. King
        foreach (int target in Attacks.King[square])
        {
            if (state[target] == Piece.King.OfColor(color))
            {
                return true;
            }
        }

        return false; //not threatened by anyone!
    }

    //** CAPTURES **

    private void AddCaptures(Action<Move> moveHandler, int square, byte[][] targets)
    {
        foreach (int target in targets[square])
        {
            if (this.IsOpponentPiece(state[target]))
            {
                Board.AddMove(moveHandler, square, target);
            }
        }
    }

    private void AddCaptures(Action<Move> moveHandler, int square, byte[][][] targets)
    {
        foreach (byte[] axis in targets[square])
        {
            foreach (int target in axis)
            {
                if (state[target] != Piece.None)
                {
                    if (this.IsOpponentPiece(state[target]))
                    {
                        Board.AddMove(moveHandler, square, target);
                    }

                    break;
                }
            }
        }
    }

    //** MOVES **

    private void AddMoves(Action<Move> moveHandler, int square, byte[][] targets)
    {
        foreach (int target in targets[square])
        {
            if (state[target] == Piece.None)
            {
                Board.AddMove(moveHandler, square, target);
            }
        }
    }

    private void AddMoves(Action<Move> moveHandler, int square, byte[][][] targets)
    {
        foreach (byte[] axis in targets[square])
        {
            foreach (int target in axis)
            {
                if (state[target] == Piece.None)
                {
                    Board.AddMove(moveHandler, square, target);
                }
                else
                {
                    break;
                }
            }
        }
    }

    //** KING MOVES **

    private void AddWhiteCastlingMoves(Action<Move> moveHandler)
    {
        //Castling is only possible if it's associated CastlingRight flag is set? it get's cleared when either the king or the matching rook move and provide a cheap early out
        if (this.HasCastlingRight(CastlingRights.WhiteQueenside) &&
            this.CanCastle(WhiteKingSquare, WhiteQueensideRookSquare, Color.White))
        {
            moveHandler(Move.WhiteCastlingLong);
        }

        if (this.HasCastlingRight(CastlingRights.WhiteKingside) &&
            this.CanCastle(WhiteKingSquare, WhiteKingsideRookSquare, Color.White))
        {
            moveHandler(Move.WhiteCastlingShort);
        }
    }


    private void AddBlackCastlingMoves(Action<Move> moveHandler)
    {
        if (this.HasCastlingRight(CastlingRights.BlackQueenside) &&
            this.CanCastle(BlackKingSquare, BlackQueensideRookSquare, Color.Black))
        {
            moveHandler(Move.BlackCastlingLong);
        }

        if (this.HasCastlingRight(CastlingRights.BlackKingside) &&
            this.CanCastle(BlackKingSquare, BlackKingsideRookSquare, Color.Black))
        {
            moveHandler(Move.BlackCastlingShort);
        }
    }

    private bool CanCastle(int kingSquare, int rookSquare, Color color)
    {
        Debug.Assert(state[kingSquare] == Piece.King.OfColor(color), "CanCastle shouldn't be called if castling right has been lost!");
        Debug.Assert(state[rookSquare] == Piece.Rook.OfColor(color), "CanCastle shouldn't be called if castling right has been lost!");

        Color enemyColor = Pieces.Flip(color);
        int gap = Math.Abs(rookSquare - kingSquare) - 1;
        int dir = Math.Sign(rookSquare - kingSquare);

        // the squares *between* the king and the rook need to be unoccupied
        for (int i = 1; i <= gap; i++)
        {
            if (state[kingSquare + i * dir] != Piece.None)
            {
                return false;
            }
        }

        // the king must not start, end or pass through a square that is attacked by an enemy piece, 
        // but the rook and the square next to the rook on queenside may be attacked
        for (int i = 0; i < 3; i++)
        {
            if (this.IsSquareAttackedBy(kingSquare + i * dir, enemyColor))
            {
                return false;
            }
        }

        return true;
    }

    //** PAWN MOVES ***

    private void AddWhitePawnMoves(Action<Move> moveHandler, int square)
    {
        //if the square above isn't free there are no legal moves
        if (state[Up(square)] != Piece.None)
        {
            return;
        }

        Board.AddWhitePawnMove(moveHandler, square, Up(square));

        //START POS? => consider double move
        if (Rank(square) == 1 && state[Up(square, 2)] == Piece.None)
        {
            AddMove(moveHandler, square, Up(square, 2));
        }
    }

    private void AddBlackPawnMoves(Action<Move> moveHandler, int square)
    {
        //if the square below isn't free there are no legal moves
        if (state[Down(square)] != Piece.None)
        {
            return;
        }

        Board.AddBlackPawnMove(moveHandler, square, Down(square));

        //START POS? => consider double move
        if (Rank(square) == 6 && state[Down(square, 2)] == Piece.None)
        {
            AddMove(moveHandler, square, Down(square, 2));
        }
    }


    private void AddWhitePawnAttacks(Action<Move> moveHandler, int square)
    {
        foreach (int target in Attacks.WhitePawn[square])
        {
            if (state[target].IsBlack() || target == enPassantSquare)
            {
                AddWhitePawnMove(moveHandler, square, target);
            }
        }
    }

    private void AddBlackPawnAttacks(Action<Move> moveHandler, int square)
    {
        foreach (int target in Attacks.BlackPawn[square])
        {
            if (state[target].IsWhite() || target == enPassantSquare)
            {
                AddBlackPawnMove(moveHandler, square, target);
            }
        }
    }

    private static void AddBlackPawnMove(Action<Move> moveHandler, int from, int to)
    {
        if (Rank(to) == 0) //Promotion?
        {
            AddPromotion(moveHandler, from, to, Piece.BlackQueen);
            AddPromotion(moveHandler, from, to, Piece.BlackRook);
            AddPromotion(moveHandler, from, to, Piece.BlackBishop);
            AddPromotion(moveHandler, from, to, Piece.BlackKnight);
        }
        else
        {
            AddMove(moveHandler, from, to);
        }
    }

    private static void AddWhitePawnMove(Action<Move> moveHandler, int from, int to)
    {
        if (Rank(to) == 7) //Promotion?
        {
            AddPromotion(moveHandler, from, to, Piece.WhiteQueen);
            AddPromotion(moveHandler, from, to, Piece.WhiteRook);
            AddPromotion(moveHandler, from, to, Piece.WhiteBishop);
            AddPromotion(moveHandler, from, to, Piece.WhiteKnight);
        }
        else
        {
            AddMove(moveHandler, from, to);
        }
    }

    //** Utility ***

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Rank(int square) => square / 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Up(int square, int steps = 1) => square + steps * 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Down(int square, int steps = 1) => square - steps * 8;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsActivePiece(Piece piece) => piece.Color() == sideToMove;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsOpponentPiece(Piece piece) => piece.Color() == Pieces.Flip(sideToMove);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool HasCastlingRight(CastlingRights flag) => (castlingRights & flag) == flag;

    private void SetCastlingRights(CastlingRights flag, bool state)
    {
        zobristHash ^= Zobrist.Castling(castlingRights);

        if (state)
        {
            castlingRights |= flag;
        }
        else
        {
            castlingRights &= ~flag;
        }

        zobristHash ^= Zobrist.Castling(castlingRights);
    }

    private void InitZobristHash()
    {
        //Side to move
        zobristHash = Zobrist.SideToMove(sideToMove);
        //Pieces
        for (int square = 0; square < 64; square++)
        {
            zobristHash ^= Zobrist.PieceSquare(state[square], square);
        }

        //En passant
        zobristHash ^= Zobrist.Castling(castlingRights);

        //Castling
        zobristHash ^= Zobrist.EnPassant(enPassantSquare);
    }
}
