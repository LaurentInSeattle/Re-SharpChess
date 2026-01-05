namespace Lyt.Chess.Model;

using static Lyt.Persistence.FileManagerModel;

public enum EndGame
{
    None,
    Checkmate,
    Stalemate,
}

public sealed partial class ChessModel : ModelBase
{
    private const int UiUpdateDelay = 66;

    // Starting moves for the computer when it is playing white 
    private static Move[] StartingMoves =
        [
            // According to Chess Strategy Online: 
            //
            // "First Tier"
            new ("d2d4"), // King's pawn
            new ("e2e4"), // Queen's pawn
            new ("c2c4"), // Queen's bishop
            new ("g1f3"), // King's knight

            // "Decent" 
            //
            new ("g2g3"),
            new ("b2b3"),
            new ("f2f4"),
            new ("b1c3"),
        ];

    [JsonIgnore]
    public Engine Engine { get; private set; }

    [JsonIgnore]
    internal EngineDriver EngineDriver { get; private set; }

    [JsonIgnore]
    public bool IsEngineStarted { get; private set; }

    [JsonIgnore]
    public bool IsGameActive { get; private set; }

    public void GameIsActive(bool isActive = true) => this.IsGameActive = isActive;

    public async void StartEngine()
    {
        try
        {
            bool startSuccess = false;
            bool initSuccess = await this.EngineDriver.Initialize();
            if (!initSuccess)
            {
                // Failed to initialize engine, report to user later 
                if (Debugger.IsAttached) { Debugger.Break(); }
            }
            else
            {
                startSuccess = await this.EngineDriver.Start();
                this.IsEngineStarted = startSuccess;
                if (!startSuccess)
                {
                    // Failed to start engine, report to user later
                    if (Debugger.IsAttached) { Debugger.Break(); }
                }
            }

            if (!startSuccess || !initSuccess)
            {
                // Failed to start or initialize engine 
                new ModelUpdatedMessage(UpdateHint.EngineError, "Failed to initialize").Publish();
                return;
            }

            new ModelUpdatedMessage(UpdateHint.EngineReady, this.Engine.Board).Publish();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("New Game, Exception thrown: " + ex);
            new ModelUpdatedMessage(UpdateHint.UnexpectedError, ex.ToString()).Publish();
        }
    }

    public async void NewGame(bool isPlayingWhite)
    {
        try
        {
            this.StartEngine();
            if (!this.EngineDriver.IsReady)
            {
                new ModelUpdatedMessage(UpdateHint.UnexpectedError, "Engine is not ready").Publish();
                return;
            }

            this.Statistics.TotalGamesStarted += 1;

            // Do not use the CTOR reserved for deserialisation 
            var game = new Game("New", isPlayingWhite);
            this.GameInProgress = game;
            this.GameIsActive();
            this.SaveGame();
            this.timeoutTimer.Start();

            this.dispatcher.OnUiThread(async () =>
            {
                new ModelUpdatedMessage(UpdateHint.NewGame, this.Engine.Board).Publish();
                var legalMoves = new LegalMoves(this.Engine.Board);
                new ModelUpdatedMessage(UpdateHint.LegalMoves, legalMoves).Publish();
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine("New Game, Exception thrown: " + ex);
            new ModelUpdatedMessage(UpdateHint.UnexpectedError, ex.ToString()).Publish();
        }
    }

    private EndGame VerifyLegalMoves(PlayerColor playerColor, bool publish = true)
    {
        var board = this.Engine.Board;
        var legalMoves = new LegalMoves(board);
        if (legalMoves.Count == 0)
        {
            // Stalemate or Checkmate 
            if (board.IsChecked(playerColor))
            {
                // Checkmate (Echec et Mat) - The king is dead 
                new ModelUpdatedMessage(UpdateHint.IsCheckmate, playerColor).Publish();
                return EndGame.Checkmate;
            }
            else
            {
                // Stalemate 
                new ModelUpdatedMessage(UpdateHint.IsStalemate, playerColor).Publish();
                return EndGame.Stalemate;
            }

        }

        if (publish)
        {
            new ModelUpdatedMessage(UpdateHint.LegalMoves, legalMoves).Publish();
        }

        return EndGame.None;
    }

    private void VerifyInCheck(PlayerColor playerColor)
    {
        if (this.Engine.Board.IsChecked(playerColor))
        {
            new ModelUpdatedMessage(UpdateHint.IsChecked, playerColor).Publish();
        }
        else
        {
            new ModelUpdatedMessage(UpdateHint.IsChecked, PlayerColor.None).Publish();
        }
    }

    public async void FirstComputerMove()
    {
        if (this.GameInProgress is null)
        {
            new ModelUpdatedMessage(UpdateHint.UnexpectedError, "No game in progress").Publish();
            return;
        }

        await Task.Delay(UiUpdateDelay);

        // Pick a starting move at random 
        var random = new Random((int)DateTime.Now.Ticks);
        int index = random.Next(ChessModel.StartingMoves.Length);
        Move startMove = ChessModel.StartingMoves[index];
        this.Engine.Play(startMove);

        new ModelUpdatedMessage(UpdateHint.Move, startMove).Publish();

        PlayerColor playerColor = this.Engine.SideToMove;
        _ = this.VerifyLegalMoves(playerColor, publish: true);
    }

    public async void Play(Move move)
    {
        if (this.GameInProgress is null)
        {
            new ModelUpdatedMessage(UpdateHint.UnexpectedError, "No game in progress").Publish();
            return;
        }

        try
        {
            Debug.WriteLine("Play: " + move.ToString());
            var board = this.Engine.Board;

            // Stop if the engine is still thinking
            if (this.EngineDriver.IsThinking)
            {
                this.EngineDriver.Stop();
            }

            // Check capture 
            Piece firstToPiece = board[move.ToSquare];
            Piece firstCapturedPiece = firstToPiece != Piece.None ? firstToPiece : Piece.None;
            if (firstCapturedPiece != Piece.None)
            {
                Debug.WriteLine("Captured: " + firstCapturedPiece.ToString());
                this.GameInProgress.Match.Capture(firstCapturedPiece);
                new ModelUpdatedMessage(UpdateHint.Capture, move.ToSquare).Publish();

                // Wait for the UI to update captures
                await Task.Delay(UiUpdateDelay);
            }

            new ModelUpdatedMessage(UpdateHint.Move, move).Publish();

            // Wait for the UI to update the board 
            await Task.Delay(UiUpdateDelay);

            if (move.Promotion != Piece.None)
            {
                Debug.WriteLine("Promotion to: " + move.Promotion.ToString());
                this.GameInProgress.Match.Promotion(move.Promotion);
            }

            // Human plays
            this.Engine.Play(move);

            // Check end game 
            PlayerColor playerColor = this.Engine.SideToMove;
            EndGame endGame = this.VerifyLegalMoves(playerColor, publish: false);
            if (endGame != EndGame.None)
            {
                // End of game; checkmate or stalemate for white
                return;
            }

            this.VerifyInCheck(playerColor);

            await Task.Delay(UiUpdateDelay);

            // Launch the thinking thread for computer side 
            // TODO : depth and maxTime should depend on difficulty level
            bool success = await this.EngineDriver.Think(depth: 3, maxTime: 2);

            if (!success || !this.EngineDriver.HasBestMove)
            {
                // Failed to find a best move 
                if (Debugger.IsAttached) { Debugger.Break(); }

                // Report error to user   
                new ModelUpdatedMessage(UpdateHint.EngineError, "No best move.").Publish();
                return;
            }

            // TODO:
            // For easy levels, we can pick a random move from the top N moves found by the engine
            // Need to parse the engine output for that. (non uci standard info lines with pv ...)
            // 	info depth 13 score cp -6 nodes 690506 nps 112022 time 6164 pv e7e5 b1c3 b8c6 g1f3 g8f6 f1b5 c6d4 f3e5 d4b5 c3b5 f6e4 d1f3 e4g5
            //  info depth 14 score cp -32 nodes 1648005 nps 113327 time 14542 pv e7e5 g1f3 b8c6 d2d4 e5d4 f3d4 g8f6 d4c6 d7c6 d1d8 e8d8 b1c3 f8b4 c1g5
            //  bestmove e7e5

            Move bestMove = this.EngineDriver.BestMove;
            Debug.WriteLine("Engine Play: " + bestMove.ToString());

            // Check capture 
            Piece secondToPiece = board[bestMove.ToSquare];
            Piece secondCapturedPiece = secondToPiece != Piece.None ? secondToPiece : Piece.None;

            // Update capture
            if (secondCapturedPiece != Piece.None)
            {
                Debug.WriteLine("Engine Captured: " + secondCapturedPiece.ToString());
                this.GameInProgress.Match.Capture(secondCapturedPiece);
                new ModelUpdatedMessage(UpdateHint.Capture, bestMove.ToSquare).Publish();

                // Wait for the UI to update the board 
                await Task.Delay(UiUpdateDelay);
            }

            new ModelUpdatedMessage(UpdateHint.Move, bestMove).Publish();

            // Wait for the UI to update the board 
            await Task.Delay(UiUpdateDelay);

            if (bestMove.Promotion != Piece.None)
            {
                Debug.WriteLine("Promotion to: " + move.Promotion.ToString());
                this.GameInProgress.Match.Promotion(move.Promotion);
            }

            // Update scores
            if ((firstCapturedPiece != Piece.None) || (secondCapturedPiece != Piece.None) || (bestMove.Promotion != Piece.None))
            {
                new ModelUpdatedMessage(UpdateHint.UpdateScores).Publish();
            }

            // Play the computer best move 
            this.Engine.Play(bestMove);

            playerColor = this.Engine.SideToMove;
            endGame = this.VerifyLegalMoves(playerColor, publish: true);
            if (endGame != EndGame.None)
            {
                // End of game; checkmate or stalemate
                return;
            }

            this.VerifyInCheck(playerColor);

            // Launch the thinking thread for human side: the 'best' move found by the engine will not
            // be played, it will be only suggested to the human player on request 
            success = await this.EngineDriver.Think(depth: 9, maxTime: 5);
            if (!success || !this.EngineDriver.HasBestMove)
            {
                // Failed to find a best move 
                if (Debugger.IsAttached) { Debugger.Break(); }

                // Not an error here, we can 'live' without a suggestion, but still a weird situation
            }

            Move suggestedMove = this.EngineDriver.BestMove;
            Debug.WriteLine("Suggested Move: " + suggestedMove.ToString());
            new ModelUpdatedMessage(UpdateHint.SuggestedMove, suggestedMove).Publish();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("New Game, Exception thrown: " + ex);
            new ModelUpdatedMessage(UpdateHint.UnexpectedError, ex.ToString()).Publish();
        }
    }

    #region In-Game Actions 

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
            new ModelUpdatedMessage(UpdateHint.UnexpectedError, ex.ToString()).Publish();
            return null;
        }
    }

    public bool SaveGame()
    {
        // LATER 
        return false;

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
