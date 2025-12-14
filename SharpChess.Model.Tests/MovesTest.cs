namespace SharpChess.Model.Tests;

[TestClass]
public class MovesTest
{
    /// <summary>
    /// A test for SortByScore. Tests that moves are sorted in descending order.
    /// </summary>
    [TestMethod]
    public void SortByScoreTest()
    {
        Moves moves =
        [
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 0),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 3),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 1),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 3),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 4),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 0),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 6),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 2),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 3),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 8),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 5),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 6),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 7),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 8),
            new Move(0, 0, Move.MoveNames.NullMove, null, null, null, null, 0, 0),
        ];

        moves.SortByScore();

        for (int i = 0; i < moves.Count - 1; i++)
        {
            Assert.IsGreaterThanOrEqualTo(moves[i + 1].Score, moves[i].Score);
        }
    }
}