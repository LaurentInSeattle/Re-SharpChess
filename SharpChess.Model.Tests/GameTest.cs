namespace SharpChess.Model.Tests; 

/// <summary>
///This is a test class for GameTest and is intended
///to contain all GameTest Unit Tests
///</summary>
[TestClass()]
public class GameTest
{
    /// <summary>
    /// A test for Move Ordering - Mid game
    /// </summary>
    [TestMethod]
    public void MoveOrdering_MidGame()
    {
        int positions = NodeCountTest("r2qk2r/ppp2ppp/2b5/4N3/1b1Pp3/8/PPP1QPPP/R1B2RK1 b k - 1 11", 5);

        // Assert.IsTrue(positions == 52931); Before finding pawn king hash score b-u-g.
        // Assert.IsTrue(positions == 94138); Before all captures in quiesence.
        // Assert.IsTrue(positions == 89310); Before reinstating extensions/reductions
        // Assert.IsTrue(positions == 58090); Dont reduce PV node.
        // Assert.IsTrue(positions == 58090); Before MVV/LVA if SEE returns zero.
        // Assert.IsTrue(positions == 54573); Before history * 100
        // Assert.AreEqual(49641, positions); Less nodes without PVS, but more time WTF!
        // Assert.AreEqual(53728, positions); Before losing capture ignored in quiescense.
        // Assert.AreEqual(50205, positions); Clear history and killer moves at the start of each iteration.
        // Assert.AreEqual(48483, positions); Add LMR, and feature enabling
        // Assert.IsTrue(positions == 33033 || positions == 33055); Moved reduction into own method.
        Assert.IsTrue(positions == 33114 || positions == 33080 || positions == 34947 || positions == 34851);
    }

    /// <summary>
    /// A test for Move Ordering - at the start of a game - no moves played.
    /// </summary>
    [TestMethod]
    public void MoveOrdering_Opening()
    {
        int positions = NodeCountTest(string.Empty, 5);
       Assert.AreEqual(11203, positions);
    }


    /// <summary>
    /// A test for Move Ordering - in the end game with a posible promotion
    /// </summary>
    [TestMethod]
    public void MoveOrdering_EndGameWithPromotion()
    {
        int positions = NodeCountTest("8/2R2pk1/2P5/2r5/1p6/1P2Pq2/8/2K1B3 w - - 5 44", 5);
        Assert.AreEqual(31690, positions);
    }

    /// <summary>
    /// A test to confirm that the eval (score) function hasn't unexpectedly changed.
    /// </summary>
    [TestMethod]
    public void ScoreEvalHasntChanged()
    {
        const string Fen = "r2qk2r/ppp2ppp/2b5/4N3/1b1Pp3/8/PPP1QPPP/R1B2RK1 b k - 1 11";
        var game = new Game();
        game.NewInternal(Fen);
        game.MaximumSearchDepth = 3;
        game.ClockFixedTimePerMove = new TimeSpan(0, 10, 0); // 10 minute max
        game.UseRandomOpeningMoves = false;
        game.PlayerToPlay.Brain.Think();

        Assert.AreEqual(-441, game.PlayerToPlay.Score);
    }

    private static int NodeCountTest(string fen, int depth)
    {
        var game = new Game();
        game.NewInternal(fen);
        game.MaximumSearchDepth = depth;
        game.ClockFixedTimePerMove = new TimeSpan(0, 10, 0); // 10 minute max
        game.UseRandomOpeningMoves = false;
        game.PlayerToPlay.Brain.Think();

        // TimeSpan elpased = Game_Accessor.PlayerToPlay.Brain.ThinkingTimeElpased;
        return game.PlayerToPlay.Brain.Search.PositionsSearchedThisTurn;
    }
}
