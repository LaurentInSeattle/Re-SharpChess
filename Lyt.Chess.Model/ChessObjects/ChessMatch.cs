namespace Lyt.Chess.Model.ChessObjects;

public class ChessMatch
{
    public ChessMatch()
    {
    }

    [JsonIgnore]
    public Board Board { get; private set; }

}
