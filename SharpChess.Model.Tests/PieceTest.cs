
namespace SharpChess.Model.Tests; 

/// <summary>
/// This is a test class for GameTest and is intended
///   to contain all GameTest Unit Tests
/// </summary>
[TestClass]
public class PieceTests
{

    [TestMethod]
    public void SquareAttackTest()
    {
        var game = new Game();
        var board = game.Board; 

        string fen = "k7/8/8/8/3N4/8/8/K7 w - - 0 1";
        game.NewInternal(fen);
        Square? s;
        // white king is in the corner, check that it can attack everything around it
        s = board.GetSquare("b1");
        Assert.IsTrue(s.PlayerCanAttackSquare(game.PlayerWhite));
        s = board.GetSquare("a2");
        Assert.IsTrue(s.PlayerCanAttackSquare(game.PlayerWhite));
        s = board.GetSquare("b2");
        Assert.IsTrue(s.PlayerCanAttackSquare(game.PlayerWhite));

        // white can't attack far away squares
        s = board.GetSquare("h8");
        Assert.IsFalse(s.PlayerCanAttackSquare(game.PlayerWhite));
        s = board.GetSquare("b8");
        Assert.IsFalse(s.PlayerCanAttackSquare(game.PlayerWhite));

        // white knights can attack b3
        s = board.GetSquare("b3");
        Assert.IsTrue(s.PlayerCanAttackSquare(game.PlayerWhite));

        // black king can attack around it
        s = board.GetSquare("a7");
        Assert.IsTrue(s.PlayerCanAttackSquare(game.PlayerBlack));

        s = board.GetSquare("b7");
        Assert.IsTrue(s.PlayerCanAttackSquare(game.PlayerBlack));

        s = board.GetSquare("b8");
        Assert.IsTrue(s.PlayerCanAttackSquare(game.PlayerBlack));

    }

    [TestMethod]
    public void SquareAttackPerfTest()
    {
        var game = new Game();
        var board = game.Board;

        string fen = "3k4/8/8/8/8/3K4/8/8";
        game.NewInternal(fen);
        Square s;

        Stopwatch stopwatch = new ();
        stopwatch.Start();
        for (int i = 0; i < 1000000; i++)
        {

            s = board.GetSquare("c2");
            _ = s.PlayerCanAttackSquare(game.PlayerWhite);
        }

        stopwatch.Stop();
        s = board.GetSquare("c2");
        Assert.IsTrue(s.PlayerCanAttackSquare(game.PlayerWhite));
        Debug.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
    }


    [TestMethod]
    public void SquareAttackByBishopTest()
    {
        var game = new Game();
        var board = game.Board;

        string fen = "8/5k2/1p3P2/8/3B4/8/8/K7 w - - 0 1";
        game.NewInternal(fen);
        Square? square;

        string[] good_squares = ["b6", "c5", "e5", "f6", "g7", "e7", "b1", "b2", "a2"];
        foreach (string s in good_squares)
        {
            square = board.GetSquare(s);
            Assert.IsTrue(square.PlayerCanAttackSquare(game.PlayerWhite));
        }

        string[] bad_squares = ["a6", "c4", "e2", "e1", "f8", "h8"];
        foreach (string s in bad_squares)
        {
            square = board.GetSquare(s);
            Assert.IsFalse(square.PlayerCanAttackSquare(game.PlayerWhite));
        }

    }

    [TestMethod]
    public void AttackByKnightTest()
    {
        var game = new Game();
        var board = game.Board;

        // just test that a knight in center of board can attack squares
        string fen = "8/8/8/8/3N4/8/8/8 w - - 0 1";
        game.NewInternal(fen);
        string[] good_squares = ["b3", "b5", "b5", "c2", "c6", "e2", "e6", "f3", "f5"];

        Piece knight = game.PlayerWhite.Pieces.Item(0);
        foreach (string s in good_squares)
        {
            bool canAttack = knight.CanAttackSquare(board.GetSquare(s));
            Assert.IsTrue(canAttack);
        }

        string[] bad_squares = ["a3", "b6", "b7", "c1", "c5", "e1", "e8", "f4", "h5"];
        foreach (string s in bad_squares)
        {
            bool canAttack = knight.CanAttackSquare(board.GetSquare(s));
            Assert.IsFalse(canAttack);
        }

    }

    [TestMethod]
    public void AttackByKingTest()
    {
        var game = new Game();
        var board = game.Board;

        string fen = "8/8/8/8/3K4/8/8/8 w - - 0 1";
        game.NewInternal(fen);
        string[] good_squares = ["c3", "c4", "c5", "d3", "d5", "e3", "e4", "e5"];

        Piece king = game.PlayerWhite.Pieces.Item(0);
        foreach (string s in good_squares)
        {
            bool canAttack = king.CanAttackSquare(board.GetSquare(s));
            Assert.IsTrue(canAttack);
        }

        string[] bad_squares = ["b3", "b5", "a8", "c2", "c6", "d4", "e6", "f3", "f5"];
        foreach (string s in bad_squares)
        {
            bool canAttack = king.CanAttackSquare(board.GetSquare(s));
            Assert.IsFalse(canAttack);
        }
    }


    [TestMethod]
    public void AttackByBishopTest()
    {
        var game = new Game();
        var board = game.Board;

        string fen = "8/8/8/8/3B4/8/8/8 w - - 0 1";
        game.NewInternal(fen);
        string[] good_squares = [ "a1", "b2", "c3", "e5", "f6", "g7", "h8",
                                "a7", "b6", "c5", "e3", "f2", "g1"];

        Piece bishop = game.PlayerWhite.Pieces.Item(0);
        foreach (string s in good_squares)
        {
            bool canAttack = bishop.CanAttackSquare(board.GetSquare(s));
            Assert.IsTrue(canAttack);
        }

        string[] bad_squares = ["b3", "b5", "a8", "c2", "c6", "d4", "e6", "f3", "f5"];
        foreach (string s in bad_squares)
        {
            bool canAttack = bishop.CanAttackSquare(board.GetSquare(s));
            Assert.IsFalse(canAttack);
        }
    }

    [TestMethod]
    public void AttackbyRookTest()
    {
        var game = new Game();
        var board = game.Board;

        string fen = "8/8/8/8/3R4/8/8/8 w - - 0 1";
        game.NewInternal(fen);
        string[] good_squares = [ "a4", "b4", "c4", "e4", "f4", "g4", "h4",
                                "d1", "d2", "d3", "d5", "d6", "d7", "d8"];

        Piece rook = game.PlayerWhite.Pieces.Item(0);
        foreach (string s in good_squares)
        {
            bool canAttack = rook.CanAttackSquare(board.GetSquare(s));
            Assert.IsTrue(canAttack);
        }

        string[] bad_squares = ["b3", "b5", "a8", "c2", "c6", "d4", "e6", "f3", "f5"];
        foreach (string s in bad_squares)
        {
            bool canAttack = rook.CanAttackSquare(board.GetSquare(s));
            Assert.IsFalse(canAttack);
        }
    }

    [TestMethod]
    public void AttackbyQueenTest()
    {
        var game = new Game();
        var board = game.Board;

        string fen = "8/8/8/8/3Q4/8/8/8 w - - 0 1";
        game.NewInternal(fen);
        string[] good_squares = [ "a4", "b4", "c4", "e4", "f4", "g4", "h4",
                                "d1", "d2", "d3", "d5", "d6", "d7", "d8",
                                "a1", "b2", "c3", "e5", "f6", "g7", "h8",
                                "a7", "b6", "c5", "e3", "f2", "g1"
                                ];

        Piece queen = game.PlayerWhite.Pieces.Item(0);
        foreach (string s in good_squares)
        {
            bool canAttack = queen.CanAttackSquare(board.GetSquare(s));
            Assert.IsTrue(canAttack);
        }

        string[] bad_squares = ["b3", "b5", "a8", "c2", "c6", "d4", "e6", "f3", "f5"];
        foreach (string s in bad_squares)
        {
            bool canAttack = queen.CanAttackSquare(board.GetSquare(s));
            Assert.IsFalse(canAttack);
        }
    }

    [TestMethod]
    public void AttackbyPawnTest()
    {
        var game = new Game();
        var board = game.Board;

        string fen = "8/8/8/8/3P4/8/8/8 w - - 0 1";
        game.NewInternal(fen);
        string[] good_squares = ["c5", "e5"];

        Piece pawn = game.PlayerWhite.Pieces.Item(0);            
        foreach (string s in good_squares)
        {
            bool canAttack = pawn.CanAttackSquare(board.GetSquare(s));
            Assert.IsTrue(canAttack);
        }

        string[] bad_squares = ["b3", "b5", "a8", "c2", "c6", "d4", "e6", "f3", "f5"];
        foreach (string s in bad_squares)
        {
            bool canAttack = pawn.CanAttackSquare(board.GetSquare(s));
            Assert.IsFalse(canAttack);
        }
    }

    [TestMethod]
    public void CheapestPieceDefendingThisSquareTest()
    {
        var game = new Game();
        var board = game.Board;

        string fen = "1b1k3r/p1q3r1/npp1pp1n/2p5/8/3K4/8/8";
        game.NewInternal(fen);
        Square s = board.GetSquare("g4");
        Piece p = s.CheapestPieceDefendingThisSquare(game.PlayerBlack);
        Assert.AreEqual(Piece.PieceNames.Knight, p.Top.Name);

        // time this
        Stopwatch stopwatch = new();
        stopwatch.Start();
        for (int i = 0; i < 1000000; i++)
        {
            _ = s.CheapestPieceDefendingThisSquare(game.PlayerBlack);
        }
        stopwatch.Stop();
        Debug.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);
    }

    [TestMethod]
    public void CanPieceTypeAttackSquareTest()
    {
        var game = new Game();
        var board = game.Board;

        string fen = "1b1k3r/p1q3r1/npp1pp1n/2p5/8/3K4/8/8";
        game.NewInternal(fen);
        Square s = board.GetSquare("g4");
        Assert.IsTrue(Piece.CanPlayerPieceNameAttackSquare(s,game.PlayerBlack,Piece.PieceNames.Rook));
        Assert.IsTrue(Piece.CanPlayerPieceNameAttackSquare(s,game.PlayerBlack,Piece.PieceNames.Knight));
        Assert.IsFalse(Piece.CanPlayerPieceNameAttackSquare(s,game.PlayerBlack,Piece.PieceNames.Queen));
        Assert.IsFalse(Piece.CanPlayerPieceNameAttackSquare(s,game.PlayerBlack,Piece.PieceNames.Bishop));
        Assert.IsFalse(Piece.CanPlayerPieceNameAttackSquare(s,game.PlayerBlack,Piece.PieceNames.Pawn));
        Assert.IsFalse(Piece.CanPlayerPieceNameAttackSquare(s,game.PlayerBlack,Piece.PieceNames.King));


        s = board.GetSquare("d4");
        Assert.IsTrue(Piece.CanPlayerPieceNameAttackSquare(s, game.PlayerWhite, Piece.PieceNames.King));
        Assert.IsTrue(Piece.CanPlayerPieceNameAttackSquare(s, game.PlayerBlack, Piece.PieceNames.Pawn));
    }
}
