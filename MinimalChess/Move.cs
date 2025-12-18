namespace MinimalChess; 

public struct Move
{
#pragma warning disable CA2211 
    // Non-constant fields should not be visible

    public static Move BlackCastlingShort = new("e8g8");
    public static Move BlackCastlingLong = new("e8c8");
    public static Move WhiteCastlingShort = new("e1g1");
    public static Move WhiteCastlingLong = new("e1c1");

    public static Move BlackCastlingShortRook = new("h8f8");
    public static Move BlackCastlingLongRook = new("a8d8");
    public static Move WhiteCastlingShortRook = new("h1f1");
    public static Move WhiteCastlingLongRook = new("a1d1");

#pragma warning restore CA2211 

    public readonly byte FromSquare;

    public readonly byte ToSquare;

    public readonly Piece Promotion;

    public Move(int fromIndex, int toIndex)
    {
        FromSquare = (byte)fromIndex;
        ToSquare = (byte)toIndex;
        Promotion = Piece.None;
    }

    public Move(int fromIndex, int toIndex, Piece promotion)
    {
        FromSquare = (byte)fromIndex;
        ToSquare = (byte)toIndex;
        Promotion = promotion;
    }

    public Move(string uciMoveNotation)
    {
        if (uciMoveNotation.Length < 4)
        {
            throw new ArgumentException($"Long algebraic notation expected. '{uciMoveNotation}' is too short!");
        }

        if (uciMoveNotation.Length > 5)
        {
            throw new ArgumentException($"Long algebraic notation expected. '{uciMoveNotation}' is too long!");
        }

        //expected format is the long algebraic notation without piece names
        //https://en.wikipedia.org/wiki/Algebraic_notation_(chess)
        //Examples: e2e4, e7e5, e1g1(white short castling), e7e8q(for promotion)
        string fromSquare = uciMoveNotation[..2];
        string toSquare = uciMoveNotation.Substring(2, 2);
        FromSquare = Notation.ToSquare(fromSquare);
        ToSquare = Notation.ToSquare(toSquare);

        //the presence of a 5th character should mean promotion
        Promotion = (uciMoveNotation.Length == 5) ? Notation.ToPiece(uciMoveNotation[4]) : Piece.None;
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj is Move move)
        {
            return this.Equals(move);
        }

        return false;
    }

    public readonly bool Equals(Move other)
    {
        return 
            (FromSquare == other.FromSquare) && 
            (ToSquare == other.ToSquare) && 
            (Promotion == other.Promotion);
    }

    //int is big enough to represent move fully. maybe use that for optimization at some point
    public override readonly int GetHashCode() =>
        FromSquare + (ToSquare << 8) + ((int)Promotion << 16);

    public static bool operator ==(Move lhs, Move rhs) => lhs.Equals(rhs);

    public static bool operator !=(Move lhs, Move rhs) => !lhs.Equals(rhs);

    public override readonly string ToString()
    {
        //result represents the move in the long algebraic notation (without piece names)
        string result = Notation.ToSquareName(FromSquare);
        result += Notation.ToSquareName(ToSquare);

        //the presence of a 5th character should mean promotion
        if (Promotion != Piece.None)
        {
            result += Notation.ToChar(Promotion);
        }

        return result;
    }
}
