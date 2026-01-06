namespace Lyt.Chess.Model.Utilities;

internal sealed class EngineDriver : IUciResponder 
{
    public static readonly Move NullMove = new(-1, -1);

    private string[] engineLastResponseTokens = [];
    private string engineLastResponseCommand = string.Empty;
    private Move bestMove = NullMove;
    private List<Move> foundMoves = [];

    public EngineDriver() => this.Engine = new Engine(this);

    public Engine Engine { get; private set; }

    public bool IsThinking { get; private set; }

    public bool IsReady { get; private set; }

    public bool HasBestMove => this.bestMove != NullMove;

    public bool HasFoundMoves => this.foundMoves.Count > 0;

    public Move BestMove 
        => this.HasBestMove ? 
                this.bestMove : 
                throw new Exception("Should have checked HasBestMove property.");

    public List<Move> FoundMoves
        => this.HasFoundMoves ?
                this.foundMoves :
                throw new Exception("Should have checked HasFoundMoves property.");

    public async Task<bool> Initialize()
    {
        try
        {
            this.Engine.UciCommand("uci");

            // Should respond with uciok 
            int retries = 3;
            while (retries > 0)
            {
                if (this.engineLastResponseCommand == "uciok")
                {
                    break;
                }

                await Task.Delay(50);
                --retries;
            }

            if (retries == 0)
            {
                return false;
            }

            this.Engine.UciCommand("isready");
            retries = 3;
            while (retries > 0)
            {
                if (this.engineLastResponseCommand == "readyok")
                {
                    break;
                }

                await Task.Delay(50);
                --retries;
            }

            if (retries == 0)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> Start()
    {
        try
        {
            // No response expected for newgame and following commands 
            this.Engine.UciCommand("ucinewgame");
            this.Engine.SetupPosition(new Board(Board.STARTING_POS_FEN));
            this.Engine.Start();
            this.IsReady = true;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> Think(int depth, int maxTime)
    {
        try
        {
            this.IsThinking = true;
            maxTime *= 1_000;

            // Use parameters tuned to human player level 
            this.Engine.Go(depth, maxTime, 20_000_000);

            // Clear previous best move and found moves
            this.bestMove = NullMove;
            this.foundMoves.Clear();

            // Wait until we get a best move 
            int retryDelay = 200;
            int retries = maxTime / retryDelay;
            while (retries > 0)
            {
                if (this.bestMove != NullMove)
                {
                    break;
                }

                await Task.Delay(retryDelay);
                --retries;
            }

            if (retries == 0)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
        finally
        {
            this.IsThinking = false;
        }
    }

    public void Stop()
    {
        try
        {
            this.Engine.Stop();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            this.IsThinking = false;
        }
    }

    public void UciResponse(string response)
    {
        Debug.WriteLine("Uci Response: " + response);

        this.engineLastResponseTokens = response.Split();
        if (this.engineLastResponseTokens.Length > 0)
        {
            this.engineLastResponseCommand = this.engineLastResponseTokens[0];
        }
        else
        {
            this.engineLastResponseCommand = string.Empty;
        }

        if (this.engineLastResponseCommand == "info")
        {
            if (this.engineLastResponseTokens.Length > 0)
            {
                string subCommandString = this.engineLastResponseTokens[1];
                if (subCommandString == "depth")
                {
                    // Use the depth and pv values to create variations or to dumb down the engine 
                    this.foundMoves = ParseDepthInfo(this.engineLastResponseTokens);
                }
            }
        }
        else if (this.engineLastResponseCommand == "bestmove")
        {
            if (this.engineLastResponseTokens.Length == 2)
            {
                try
                {
                    string moveString = this.engineLastResponseTokens[1];
                    var move = new Move(moveString);
                    this.bestMove = move;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
            else
            {
                Debug.WriteLine("Missing bestmove token.");
            }
        }
        else
        {
        }
    }

    // Examples of depth info lines:
    //
    //	info depth 13 score cp -6 nodes 690506 nps 112022 time 6164 pv e7e5 b1c3 b8c6 g1f3 g8f6 f1b5 c6d4 f3e5 d4b5 c3b5 f6e4 d1f3 e4g5
    //	info depth 14 score cp -32 nodes 1648005 nps 113327 time 14542 pv e7e5 g1f3 b8c6 d2d4 e5d4 f3d4 g8f6 d4c6 d7c6 d1d8 e8d8 b1c3 f8b4 c1g5
    private static List<Move> ParseDepthInfo(string[] engineLastResponseTokens)
    {
        List<Move> moves = [];
        int pvIndex = Array.IndexOf(engineLastResponseTokens, "pv");
        if (pvIndex >= 0)
        {
            for (int i = pvIndex + 1; i < engineLastResponseTokens.Length; i++)
            {
                try
                {
                    string moveString = engineLastResponseTokens[i];
                    var move = new Move(moveString);
                    moves.Add(move);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        return moves;
    }

}
