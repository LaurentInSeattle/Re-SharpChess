namespace SharpChess.Model; 

/// <summary> The piece knight.</summary>
public class PieceKnight : IPieceTop
{
    /// <summary> Simple positional piece-square score values. </summary>
    private static readonly int[] SquareValues =
    { 
        1, 1,  1,  1,  1,  1, 1, 1,    0, 0, 0, 0, 0, 0, 0, 0, 
        1, 7,  7,  7,  7,  7, 7, 1,    0, 0, 0, 0, 0, 0, 0, 0, 
        1, 7, 18, 18, 18, 18, 7, 1,    0, 0, 0, 0, 0, 0, 0, 0, 
        1, 7, 18, 27, 27, 18, 7, 1,    0, 0, 0, 0, 0, 0, 0, 0, 
        1, 7, 18, 27, 27, 18, 7, 1,    0, 0, 0, 0, 0, 0, 0, 0, 
        1, 7, 18, 18, 18, 18, 7, 1,    0, 0, 0, 0, 0, 0, 0, 0, 
        1, 7,  7,  7,  7,  7, 7, 1,    0, 0, 0, 0, 0, 0, 0, 0,
        1, 1,  1,  1,  1,  1, 1, 1,    0, 0, 0, 0, 0, 0, 0, 0
    };

    /// <summary> Directional vectors of where the piece can go </summary>
    public static int[] moveVectors = { 33, 18, -14, -31, -33, -18, 14, 31 };

    /// <summary> Initializes a new instance of the <see cref="PieceKnight"/> class. </summary>
    /// <param name="pieceBase"> The piece base. </param>
    public PieceKnight(Piece pieceBase)
        => this.Base = pieceBase;

    /// <summary> Gets the piece Abbreviation. </summary>
    public string Abbreviation => "N";

    /// <summary> Gets the base part of the piece. i.e. the bit that sits on the chess square. </summary>
    public Piece Base { get; private set; }

    /// <summary> Gets basic value of the piece. e.g. pawn = 1, bishop = 3, queen = 9 </summary>
    public int BasicValue => 3;

    /// <summary> Gets the image index for this piece. Used to determine which graphic image is displayed for thie piece.</summary>
    public int ImageIndex
            => this.Base.Player.Colour == Player.PlayerColourNames.White ? 7 : 6;

    /// <summary> Gets a value indicating whether the piece is capturable. Kings aren't, everything else is. </summary>
    public bool IsCapturable => true;

    /// <summary> Gets the piece's name. </summary>
    public Piece.PieceNames Name => Piece.PieceNames.Knight;

    // TODO : Make this a method 
    /// <summary> Gets the positional points assigned to this piece. </summary>
    public int PositionalPoints
    {
        get
        {
            int intPoints = 0;

            if (Game.Stage == Game.GameStageNames.End)
            {
                intPoints -= this.Base.TaxiCabDistanceToEnemyKingPenalty() << 4;
            }
            else
            {
                intPoints += SquareValues[this.Base.Square.Ordinal] << 3;

                if (this.Base.CanBeDrivenAwayByPawn())
                {
                    intPoints -= 30;
                }
            }

            intPoints += this.Base.DefensePoints;

            return intPoints;
        }
    }

    /// <summary> Gets the material value of this piece. </summary>
    public int Value => 3250;
    // raise the knight's value by 1/16 for each pawn above five of the side being valued,
    // with the opposite adjustment for each pawn short of five;    
    // + ((m_Base.Player.PawnsInPlay-5) * 63);  

    /// <summary> 
    /// Generate "lazy" moves for this piece, which is all usual legal moves, 
    /// but also includes moves that put the king in check.
    /// </summary>
    /// <param name="moves"> Moves list that will be populated with lazy moves. </param>
    /// <param name="movesType"> Types of moves to include. e.g. All, or captures-only. </param>
    public void GenerateLazyMoves(Moves moves, Moves.MoveListNames movesType)
    {
        Square? square;
        switch (movesType)
        {
            case Moves.MoveListNames.All:
                for (int i = 0; i < moveVectors.Length; i++)
                {
                    square = Board.GetSquare(this.Base.Square.Ordinal + moveVectors[i]);
                    if (square != null && 
                       (square.Piece == null || 
                       (square.Piece.Player.Colour != this.Base.Player.Colour && square.Piece.IsCapturable)))
                    {
                        moves.Add(0, 0, Move.MoveNames.Standard, this.Base, this.Base.Square, square, square.Piece, 0, 0);
                    }
                }
                break;

            case Moves.MoveListNames.CapturesPromotions:
                for (int i = 0; i < moveVectors.Length; i++)
                {
                    square = Board.GetSquare(this.Base.Square.Ordinal + moveVectors[i]);
                    if (square != null && (square.Piece != null && (square.Piece.Player.Colour != this.Base.Player.Colour && square.Piece.IsCapturable)))
                    {
                        moves.Add(0, 0, Move.MoveNames.Standard, this.Base, this.Base.Square, square, square.Piece, 0, 0);
                    }
                }
                break;
        }
    }

    public bool CanAttackSquare(Square targetSquare)
    {
        for (int i = 0; i < moveVectors.Length; i++)
        {
            Square? square = Board.GetSquare(this.Base.Square.Ordinal + moveVectors[i]);
            if (square is not null && square.Ordinal == targetSquare.Ordinal)
            {
                return true;
            }
        }
        return false;
    }

    static private Piece.PieceNames _pieceType = Piece.PieceNames.Knight;
    
    /// <summary> static method to determine if a square is attacked by this piece </summary>
    /// <param name="square"></param>
    /// <param name="player"></param>
    /// <returns></returns>
    static public bool DoesPieceAttackSquare(Square square, Player player)
        => Piece.DoesLeaperPieceTypeAttackSquare(square, player, _pieceType, moveVectors);

    static public bool DoesPieceAttackSquare(Square square, Player player, out Piece attackingPiece)    
        => Piece.DoesLeaperPieceTypeAttackSquare(square, player, _pieceType, moveVectors, out attackingPiece);

    // TODO: Figure out why these were commented out and whether they are needed
    //
    /*
        static public bool DoesPieceAttackSquare(Square square, Player player, out Piece attackingpiece)
        {
            Piece piece;
            for (int i = 0; i < moveVectors.Length; i++)
            {
                piece = Board.GetPiece(square.Ordinal + moveVectors[i]);
                if (piece != null && piece.Name == _pieceType && piece.Player.Colour == player.Colour)
                {
                    attackingpiece = piece;
                    return true;
                }
            }
            attackingpiece = null;
            return false;
        }

        static public bool DoesPieceAttackSquare(Square square, Player player)
        {
            Piece piece;
            for (int i = 0; i < moveVectors.Length; i++)
            {
                piece = Board.GetPiece(square.Ordinal + moveVectors[i]);
                if (piece != null && piece.Name == _pieceType && piece.Player.Colour == player.Colour)
                {
                    return true;
                }
            }
            return false;
        }*/
}