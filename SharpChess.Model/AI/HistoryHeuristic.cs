namespace SharpChess.Model.AI;

/// <summary>
/// Represents the History Heuristic used to improve moved ordering.
/// http://chessprogramming.wikispaces.com/History+Heuristic
/// </summary>
public class HistoryHeuristic
{
    /// <summary> History table entries for black. </summary>
    private readonly int[,] HistoryTableEntriesforBlack;

    /// <summary> History table entries for white. </summary>
    private readonly int[,] HistoryTableEntriesforWhite;

    private readonly Game game;

    public HistoryHeuristic(Game game)
    {
        this.game = game;

        this.HistoryTableEntriesforBlack = new int[Board.SquareCount, Board.SquareCount];
        this.HistoryTableEntriesforWhite = new int[Board.SquareCount, Board.SquareCount];
    }

    /// <summary> Clear all history heuristic values. </summary>
    public void Clear()
    {
        for (int i = 0; i < Board.SquareCount; i++)
        {
            for (int j = 0; j < Board.SquareCount; j++)
            {
                this.HistoryTableEntriesforWhite[i, j] = 0;
                this.HistoryTableEntriesforBlack[i, j] = 0;
            }
        }
    }

    /// <summary> Record a new history entry. </summary>
    /// <param name="colour"> The player colour. </param>
    /// <param name="ordinalFrom"> The From square ordinal. </param>
    /// <param name="ordinalTo"> The To square ordinal. </param>
    /// <param name="value"> The history heuristic weighting value. </param>
    public void Record(Player.PlayerColourNames colour, int ordinalFrom, int ordinalTo, int value)
    {
        // Disable if this feature when switched off.
        if (!this.game.EnableHistoryHeuristic)
        {
            return;
        }

        if (colour == Player.PlayerColourNames.White)
        {
            this.HistoryTableEntriesforWhite[ordinalFrom, ordinalTo] += value;
        }
        else
        {
            this.HistoryTableEntriesforBlack[ordinalFrom, ordinalTo] += value;
        }
    }

    /// <summary> Retrieve a value from the History Heuristic table. </summary>
    /// <param name="colour"> The player colour. </param>
    /// <param name="ordinalFrom"> The From square ordinal. </param>
    /// <param name="ordinalTo"> The To square ordinal. </param>
    /// <returns> The history heuristic weighting value. </returns>
    public int Retrieve(Player.PlayerColourNames colour, int ordinalFrom, int ordinalTo)
        => colour == Player.PlayerColourNames.White ?
            this.HistoryTableEntriesforWhite[ordinalFrom, ordinalTo] :
            this.HistoryTableEntriesforBlack[ordinalFrom, ordinalTo];
}