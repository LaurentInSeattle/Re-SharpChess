namespace MinimalChessEngine;

public sealed partial class Engine : IUciRequester
{
    const string NAME_VERSION = "MinimalChess 0.6.3";
    const string AUTHOR = "Thomas Jahn";

    private void RespondUci(string response)
        => this.uciResponder.UciResponse(response);

    public void UciCommand(string command)
    {
        // remove leading & trailing whitecases and split using the space char as delimiter
        string[] tokens = command.Trim().Split();
        switch (tokens[0])
        {
            case "uci":
                this.RespondUci($"id name {NAME_VERSION}");
                this.RespondUci($"id author {AUTHOR}");
                this.RespondUci($"option name Hash type spin default {Transpositions.DEFAULT_SIZE_MB} min 1 max 2047");//consider gcAllowVeryLargeObjects if larger TT is needed
                this.RespondUci("uciok");
                break;
            case "isready":
                this.RespondUci("readyok");
                break;

            case "position":
                this.UciPosition(tokens);
                break;

            case "go":
                this.UciGo(tokens);
                break;

            case "ucinewgame":
                Transpositions.Clear();
                break;

            case "stop":
                this.Stop();
                break;

            case "quit":
                this.Quit();
                break;

            case "setoption":
                Engine.UciSetOption(tokens);
                break;

            default:
                this.RespondUci("UNKNOWN INPUT " + command);
                return;
        }
    }

    private static void UciSetOption(string[] tokens)
    {
        if (tokens[1] == "name" &&
            tokens[2] == "Hash" &&
            tokens[3] == "value" &&
            int.TryParse(tokens[4], out int hashSizeMBytes))
        {
            Transpositions.Resize(hashSizeMBytes);
        }
    }

    private void UciPosition(string[] tokens)
    {
        // position [fen <fenstring> | startpos ]  moves <move1> .... <movei>
        if (tokens[1] == "startpos")
        {
            this.SetupPosition(new Board(Board.STARTING_POS_FEN));
        }
        else if (tokens[1] == "fen") //rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
        {
            string fen = string.Join(' ', tokens[2..]);
            this.SetupPosition(new Board(fen));
        }
        else
        {
            this.UciLog("'position' parameters missing or not understood. Assuming 'startpos'.");
            this.SetupPosition(new Board(Board.STARTING_POS_FEN));
        }

        int firstMove = Array.IndexOf(tokens, "moves") + 1;
        if (firstMove == 0)
        {
            return;
        }

        for (int i = firstMove; i < tokens.Length; i++)
        {
            var move = new Move(tokens[i]);
            this.Play(move);
        }

        // Emit an info string message providing all legal moves in the new current position
        string legalMoves = this.LegalMoves();
        this.RespondUci(string.Format("info string legal: {0}", legalMoves));

        // TODO: Emit an info string message about the check status in the new current position
        // TODO: Emit an info string message about the castle status in the new current position
        // TODO: Emit an info string message about the squares under attack in the new current position
    }

    private string LegalMoves()
    {

        var sb = new StringBuilder();
        var search = new IterativeSearch(1, this.board);

        // Maybe not needed? If absent, search will warn about not used variable
        _ = search.PrincipalVariation;
        foreach (var move in new LegalMoves(board))
        {
            _ = sb.Append(move.ToString()).Append(' ');
        }

        return sb.ToString().TrimEnd();
    }

    private void UciGo(string[] tokens)
    {
        // Searching on a budget that may increase at certain intervals
        // 40 Moves in 5 Minutes = go wtime 300000 btime 300000 movestogo 40
        // 40 Moves in 5 Minutes, 1 second increment per Move =  go wtime 300000 btime 300000 movestogo 40 winc 1000 binc 1000 movestogo 40
        // 5 Minutes total, no increment (sudden death) = go wtime 300000 btime 300000

        TryParse(tokens, "depth", out int maxDepth, 99);
        TryParse(tokens, "movetime", out int maxTime, int.MaxValue);
        TryParse(tokens, "nodes", out long maxNodes, long.MaxValue);

        // assuming 30 e.g. spend 1/30th of total budget on the move
        TryParse(tokens, "movestogo", out int movesToGo, 40); 

        if (this.SideToMove == PlayerColor.White && TryParse(tokens, "wtime", out int whiteTime))
        {
            TryParse(tokens, "winc", out int whiteIncrement);
            this.Go(whiteTime, whiteIncrement, movesToGo, maxDepth, maxNodes);
        }
        else if (this.SideToMove == PlayerColor.Black && TryParse(tokens, "btime", out int blackTime))
        {
            TryParse(tokens, "binc", out int blackIncrement);
            this.Go(blackTime, blackIncrement, movesToGo, maxDepth, maxNodes);
        }
        else
        {
            //Searching infinite within optional constraints
            this.Go(maxDepth, maxTime, maxNodes);
        }
    }

    public void BestMove(Move move) => this.RespondUci($"bestmove {move}");

    public void Info(int depth, int score, long nodes, int timeMs, Move[] pv)
    {
        double tS = Math.Max(1, timeMs) / 1000.0;
        int nps = (int)(nodes / tS);
        this.RespondUci($"info depth {depth} score {ScoreToString(score)} nodes {nodes} nps {nps} time {timeMs} pv {string.Join(' ', pv)}");
    }

    private static bool TryParse(string[] tokens, string name, out int value, int defaultValue = 0)
    {
        if (int.TryParse(Token(tokens, name), out value))
        {
            return true;
        }

        //token couldn't be parsed. use default value
        value = defaultValue;
        return false;
    }

    private static bool TryParse(string[] tokens, string name, out long value, long defaultValue = 0)
    {
        if (long.TryParse(Token(tokens, name), out value))
        {
            return true;
        }

        // token couldn't be parsed. use default value
        value = defaultValue;
        return false;
    }

    private static string? Token(string[] tokens, string name)
    {
        int iParam = Array.IndexOf(tokens, name);
        if (iParam < 0)
        {
            return null;
        }

        int iValue = iParam + 1;
        return (iValue < tokens.Length) ? tokens[iValue] : null;
    }

    private static string ScoreToString(int score)
    {
        if (Evaluation.IsCheckmate(score))
        {
            int sign = Math.Sign(score);
            int moves = Evaluation.GetMateDistance(score);
            return $"mate {sign * moves}";
        }

        return $"cp {score}";
    }

    public void UciLog(string message)
        => this.RespondUci($"info string {message}");
}
