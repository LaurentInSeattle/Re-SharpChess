namespace SharpChess.Model; 

/// <summary> Represents the chess board using a 0x88 represenation. http://chessprogramming.wikispaces.com/0x88  </summary>
public sealed class Board
{
    /// <summary>
    ///   Number of files on the chess board.
    /// </summary>
    public const byte FileCount = 8;

    /// <summary>
    ///   Width of board matrix.
    /// </summary>
    public const byte MatrixWidth = 16;

    /// <summary>
    ///   Number of ranks on the chess board.
    /// </summary>
    public const byte RankCount = 8;

    /// <summary>
    ///   Number of square in the board matrix.
    /// </summary>
    public const byte SquareCount = 128;

    /// <summary> List of squares on the board. </summary>
    private static readonly Square[] Squares = new Square[RankCount * MatrixWidth];

    /// <summary>
    ///   Orientation of the board. Black or White at the bottom.
    /// </summary>
    private OrientationNames orientation = OrientationNames.White;

    /// <summary> Initializes members of the <see cref = "Board" /> class. </summary>
    public Board()
    {
        for (int intOrdinal = 0; intOrdinal < SquareCount; intOrdinal++)
        {
            Squares[intOrdinal] = new Square(intOrdinal);
        }
    }

    /// <summary> Valid values for orientation of the board. Black or White at the bottom. </summary>
    public enum OrientationNames
    {
        /// <summary> White at the bottom. </summary>
        White, 

        /// <summary> Black at the bottom. </summary>
        Black
    }

    public string DebugString
    {
        get
        {
            string strOutput = string.Empty;
            int intOrdinal = SquareCount - 1;

            for (int intRank = 0; intRank < RankCount; intRank++)
            {
                for (int intFile = 0; intFile < FileCount; intFile++)
                {
                    Square? square = GetSquare(intOrdinal);
                    if (square != null)
                    {
                        Piece? piece = square.Piece;
                        if (piece is not null)
                        {
                            strOutput += piece.Abbreviation;
                        }
                        else
                        {
                            strOutput += square.Colour == Square.ColourNames.White ? "." : "#";
                        }
                    }

                    strOutput += Convert.ToChar(13) + Convert.ToChar(10);
                    intOrdinal--;
                }
            }

            return strOutput;
        }
    }

    /// <summary> Gets or sets the hash code a. </summary>
    public  ulong HashCodeA { get; set; }

    /// <summary> Gets or sets the hash code b. </summary>
    public  ulong HashCodeB { get; set; }

    /// <summary> Gets or sets Orientation. </summary>
    public  OrientationNames Orientation
    {
        get => orientation;
        set => orientation = value;
    }

    /// <summary> Gets or sets the pawn hash code a. </summary>
    public  ulong PawnHashCodeA { get; set; }

    /// <summary> Gets or sets the pawn hash code b. </summary>
    public  ulong PawnHashCodeB { get; set; }

    /// <summary> Append piece path. </summary>
    /// <param name="moves"> The moves. </param>
    /// <param name="piece"> The piece. </param>
    /// <param name="player"> The player. </param>
    /// <param name="offset"> The offset. </param>
    /// <param name="movesType"> The moves type. </param>
    public  void AppendPiecePath(
        Moves moves, Piece piece, Player player, int offset, Moves.MoveListNames movesType)
    {
        int intOrdinal = piece.Square.Ordinal;
        Square? square;

        intOrdinal += offset;
        while ((square = GetSquare(intOrdinal)) != null)
        {
            if (square.Piece == null)
            {
                if (movesType == Moves.MoveListNames.All)
                {
                    moves.Add(0, 0, Move.MoveNames.Standard, piece, piece.Square, square, null, 0, 0);
                }
            }
            else if (square.Piece.Player.Colour != player.Colour && square.Piece.IsCapturable)
            {
                moves.Add(0, 0, Move.MoveNames.Standard, piece, piece.Square, square, square.Piece, 0, 0);
                break;
            }
            else
            {
                break;
            }

            intOrdinal += offset;
        }
    }

    /// <summary> Establish the hash key. </summary>
    public  void EstablishHashKey()
    {
        HashCodeA = 0UL;
        HashCodeB = 0UL;
        PawnHashCodeA = 0UL;
        PawnHashCodeB = 0UL;
        for (int intOrdinal = 0; intOrdinal < SquareCount; intOrdinal++)
        {
            Piece? piece = GetPiece(intOrdinal);
            if (piece != null)
            {
                HashCodeA ^= piece.HashCodeAForSquareOrdinal(intOrdinal);
                HashCodeB ^= piece.HashCodeBForSquareOrdinal(intOrdinal);
                if (piece.Name == Piece.PieceNames.Pawn)
                {
                    PawnHashCodeA ^= piece.HashCodeAForSquareOrdinal(intOrdinal);
                    PawnHashCodeB ^= piece.HashCodeBForSquareOrdinal(intOrdinal);
                }
            }
        }
    }

    /// <summary> Gets the Board File number from a file name. </summary>
    /// <param name="fileName"> The file name. </param>
    /// <returns> The file number. </returns>
    public int FileFromName(string fileName)
    {
        return fileName switch
        {
            "a" => 0,
            "b" => 1,
            "c" => 2,
            "d" => 3,
            "e" => 4,
            "f" => 5,
            "g" => 6,
            "h" => 7,
            _ => -1,
        };
    }

    /// <summary> Flip the board orientation. </summary>
    public void Flip() 
        => orientation = Orientation == OrientationNames.White ? 
            OrientationNames.Black : 
            OrientationNames.White;

    /// <summary> Gets a piece from an ordinal. </summary>
    /// <param name="ordinal"> The ordinal. </param>
    /// <returns> The corresponding piece or null if the square is empty </returns>
    public Piece? GetPiece(int ordinal) 
        => (ordinal & 0x88) == 0 ? 
                Squares[ordinal].Piece : 
                null;

    /// <summary> Gets a piece from a file and a rank. </summary>
    /// <param name="file"> The file. </param>
    /// <param name="rank"> The rank. </param>
    /// <returns> The corresponding piece or null if the square is empty </returns>
    public Piece? GetPiece(int file, int rank)
        => (OrdinalFromFileRank(file, rank) & 0x88) == 0 ? 
                Squares[OrdinalFromFileRank(file, rank)].Piece : 
                null;

    /// <summary> Gets a square from an ordinal. </summary>
    /// <param name="ordinal"> The ordinal. </param>
    /// <returns> The corresponding square or or exception thrown </returns>
    public Square GetSquare(int ordinal)
        => (ordinal & 0x88) == 0 ? 
            Squares[ordinal] 
            : throw new ApplicationException("Invalid file and/or rank ");

    /// <summary> Gets a square from a file and a rank. </summary>
    /// <param name="file"> The file. </param>
    /// <param name="rank"> The rank. </param>
    /// <returns> The corresponding square or exception thrown </returns>
    public Square GetSquare(int file, int rank)
        => (OrdinalFromFileRank(file, rank) & 0x88) == 0 ? 
                Squares[OrdinalFromFileRank(file, rank)] :
                throw new ApplicationException("Invalid file and/or rank ");

    /// <summary> Gets a square from a label string. </summary>
    /// <param name="label"> The label. </param>
    /// <returns> The corresponding Matching Square or null </returns>
    public Square? GetSquare(string label)
            => Squares[OrdinalFromFileRank(FileFromName(label[..1]), int.Parse(label.Substring(1, 1)) - 1)];

    /// <summary> Populate the provided list of squares with squares for the line threatened by the provided player. </summary>
    /// <param name="player"> The player. </param>
    /// <param name="squares">The list of squares to populate. </param>
    /// <param name="squareStart"> The starting square. </param>
    /// <param name="offset"> The offset. </param>
    public void LineThreatenedBy(Player player, Squares squares, Square squareStart, int offset)
    {
        int intOrdinal = squareStart.Ordinal;
        Square? square;
        intOrdinal += offset;
        while ((square = GetSquare(intOrdinal)) != null)
        {
            if (square.Piece == null)
            {
                squares.Add(square);
            }
            else if (square.Piece.Player.Colour != player.Colour && square.Piece.IsCapturable)
            {
                squares.Add(square);
                break;
            }
            else
            {
                break;
            }

            intOrdinal += offset;
        }
    }

    /// <summary> Returns the first piece found in a vector from the specified Square. </summary>
    /// <param name="colour"> The player's colour. </param>
    /// <param name="pieceName"> The piece name. </param>
    /// <param name="squareStart"> The starting square. </param>
    /// <param name="vectorOffset"> The vector offset. </param>
    /// <returns> The first piece on the line, or null. </returns>
    public Piece? LinesFirstPiece(
        Player.PlayerColourNames colour, Piece.PieceNames pieceName, Square squareStart, int vectorOffset)
    {
        int intOrdinal = squareStart.Ordinal;
        Square? square;
        intOrdinal += vectorOffset;
        while ((square = GetSquare(intOrdinal)) != null)
        {
            if (square.Piece == null)
            {
                // continue searching
            }
            else if (square.Piece.Player.Colour != colour)
            {
                return null;
            }
            else if (square.Piece.Name == pieceName)
            {
                return square.Piece;
            }
            else
            {
                return null;
            }

            intOrdinal += vectorOffset;
        }

        return null;
    }

    /// <summary>
    /// Calculates a positional penalty score for a single open line to a square (usually the king square), 
    /// in a specified direction.
    /// </summary>
    /// <param name="colour"> The player's colour. </param>
    /// <param name="squareStart"> The square piece (king) is on. </param>
    /// <param name="directionOffset"> The direction offset. </param>
    /// <returns> The open line penalty. </returns>
    public int OpenLinePenalty(Player.PlayerColourNames colour, Square squareStart, int directionOffset)
    {
        int intOrdinal = squareStart.Ordinal;
        int intSquareCount = 0;
        int intPenalty = 0;
        Square? square;

        intOrdinal += directionOffset;
        while (intSquareCount <= 2
               &&
               ((square = GetSquare(intOrdinal)) != null
                &&
                (square.Piece == null
                 || (square.Piece.Name != Piece.PieceNames.Pawn && square.Piece.Name != Piece.PieceNames.Rook)
                 || square.Piece.Player.Colour != colour)))
        {
            intPenalty += 75;
            intSquareCount++;
            intOrdinal += directionOffset;
        }

        return intPenalty;
    }

    /// <summary> Get ordinal number of a given square, specified by file and rank. </summary>
    /// <param name="file"> The file. </param>
    /// <param name="rank"> The rank. </param>
    /// <returns> the Ordinal value from file and rank. </returns>
    private int OrdinalFromFileRank(int file, int rank)
        => (rank << 4) | file;
}