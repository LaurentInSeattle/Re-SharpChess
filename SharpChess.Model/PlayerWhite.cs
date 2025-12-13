namespace SharpChess.Model; 

/// <summary> The player playing white. </summary>
public class PlayerWhite : Player
{
    /// <summary> Initializes a new instance of the <see cref="PlayerWhite"/> class. </summary>
    public PlayerWhite(Game game) : base(game)
    {
        this.Colour = PlayerColourNames.White;
        this.Intellegence = PlayerIntellegenceNames.Human;
        this.SetPiecesAtStartingPositions();
    }

    /// <summary> Gets PawnAttackLeftOffset. </summary>
    public override int PawnAttackLeftOffset => 15;

    /// <summary> Gets PawnAttackRightOffset.
    public override int PawnAttackRightOffset => 17;

    /// <summary> Gets PawnForwardOffset. </summary>
    public override int PawnForwardOffset => 16;

    /// <summary> Set all WHITE pieces at their starting positions. </summary>
    protected override sealed void SetPiecesAtStartingPositions()
    {
        this.Pieces.Add(this.King = new Piece(Piece.PieceNames.King, this, 4, 0, Piece.PieceIdentifierCodes.WhiteKing));

        this.Pieces.Add(new Piece(Piece.PieceNames.Queen, this, 3, 0, Piece.PieceIdentifierCodes.WhiteQueen));

        this.Pieces.Add(new Piece(Piece.PieceNames.Rook, this, 0, 0, Piece.PieceIdentifierCodes.WhiteQueensRook));
        this.Pieces.Add(new Piece(Piece.PieceNames.Rook, this, 7, 0, Piece.PieceIdentifierCodes.WhiteKingsRook));

        this.Pieces.Add(new Piece(Piece.PieceNames.Bishop, this, 2, 0, Piece.PieceIdentifierCodes.WhiteQueensBishop));
        this.Pieces.Add(new Piece(Piece.PieceNames.Bishop, this, 5, 0, Piece.PieceIdentifierCodes.WhiteKingsBishop));

        this.Pieces.Add(new Piece(Piece.PieceNames.Knight, this, 1, 0, Piece.PieceIdentifierCodes.WhiteQueensKnight));
        this.Pieces.Add(new Piece(Piece.PieceNames.Knight, this, 6, 0, Piece.PieceIdentifierCodes.WhiteKingsKnight));

        this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, 0, 1, Piece.PieceIdentifierCodes.WhitePawn1));
        this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, 1, 1, Piece.PieceIdentifierCodes.WhitePawn2));
        this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, 2, 1, Piece.PieceIdentifierCodes.WhitePawn3));
        this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, 3, 1, Piece.PieceIdentifierCodes.WhitePawn4));
        this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, 4, 1, Piece.PieceIdentifierCodes.WhitePawn5));
        this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, 5, 1, Piece.PieceIdentifierCodes.WhitePawn6));
        this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, 6, 1, Piece.PieceIdentifierCodes.WhitePawn7));
        this.Pieces.Add(new Piece(Piece.PieceNames.Pawn, this, 7, 1, Piece.PieceIdentifierCodes.WhitePawn8));
    }
}