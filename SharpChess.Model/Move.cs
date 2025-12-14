namespace SharpChess.Model;

/// <summary> Represents a chess move. </summary>
public sealed class Move : IComparable
{
    public readonly Game Game;
    public readonly Board Board;

    /// <summary> Initializes a new instance of the <see cref="Move"/> class. </summary>
    /// <param name="turnNo"> The turn number. </param>
    /// <param name="lastMoveTurnNo"> The last move turn number. </param>
    /// <param name="moveName"> The move name. </param>
    /// <param name="piece"> The piece moving.</param>
    /// <param name="from">The square the piece is moving from.</param>
    /// <param name="to">The square the piece is moving to.</param>
    /// <param name="pieceCaptured"> The piece being captured, or null if no capture.</param>
    /// <param name="pieceCapturedOrdinal"> Ordinal position of the piece being captured, only valid if capture.</param>
    /// <param name="score"> The positional score. </param>
    public Move(
        int turnNo, int lastMoveTurnNo, MoveNames moveName, 
        Piece? piece, 
        Square from, Square to, 
        Piece? pieceCaptured, int pieceCapturedOrdinal, 
        int score)
    {
        if ( piece is not null)
        {
            // Sillyness For Unit test checking sorting  
            this.Game = piece.Game;
            this.Board = piece.Game.Board;
        }

        this.EnemyStatus = Player.PlayerStatusNames.Normal;
        this.TurnNo = turnNo;
        this.LastMoveTurnNo = lastMoveTurnNo;
        this.Name = moveName;
        this.Piece = piece;
        this.From = from;
        this.To = to;
        this.PieceCaptured = pieceCaptured;
        this.PieceCapturedOrdinal = pieceCapturedOrdinal;
        this.Score = score;
        this.DebugComment = string.Empty;
        this.Moves = []; 

        if (moveName != MoveNames.NullMove && pieceCaptured == null && piece != null && piece.Name != Piece.PieceNames.Pawn)
        {
            this.FiftyMoveDrawCounter = Game.MoveHistory.Count > 0 ? Game.MoveHistory.Last.FiftyMoveDrawCounter + 1 : (Game.FiftyMoveDrawBase / 2) + 1;
        }
    }

    /// <summary> Move type names.</summary>
    public enum MoveNames
    {
        Standard, 
        CastleQueenSide, 
        CastleKingSide, 
        PawnPromotionQueen, 
        PawnPromotionRook, 
        PawnPromotionKnight, 
        PawnPromotionBishop, 
        EnPassent, 
        NullMove
    }

    /// <summary> Gets or sets Alpha.</summary>
    public int Alpha { get; set; }

    /// <summary> Gets or sets Beta. </summary>
    public int Beta { get; set; }

    /// <summary> Gets text for the move useful in debugging. </summary>
    public string DebugText
        => (this.Piece != null ? this.Piece.Player.Colour.ToString() + " " 
                + this.Piece.Name.ToString() : string.Empty) + " " 
                + this.From.Name 
                + (this.PieceCaptured == null ? "-" : "x") + this.To.Name + " " 
                + (this.PieceCaptured == null ? string.Empty : this.PieceCaptured.Name.ToString()) + " " // + this.Name
                + " A: " + this.Alpha 
                + " B: " + this.Beta 
                + " Score: " + this.Score 
                + " " + this.DebugComment;  // + " h: " + this.m_HashEntries.ToString() + " c:" + this.m_HashCaptures.ToString();

    /// <summary> Gets or sets a comment string containing useful debug info. </summary>
    public string DebugComment { get; set; }

    /// <summary> Gets a texual description of the move. </summary>
    public string Description
    {
        get
        {
            StringBuilder strbMove = new StringBuilder();
            switch (this.Name)
            {
                case MoveNames.CastleKingSide:
                    strbMove.Append("O-O");
                    break;

                case MoveNames.CastleQueenSide:
                    strbMove.Append("O-O-O");
                    break;

                default:
                    if ((this.Piece.Name != Piece.PieceNames.Pawn) && !this.Piece.HasBeenPromoted)
                    {
                        strbMove.Append(this.Piece.Abbreviation);
                    }

                    strbMove.Append(this.From.Name);
                    if (this.PieceCaptured != null)
                    {
                        strbMove.Append("x");
                        if (this.PieceCaptured.Name != Piece.PieceNames.Pawn)
                        {
                            strbMove.Append(this.PieceCaptured.Abbreviation);
                        }
                    }
                    else
                    {
                        strbMove.Append("-");
                    }

                    strbMove.Append(this.To.Name);
                    break;
            }

            if (this.Piece.HasBeenPromoted)
            {
                strbMove.Append(":");
                strbMove.Append(this.Piece.Abbreviation);
            }

            switch (this.EnemyStatus)
            {
                case Player.PlayerStatusNames.InCheckMate:
                    strbMove.Append((this.Piece.Player.Colour == Player.PlayerColourNames.White) ? "# 1-0" : "# 0-1");
                    break;

                case Player.PlayerStatusNames.InStalemate:
                    strbMove.Append(" 1/2-1/2");
                    break;

                case Player.PlayerStatusNames.InCheck:
                    strbMove.Append("+");
                    break;
            }

            if (this.IsThreeMoveRepetition || this.IsFiftyMoveDraw)
            {
                strbMove.Append(" 1/2-1/2");
            }

            return strbMove.ToString();
        }
    }

    /// <summary> Gets or sets status of the enemy e.g. In check, stalemate, checkmate etc. </summary>
    public Player.PlayerStatusNames EnemyStatus { get; set; }

    /// <summary> Gets a counter indicating closeness to a fifty-move-draw condition.</summary>
    public int FiftyMoveDrawCounter { get; private set; }

    /// <summary> Gets the move From square. </summary>
    public Square From { get; private set; }

    /// <summary> Gets or sets the board position HashCodeA. </summary>
    public ulong HashCodeA { get; set; }

    /// <summary> Gets or sets the board position HashCodeB.</summary>
    public ulong HashCodeB { get; set; }

    /// <summary>Gets or sets a value indicating whether the enemy is in check.</summary>
    public bool IsEnemyInCheck { get; set; }

    /// <summary> Gets a value indicating whether a fifty-move-draw condition has been reached. </summary>
    public bool IsFiftyMoveDraw => this.FiftyMoveDrawCounter >= 100;

    /// <summary> Gets or sets a value indicating whether the player-to-play is in check.</summary>
    public bool IsInCheck { get; set; }

    /// <summary> Gets or sets a value indicating whether three-move-repetition applied to this move. </summary>
    public bool IsThreeMoveRepetition { get; set; }

    /// <summary> Gets last move turn-number. </summary>
    public int LastMoveTurnNo { get; private set; }

    /// <summary> Gets the move number. </summary>
    public int MoveNo =>  (this.TurnNo / 2) + 1;

    /// <summary> Gets or sets Moves. </summary>
    public Moves Moves { get; set; }

    /// <summary> Gets the move name. </summary>
    public MoveNames Name { get; private set; }

    /// <summary> Gets the Piece being moved. </summary>
    public Piece Piece { get; private set; }

    /// <summary> Gets or sets the score relating to this move. Ususally used for assigning a move-ordering weighting. </summary>
    public int Score { get; set; }

    /// <summary> Gets or sets TimeStamp. </summary>
    public TimeSpan TimeStamp { get; set; }

    /// <summary> Gets the move To square.</summary>
    public Square To { get; private set; }

    /// <summary> Gets the turn number. </summary>
    public int TurnNo { get; private set; }

    /// <summary> Gets the piece being captured, maybe null. </summary>
    public Piece? PieceCaptured { get; private set; }

    /// <summary> Gets the ordinal of the piece being captured. </summary>
    public int PieceCapturedOrdinal { get; private set; }

    /// <summary> Gets the move name from the provided text. </summary>
    /// <param name="moveName"> The move name text. </param>
    /// <returns> Returns the Move Name enum value. </returns>
    public static MoveNames MoveNameFromString(string moveName)
    {
        if (moveName == MoveNames.Standard.ToString())
        {
            return MoveNames.Standard;
        }

        if (moveName == MoveNames.CastleKingSide.ToString())
        {
            return MoveNames.CastleKingSide;
        }

        if (moveName == MoveNames.CastleQueenSide.ToString())
        {
            return MoveNames.CastleQueenSide;
        }

        if (moveName == MoveNames.EnPassent.ToString())
        {
            return MoveNames.EnPassent;
        }

        if (moveName == "PawnPromotion")
        {
            return MoveNames.PawnPromotionQueen;
        }

        if (moveName == MoveNames.PawnPromotionQueen.ToString())
        {
            return MoveNames.PawnPromotionQueen;
        }

        if (moveName == MoveNames.PawnPromotionRook.ToString())
        {
            return MoveNames.PawnPromotionRook;
        }

        if (moveName == MoveNames.PawnPromotionBishop.ToString())
        {
            return MoveNames.PawnPromotionBishop;
        }

        if (moveName == MoveNames.PawnPromotionKnight.ToString())
        {
            return MoveNames.PawnPromotionKnight;
        }

        return 0;
    }

    /// <summary> Determine where two moves are identical moves. </summary>
    /// <param name="moveA"> Move A. </param>
    /// <param name="moveB"> Move B. </param>
    /// <returns> True if moves match. </returns>
    public static bool MovesMatch(Move moveA, Move moveB)
    {
        return moveA != null 
            && moveB != null 
            && moveA.Piece == moveB.Piece 
            && moveA.From == moveB.From 
            && moveA.To == moveB.To 
            && moveA.Name == moveB.Name 
            && (
                (moveA.PieceCaptured == null && moveB.PieceCaptured == null)
                || (moveA.PieceCaptured != null && moveB.PieceCaptured != null && moveA.PieceCaptured == moveB.PieceCaptured));
    }

    /// <summary> Undo the specified move. </summary>
    /// <param name="move"> The move to undo. </param>
    public void Undo(Move move)
    {
        Board.HashCodeA ^= move.To.Piece.HashCodeA; // un_XOR the piece from where it was previously moved to
        Board.HashCodeB ^= move.To.Piece.HashCodeB; // un_XOR the piece from where it was previously moved to
        if (move.Piece.Name == Piece.PieceNames.Pawn)
        {
            Board.PawnHashCodeA ^= move.To.Piece.HashCodeA;
            Board.PawnHashCodeB ^= move.To.Piece.HashCodeB;
        }

        move.Piece.Square = move.From; // Set piece board location
        move.From.Piece = move.Piece; // Set piece on board
        move.Piece.LastMoveTurnNo = move.LastMoveTurnNo;
        move.Piece.NoOfMoves--;


        Piece? pieceCaptured = move.PieceCaptured; 
        if (move.Name != MoveNames.EnPassent)
        {
            // TODO: Can be null 
            move.To.Piece = pieceCaptured; // Return piece taken
        }
        else
        {
            // TODO: Can be null 
            move.To.Piece = null; // Blank square where this pawn was
            Square? square = Board.GetSquare(move.To.Ordinal - move.Piece.Player.PawnForwardOffset);
            if (square is not null)
            {
                // TODO: Can be null 
                square.Piece = pieceCaptured; // Return En Passent pawn taken
            } 
        }

        if (pieceCaptured is not null)
        {
            pieceCaptured.Uncapture(move.PieceCapturedOrdinal);
            Board.HashCodeA ^= pieceCaptured.HashCodeA; // XOR back into play the piece that was taken
            Board.HashCodeB ^= pieceCaptured.HashCodeB; // XOR back into play the piece that was taken
            if (pieceCaptured.Name == Piece.PieceNames.Pawn)
            {
                Board.PawnHashCodeA ^= pieceCaptured.HashCodeA;
                Board.PawnHashCodeB ^= pieceCaptured.HashCodeB;
            }
        } 
        
        Piece pieceRook;
        switch (move.Name)
        {
            case MoveNames.CastleKingSide:
                pieceRook = move.Piece.Player.Colour == Player.PlayerColourNames.White ? Board.GetPiece(5, 0) : Board.GetPiece(5, 7);
                Board.HashCodeA ^= pieceRook.HashCodeA;
                Board.HashCodeB ^= pieceRook.HashCodeB;
                pieceRook.Square = Board.GetSquare(7, move.Piece.Square.Rank);
                pieceRook.LastMoveTurnNo = move.LastMoveTurnNo;
                pieceRook.NoOfMoves--;
                Board.GetSquare(7, move.Piece.Square.Rank).Piece = pieceRook;
                Board.GetSquare(5, move.Piece.Square.Rank).Piece = null;
                move.Piece.Player.HasCastled = false;
                Board.HashCodeA ^= pieceRook.HashCodeA;
                Board.HashCodeB ^= pieceRook.HashCodeB;
                break;

            case MoveNames.CastleQueenSide:
                pieceRook = move.Piece.Player.Colour == Player.PlayerColourNames.White ? Board.GetPiece(3, 0) : Board.GetPiece(3, 7);
                Board.HashCodeA ^= pieceRook.HashCodeA;
                Board.HashCodeB ^= pieceRook.HashCodeB;
                pieceRook.Square = Board.GetSquare(0, move.Piece.Square.Rank);
                pieceRook.LastMoveTurnNo = move.LastMoveTurnNo;
                pieceRook.NoOfMoves--;
                Board.GetSquare(0, move.Piece.Square.Rank).Piece = pieceRook;
                Board.GetSquare(3, move.Piece.Square.Rank).Piece = null;
                move.Piece.Player.HasCastled = false;
                Board.HashCodeA ^= pieceRook.HashCodeA;
                Board.HashCodeB ^= pieceRook.HashCodeB;
                break;

            case MoveNames.PawnPromotionQueen:
            case MoveNames.PawnPromotionRook:
            case MoveNames.PawnPromotionBishop:
            case MoveNames.PawnPromotionKnight:
                move.Piece.Demote();
                break;
        }

        Board.HashCodeA ^= move.From.Piece.HashCodeA; // XOR the piece back into the square it moved back to
        Board.HashCodeB ^= move.From.Piece.HashCodeB; // XOR the piece back into the square it moved back to
        if (move.From.Piece.Name == Piece.PieceNames.Pawn)
        {
            Board.PawnHashCodeA ^= move.From.Piece.HashCodeA;
            Board.PawnHashCodeB ^= move.From.Piece.HashCodeB;
        }

        if (move.IsThreeMoveRepetition)
        {
            Board.HashCodeA ^= 31;
            Board.HashCodeB ^= 29;
        }

        Game.TurnNo--;
        Game.MoveHistory.RemoveLast();
    }

    /// <summary> Compare the score of this move, and the specified move.</summary>
    /// <param name="move"> Move to compare. </param>
    /// <returns> 1 if specified move score is less, -1 if more, otherwise 0 </returns>
    public int CompareTo(object? move)
    {
        if (move is null)
        {
            return 1;
        } 

        if (this.Score < ((Move)move).Score)
        {
            return 1;
        }

        if (this.Score > ((Move)move).Score)
        {
            return -1;
        }

        return 0;
    }

    /// <summary> Is the move a promotion of a pawn  </summary>
    /// <returns> true if promotion otherwise false </returns>
    /// <remarks> Keep the order of the enumeration <see cref="MoveNames"/>.PawnPromotionQueen before PawnPromotionBishop </remarks>
    public bool IsPromotion() 
        => (this.Name >= MoveNames.PawnPromotionQueen) && (this.Name <= MoveNames.PawnPromotionBishop);    
}