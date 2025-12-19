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

    public Engine Engine { get; private set; }

    public async void InitializeEngine ()
    {
        this.Engine.UciCommand("uci");
        await Task.Delay(200);
        this.Engine.UciCommand("ucinewgame");
        await Task.Delay(200);
        this.Engine.UciCommand("isready");
        this.Engine.SetupPosition(new Board(Board.STARTING_POS_FEN));
        this.Engine.Start();
        this.Engine.Play(new Move("e2e4"));
        this.Engine.Go(15, 15_000, 10_000);

        //await this.WaitUciTask();
    }

    public TaskCompletionSource<string> Tcs { get; private set; }

    public void UciResponse(string response)
    {
        if ( this.Tcs is null)
        {
            Debug.WriteLine(response);
            return; 
        }

        _ = this.Tcs.TrySetResult(response);
    }

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
