namespace Lyt.Chess.Model.ChessObjects;

public class ChessMatch
{
    private static readonly Dictionary<Piece, int> PieceValues = new()
    {
        { Piece.WhitePawn, 1 },
        { Piece.BlackPawn, 1 },
        { Piece.WhiteKnight, 3 },
        { Piece.BlackKnight, 3 },
        { Piece.WhiteBishop, 3 },
        { Piece.BlackBishop, 3 },
        { Piece.WhiteRook, 5 },
        { Piece.BlackRook, 5 },
        { Piece.WhiteQueen, 9 },
        { Piece.BlackQueen, 9 },
        { Piece.WhiteKing, 0 },
        { Piece.BlackKing, 0 }
    };

    public ChessMatch(bool isPlayingWhite)
    {
        this.WhiteCapturedPieces = [];
        this.BlackCapturedPieces = [];
        this.IsPlayingWhite = isPlayingWhite;
        this.Board = new Board();
    }

    public Board Board { get; private set; }

    public bool IsPlayingWhite { get; private set; }

    public List<Piece> WhiteCapturedPieces { get; set; }

    public List<Piece> BlackCapturedPieces { get; set; }

    public bool IsTied { get; private set; }

    public bool IsLeading { get; private set; }

    public int Lead { get; private set; }

    public int WhiteScore { get; private set; }

    public int BlackScore { get; private set; }

    public int WhitePromotionPoints { get; private set; }

    public int BlackPromotionPoints { get; private set; }

    public void UpdateBoard(Board board) => this.Board = new Board(board);

    public void Capture(Piece piece)
    {
        if (piece == Piece.None)
        {
            return;
        }

        if (piece.IsWhite())
        {
            this.WhiteCapturedPieces.Add(piece);
        }
        else
        {
            this.BlackCapturedPieces.Add(piece);
        }

        Debug.WriteLine($"Captured piece: {piece}");
        this.UpdateScores();
    }

    private void UpdateScores()
    {
        this.WhiteScore =
            this.WhitePromotionPoints +
            this.BlackCapturedPieces.Sum(p => PieceValues[p]);
        this.BlackScore =
            this.BlackPromotionPoints +
            this.WhiteCapturedPieces.Sum(p => PieceValues[p]);
        this.IsTied = this.WhiteScore == this.BlackScore;
        this.IsLeading = this.IsPlayingWhite ? this.WhiteScore > this.BlackScore : this.BlackScore > this.WhiteScore;
        this.Lead = Math.Abs(this.WhiteScore - this.BlackScore);
    }

    internal void Promotion(Piece promotion)
    {
        if (promotion == Piece.None)
        {
            return;
        }

        if (promotion.IsWhite())
        {
            this.WhitePromotionPoints += PieceValues[promotion];
            --this.WhitePromotionPoints;
        }
        else
        {
            this.BlackPromotionPoints += PieceValues[promotion];
            --this.BlackPromotionPoints;
        }

        Debug.WriteLine($"Promoted piece: {promotion}");
        this.UpdateScores();
    }
}
