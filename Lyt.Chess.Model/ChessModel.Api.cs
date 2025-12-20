namespace Lyt.Chess.Model;

using static Lyt.Persistence.FileManagerModel;

public sealed partial class ChessModel : ModelBase
{
    public bool IsPuzzleDirty { get; private set; }

    public bool IsGameActive { get; private set; }

    public void GameIsActive(bool isActive = true) => this.IsGameActive = isActive;

    public Game? NewGame()
    {
        try
        {
            this.Statistics.TotalGamesStarted += 1;

            var game = new Game();

            this.GameInProgress = game;
            this.SaveGame();
            this.timeoutTimer.Start();
            return game;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Save, Exception thrown: " + ex);
            return null;
        }
    }

    #region In-Game Actions 

    [JsonIgnore]
    public Engine Engine { get; private set; }

    [JsonIgnore]
    public EnginePhase Phase { get; private set; }

    public enum EnginePhase
    {
        None,
        Initialisation,
        GameSetup,
        Play,
        Shutdown,
    }

    public async Task<bool> InitializeEngine()
    {
        try
        {
            this.Phase = EnginePhase.Initialisation;
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

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> StartNewGame()
    {
        try
        {
            this.Phase = EnginePhase.GameSetup;

            // No response expected for newgame and following commands 
            this.Engine.UciCommand("ucinewgame");
            this.Engine.SetupPosition(new Board(Board.STARTING_POS_FEN));
            this.Engine.Start();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    public async Task<bool> Play(Move move)
    {
        try
        {
            this.Phase = EnginePhase.Play;

            // Human plays
            this.Engine.Play(move);
            // TODO: Use parameters tuned to human player level 
            int maxTime = 20_000;
            this.Engine.Go(20, maxTime, 10_000_000);

            // Wait until we get a best move 
            this.bestMove = NullMove; 
            int retryDelay = 250;
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

            // Play the computer best move 
            this.Engine.Play (this.bestMove);
            var legalMoves = new LegalMoves(this.Engine.Board);
            foreach (var legalMove in legalMoves)
            {
                Debug.WriteLine(legalMove.ToString());
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }

    private string[] engineLastResponseTokens = new string[0];
    private string engineLastResponseCommand = string.Empty;
    private static Move NullMove = new Move(-1, -1);
    private Move bestMove = NullMove;

    public TaskCompletionSource<string> Tcs { get; private set; }

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

        if (this.Phase != EnginePhase.Play)
        {
            return;
        }

        if (this.engineLastResponseCommand == "info")
        {
            if (this.engineLastResponseTokens.Length > 0)
            {
                this.engineLastResponseCommand = this.engineLastResponseTokens[0];
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

        //if ( this.Tcs is null)
        //{
        //    Debug.WriteLine(response);
        //    return; 
        //}

        //_ = this.Tcs.TrySetResult(response);
    }

    //15:43:51:739	info string legal: a7a6 a7a5 b7b6 b7b5 c7c6 c7c5 d7d6 d7d5 e7e6 e7e5 f7f6 f7f5 g7g6 g7g5 h7h6 h7h5 b8a6 b8c6 g8f6 g8h6
    //15:43:51:739	info string Search scheduled to take 19980ms!
    //15:43:57:989	info depth 13 score cp -6 nodes 690506 nps 112022 time 6164 pv e7e5 b1c3 b8c6 g1f3 g8f6 f1b5 c6d4 f3e5 d4b5 c3b5 f6e4 d1f3 e4g5
    //15:44:06:237	info depth 14 score cp -32 nodes 1648005 nps 113327 time 14542 pv e7e5 g1f3 b8c6 d2d4 e5d4 f3d4 g8f6 d4c6 d7c6 d1d8 e8d8 b1c3 f8b4 c1g5
    //15:44:06:237	bestmove e7e5

    private async Task<string> WaitUciTask()
    {
        this.Tcs = new TaskCompletionSource<string>(TaskCreationOptions.None);
        string response = await this.Tcs.Task;
        Debug.WriteLine(response);
        return response;
    }

    public void GameIsActive() => this.timeoutTimer.ResetTimeout();

    public void ResumePlaying()
    {
        if (this.GameInProgress is null)
        {
            return;
        }

        var now = DateTime.Now;
        this.GameInProgress.LastPlayed = now;
        this.StartedPlay = now;
    }

    public void PausePlaying()
    {
        if (this.GameInProgress is null)
        {
            return;
        }

        var now = DateTime.Now;
        TimeSpan playedThisSession = now - this.StartedPlay;
        this.GameInProgress.Played += playedThisSession;
        this.Statistics.TotalTimePlayed += playedThisSession;

        if (this.GameInProgress.Played > this.Statistics.LongestGameTimePlayed)
        {
            this.Statistics.LongestGameTimePlayed = this.GameInProgress.Played;
            this.Statistics.LongestGameDate = now;
        }
    }

    public bool IsPuzzleComplete()
    {
        if (this.GameInProgress is null)
        {
            return false;
        }

        bool isComplete = true;
        if (isComplete)
        {
            this.Statistics.TotalGamesCompleted += 1;
            this.SaveGame();
            this.timeoutTimer.Stop();
        }

        return isComplete;
    }

    #endregion In-Game Puzzle Actions 

    #region Load and Save 

    public byte[]? LoadGame(string gameKey)
    {
        try
        {
            // Load from disk and deserialize
            string gameName = Game.GameNameFromKey(gameKey);
            var fileId = new FileId(Area.User, Kind.Json, gameName);
            var game =
                this.fileManager.Load<Game>(fileId) ??
                throw new Exception("Failed to deserialize");

            this.GameInProgress = game;
            byte[]? imageBytes = this.LoadImage();
            if ((imageBytes is null) || (imageBytes.Length < 256))
            {
                throw new Exception("Failed to read image from disk: " + gameName);
            }

            this.timeoutTimer.Start();

            Debug.WriteLine("Game Loaded");
            return imageBytes;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Load, Exception thrown: " + ex);
            return null;
        }
    }

    public bool SaveGame()
    {
        if (this.GameInProgress is null)
        {
            return false;
        }

        try
        {
            lock (this.GameInProgress)
            {

                // Serialize and save to disk
                var fileId = new FileId(Area.User, Kind.Json, this.GameInProgress.GameName);
                this.fileManager.Save(fileId, this.GameInProgress);

                if (this.SavedGames.ContainsKey(this.GameInProgress.Name))
                {
                    this.SavedGames[this.GameInProgress.Name] = this.GameInProgress;
                }
                else
                {
                    this.SavedGames.Add(this.GameInProgress.Name, this.GameInProgress);
                }
            }

            Debug.WriteLine("Game Saved");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Save, Exception thrown: " + ex);
            return false;
        }
    }

    public bool DeleteGame(string key, out string message)
    {
        message = string.Empty;
        if (this.GameInProgress is not null && this.IsGameActive)
        {
            if (this.GameInProgress.Name == key)
            {
                // Cannot delete the game that is currently loaded
                message = "Cannot delete the game that is currently loaded.";
                return false;
            }
        }

        try
        {
            // Delete from disk the four files 
            var fileId = new FileId(Area.User, Kind.Json, Game.GameNameFromKey(key));
            this.fileManager.Delete(fileId);
            //fileId = new FileId(Area.User, Kind.Binary, Game.ThumbnailNameFromKey(key));
            //this.fileManager.Delete(fileId);

            // Clear in memory data 
            this.SavedGames.Remove(key);
            this.ThumbnailCache.Remove(key);

            Debug.WriteLine("Game Deleted");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Delete, Exception thrown: " + ex);
            message = "Delete, Exception thrown: " + ex.Message;
            return false;
        }
    }


    private bool SaveImages(byte[] imageBytes, byte[] thumbnailBytes)
    {
        if (this.GameInProgress is null)
        {
            return false;
        }

        try
        {
            // Save to disk 
            var fileIdThumbnail = new FileId(Area.User, Kind.Binary, this.GameInProgress.ThumbnailName);
            this.fileManager.Save(fileIdThumbnail, thumbnailBytes);

            Debug.WriteLine("Images Saved");
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Save, Exception thrown: " + ex);
            return false;
        }
    }

    private byte[]? LoadImage()
    {
        if (this.GameInProgress is null)
        {
            return null;
        }

        try
        {
            // Load from disk 
            //var fileIdImage = new FileId(Area.User, Kind.Binary, this.GameInProgress.ImageName);
            //byte[] imageBytes = this.fileManager.Load<byte[]>(fileIdImage);

            Debug.WriteLine("Image Loaded");
            return null; //  imageBytes;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Save, Exception thrown: " + ex);
            return null;
        }
    }

    public byte[]? GetThumbnail(string name)
    {
        if (!this.ThumbnailCache.TryGetValue(name, out byte[]? thumbnailBytes))
        {
            try
            {
                // Load from disk 
                var fileIdImage = new FileId(Area.User, Kind.Binary, Game.ThumbnailNameFromKey(name));
                byte[] imageBytes = this.fileManager.Load<byte[]>(fileIdImage);

                Debug.WriteLine("Thumbnail Loaded");
                this.ThumbnailCache.Add(name, imageBytes);
                return imageBytes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Get Thumbnail, Exception thrown: " + ex);
                return null;
            }
        }

        return thumbnailBytes;
    }

    private void OnSaveGame()
    {
        if (this.GameInProgress is null)
        {
            return;
        }

        this.SaveGame();
    }

    #endregion Load and Save 
}
