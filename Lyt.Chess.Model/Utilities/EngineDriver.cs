namespace Lyt.Chess.Model.Utilities;

internal sealed class EngineDriver : IUciResponder 
{
    public static readonly Move NullMove = new(-1, -1);

    private string[] engineLastResponseTokens = [];
    private string engineLastResponseCommand = string.Empty;
    private Move bestMove = NullMove;

    public EngineDriver() => this.Engine = new Engine(this);

    public Engine Engine { get; private set; }

    public bool IsThinking { get; private set; }

    public bool IsReady { get; private set; }

    public bool HasBestMove => this.bestMove != NullMove;

    public Move BestMove 
        => this.HasBestMove ? 
                this.bestMove : 
                throw new Exception("Should have checked HasBestMove property.");

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

            // Wait until we get a best move 
            this.bestMove = NullMove;
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
                this.engineLastResponseCommand = this.engineLastResponseTokens[0];

                // TODO: Use the depth and pv values to create variations or to dumb down the engine 
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

    //15:43:51:739	info string legal: a7a6 a7a5 b7b6 b7b5 c7c6 c7c5 d7d6 d7d5 e7e6 e7e5 f7f6 f7f5 g7g6 g7g5 h7h6 h7h5 b8a6 b8c6 g8f6 g8h6
    //15:43:51:739	info string Search scheduled to take 19980ms!
    //15:43:57:989	info depth 13 score cp -6 nodes 690506 nps 112022 time 6164 pv e7e5 b1c3 b8c6 g1f3 g8f6 f1b5 c6d4 f3e5 d4b5 c3b5 f6e4 d1f3 e4g5
    //15:44:06:237	info depth 14 score cp -32 nodes 1648005 nps 113327 time 14542 pv e7e5 g1f3 b8c6 d2d4 e5d4 f3d4 g8f6 d4c6 d7c6 d1d8 e8d8 b1c3 f8b4 c1g5
    //15:44:06:237	bestmove e7e5
}
