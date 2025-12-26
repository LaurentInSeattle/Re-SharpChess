namespace Lyt.Chess.Model.ChessObjects;

public class ChessMatch
{
    public ChessMatch(bool isPlayingWhite)
    {
        this.WhiteCapturedPieces = [];
        this.BlackCapturedPieces = [];
        this.IsPlayingWhite = isPlayingWhite;
        this.Board = new Board();
    }

    [JsonIgnore]
    public Board Board { get; private set; }

    public bool IsPlayingWhite { get; private set; }

    public List<Piece> WhiteCapturedPieces { get; set; }

    public List<Piece> BlackCapturedPieces { get; set; }

    public int WhiteScore { get; private set; }

    public int BlackScore { get; private set; }

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

        this.UpdateScores();

    }

    private void UpdateScores()
    {
        // TODO 
    }
}
