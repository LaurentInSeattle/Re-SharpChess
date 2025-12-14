namespace SharpChess.Model
{
    /// <summary>
    ///   Represents the game of chess over its lifetime. 
    ///   Holds the board, players, turn number and everything related to the chess game in progress.
    /// </summary>
    public sealed class Game
    {
        /// <summary>The available MegaBytes of free computer memory. </summary>
        public const uint AvailableMegaBytes = 16;

        /// <summary> The largest valid Material Count. </summary>
        public const int MaxMaterialCount = 7;

        public readonly Board Board;
        public readonly HashTable HashTable;
        public readonly HashTablePawn HashTablePawn;
        public readonly HashTableCheck HashTableCheck;
        public readonly OpeningBookSimple OpeningBookSimple;
        public readonly HistoryHeuristic HistoryHeuristic;
        public readonly KillerMoves KillerMoves;

        /// <summary> The file name.
        private string saveGameFileName = string.Empty;

        /// <summary> Initializes members of the <see cref="Game" /> class. </summary>
        public Game()
        {
            this.EnableFeatures();

            this.Board = new Board();
            this.ClockIncrementPerMove = new TimeSpan(0, 0, 0);
            this.ClockFixedTimePerMove = new TimeSpan(0, 0, 0);
            this.DifficultyLevel = 1;
            this.ClockTime = new TimeSpan(0, 5, 0);
            this.ClockMaxMoves = 40;
            this.UseRandomOpeningMoves = true;
            this.MoveRedoList = [];
            this.MaximumSearchDepth = 1;
            this.MoveAnalysis = [];
            this.MoveHistory = [];
            this.FenStartPosition = string.Empty;
            this.HashTable = new HashTable(this);
            this.HashTablePawn = new HashTablePawn();
            this.HashTableCheck = new HashTableCheck();
            this.HistoryHeuristic = new HistoryHeuristic(this);
            this.KillerMoves = new KillerMoves(this);
            this.PlayerWhite = new PlayerWhite(this);
            this.PlayerBlack = new PlayerBlack(this);
            this.PlayerToPlay = this.PlayerWhite;

            // Initialize the opening book: Players must exists 
            this.OpeningBookSimple = new OpeningBookSimple(this);
            this.Board.EstablishHashKey();

            this.PlayerWhite.Brain.ReadyToMakeMoveEvent += this.PlayerReadyToMakeMove;
            this.PlayerBlack.Brain.ReadyToMakeMoveEvent += this.PlayerReadyToMakeMove;

            this.BackupGamePath = string.Empty;

            #region Commented OUT : Load Settings from Registry

            //RegistryKey registryKeySoftware = Registry.CurrentUser.OpenSubKey("Software", true);
            //if (registryKeySoftware != null)
            //{
            //    RegistryKey registryKeySharpChess = registryKeySoftware.CreateSubKey(@"PeterHughes.org\SharpChess");

            //    if (registryKeySharpChess != null)
            //    {
            //        if (registryKeySharpChess.GetValue("FileName") == null)
            //        {
            //            saveGameFileName = string.Empty;
            //        }
            //        else
            //        {
            //            saveGameFileName = registryKeySharpChess.GetValue("FileName").ToString();
            //        }

            //        if (registryKeySharpChess.GetValue("ShowThinking") == null)
            //        {
            //            ShowThinking = true;
            //        }
            //        else
            //        {
            //            ShowThinking = registryKeySharpChess.GetValue("ShowThinking").ToString() == "1";
            //        }

            //        // Delete deprecated values
            //        if (registryKeySharpChess.GetValue("EnablePondering") != null)
            //        {
            //            registryKeySharpChess.DeleteValue("EnablePondering");
            //        }

            //        if (registryKeySharpChess.GetValue("DisplayMoveAnalysisTree") != null)
            //        {
            //            registryKeySharpChess.DeleteValue("DisplayMoveAnalysisTree");
            //        }

            //        if (registryKeySharpChess.GetValue("ClockMoves") != null)
            //        {
            //            registryKeySharpChess.DeleteValue("ClockMoves");
            //        }

            //        if (registryKeySharpChess.GetValue("ClockMinutes") != null)
            //        {
            //            registryKeySharpChess.DeleteValue("ClockMinutes");
            //        }
            //    }
            //}

            #endregion Load Settings from Registry

            // TODO 
            // Figure out why this line below was commented out 
            // OpeningBook.BookConvert(Game.PlayerWhite);
        }

        // TODO: Fix the event nullability warnings
        // Consider using weak references for event handlers (use WeakEventManager or similar pattern)

        /// <summary> The game event type, raised to the UI when significant game events occur.</summary>
        public delegate void GameEvent();

        /// <summary> Raised when the board position changes. </summary>
        public event GameEvent? BoardPositionChanged;

        /// <summary> Raised when the game is paused. </summary>
        public event GameEvent? GamePaused;

        /// <summary> Raised when the game is resumed. </summary>
        public event GameEvent? GameResumed;

        /// <summary> Raised when the game is saved. </summary>
        public event GameEvent? GameSaved;

        /// <summary> Raised when settings are updated. </summary>
        public event GameEvent? SettingsUpdated;

        /// <summary> The Game stages. </summary>
        public enum GameStageNames
        {
            /// <summary> The opening. </summary>
            Opening,

            /// <summary> The middle. </summary>
            Middle,

            /// <summary> The end. </summary>
            End
        }

        /// <summary> Gets or sets the Backup Game Path. </summary>
        public string BackupGamePath { private get; set; }

        /// <summary> Gets or sets a value indicating whether CaptureMoveAnalysisData. </summary>
        public bool CaptureMoveAnalysisData { get; set; }

        /// <summary> Gets or sets the Clock Fixed Time Per Move. </summary>
        public TimeSpan ClockFixedTimePerMove { get; set; }

        /// <summary> Gets or sets the Clock Increment Per Move. </summary>
        public TimeSpan ClockIncrementPerMove { get; set; }

        /// <summary> Gets or sets the max number of moves on the clock. e.g. 60 moves in 30 minutes </summary>
        public int ClockMaxMoves { get; set; }

        /// <summary> Gets or sets the Clock Time. </summary>
        public TimeSpan ClockTime { get; set; }

        /// <summary> Gets or sets game Difficulty Level. </summary>
        public int DifficultyLevel { get; set; }

        /// <summary> Gets a value indicating whether Edit Mode is Active. </summary>
        public bool EditModeActive { get; private set; }

        /// <summary> Gets or sets a value indicating whether to use Aspiration Search. </summary>
        public bool EnableAspiration { get; set; }

        /// <summary> Gets or sets a value indicating whether to use Search Extensions. </summary>
        public bool EnableExtensions { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use the history heuristic ( <see cref="HistoryHeuristic" /> class).
        /// </summary>
        public bool EnableHistoryHeuristic { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use the killer move heuristic ( <see cref="KillerMoves" /> class).
        /// </summary>
        public bool EnableKillerMoves { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Null Move Forward Pruning.
        /// </summary>
        public bool EnableNullMovePruning { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether Pondering has been enabled.
        /// </summary>
        public bool EnablePondering { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use PVS Search.
        /// </summary>
        public bool EnablePvsSearch { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Quiescense.
        /// </summary>
        public bool EnableQuiescense { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Search Reductions.
        /// </summary>
        public bool EnableReductions { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Late Move Reductions.
        /// </summary>
        public bool EnableReductionLateMove { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Margin Futilty Reductions.
        /// </summary>
        public bool EnableReductionFutilityMargin { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use Fixed Depth Futilty Reductions.
        /// </summary>
        public bool EnableReductionFutilityFixedDepth { get; set; }

        /// <summary>
        ///   Gets or sets a value indicating whether to use the transposition table ( <see cref="HashTable" /> class).
        /// </summary>
        public bool EnableTranspositionTable { get; set; }

        /// <summary>
        ///   Gets or sets the FEN string for the chess Start Position.
        /// </summary>
        public string FenStartPosition { private get; set; }

        /// <summary>
        ///   Gets or sets FiftyMoveDrawBase. Appears to be a value set when using a FEN string. Doesn't seem quite right! TODO Invesigate FiftyMoveDrawBase.
        /// </summary>
        public static int FiftyMoveDrawBase { get; set; }

        /// <summary> Gets the current game save file name. </summary>
        public string FileName 
            => saveGameFileName == string.Empty ? "New Game" : saveGameFileName;
            
        /// <summary> Gets or sets a value indicating whether Analyse Mode is active. </summary>
        public bool IsInAnalyseMode { get; set; }

        /// <summary> Gets a value indicating whether the game is paused.</summary>
        public bool IsPaused => !this.PlayerToPlay.Clock.IsTicking;

        /// <summary> Gets the lowest material count for black or white. </summary>
        public int LowestMaterialCount
        {
            get
            {
                int intWhiteMaterialCount = this.PlayerWhite.MaterialCount;
                int intBlackMaterialCount = this.PlayerBlack.MaterialCount;
                return intWhiteMaterialCount < intBlackMaterialCount ? intWhiteMaterialCount : intBlackMaterialCount;
            }
        }

        /// <summary> Gets or sets the maximum search depth. </summary>
        public int MaximumSearchDepth { get; set; }

        /// <summary> Gets or sets the list of move-analysis moves. </summary>
        public Moves MoveAnalysis { get; set; }

        /// <summary> Gets the current move history. </summary>
        public Moves MoveHistory { get; private set; }

        /// <summary> Gets the current move number. </summary>
        public int MoveNo => this.TurnNo >> 1;

        /// <summary> Gets the move redo list. </summary>
        public Moves MoveRedoList { get; private set; }

        /// <summary> Gets the player playing white. </summary>
        public Player PlayerWhite { get; private set; }

        /// <summary> Gets the player playing black. </summary>
        public Player PlayerBlack { get; private set; }

        /// <summary> Gets or sets the player to play. </summary>
        public Player PlayerToPlay { get; set; }

        /// <summary> Gets or sets a value indicating whether to show thinking. </summary>
        public bool ShowThinking { get; set; }

        /// <summary> Gets current game stage. </summary>
        public GameStageNames Stage
        {
            get
            {
                if (this.LowestMaterialCount >= Game.MaxMaterialCount)
                {
                    return GameStageNames.Opening;
                }

                return this.LowestMaterialCount <= 3 ? GameStageNames.End : GameStageNames.Middle;
            }
        }

        /// <summary> Gets the ThreadCounter. </summary>
        public int ThreadCounter { get; internal set; }

        /// <summary> Gets the current turn number. </summary>
        public int TurnNo { get; internal set; }

        /// <summary> Gets or sets a value indicating whether to use random opening moves. </summary>
        public bool UseRandomOpeningMoves { get; set; }

        /// <summary> Captures all pieces. </summary>
        public void CaptureAllPieces()
        {
            this.PlayerWhite.CaptureAllPieces();
            this.PlayerBlack.CaptureAllPieces();
        }

        /// <summary> Demotes all pieces. </summary>
        public void DemoteAllPieces()
        {
            this.PlayerWhite.DemoteAllPieces();
            this.PlayerBlack.DemoteAllPieces();
        }

        /// <summary> Load a saved game. </summary>
        /// <param name="fileName"> File name. </param>
        /// <returns> Returns True is game loaded successfully. </returns>
        public bool Load(string fileName)
        {
            this.SuspendPondering();

            this.NewInternal();
            saveGameFileName = fileName;
            bool blnSuccess = this.LoadGame(fileName);
            if (blnSuccess)
            {
                this.SaveBackup();
                this.SendBoardPositionChangeEvent();
            }

            this.PausePlay();
            return blnSuccess;
        }

        /// <summary> Load backup game. </summary>
        /// <returns> Returns True is game loaded successfully. </returns>
        public bool LoadBackup() => this.LoadGame(this.BackupGamePath);

        /// <summary> Make a move.</summary>
        /// <param name="moveName"> The move name. </param>
        /// <param name="piece"> The piece to move. </param>
        /// <param name="square"> The square to move to. </param>
        public void MakeAMove(Move.MoveNames moveName, Piece piece, Square square)
        {
            this.SuspendPondering();
            this.MakeAMoveInternal(moveName, piece, square);
            this.SaveBackup();
            this.SendBoardPositionChangeEvent();
            this.CheckIfAutoNextMove();
        }

        /// <summary> Start a new game. </summary>
        public void New() => this.New(string.Empty);

        /// <summary> Start a new game using a FEN string. </summary>
        /// <param name="fenString"> The FEN string. </param>
        public void New(string fenString)
        {
            this.SuspendPondering();
            this.NewInternal(fenString);
            this.SaveBackup();
            this.SendBoardPositionChangeEvent();
            this.ResumePondering();
        }

        /// <summary> Pause the game. </summary>
        public void PausePlay()
        {
            this.PlayerToPlay.Clock.Stop();
            this.PlayerToPlay.Brain.ForceImmediateMove();
            this.GamePaused();
        }

        /// <summary> Redo all moves. </summary>
        public void RedoAllMoves()
        {
            this.SuspendPondering();
            while (this.MoveRedoList.Count > 0)
            {
                this.RedoMoveInternal();
            }

            this.SaveBackup();
            this.SendBoardPositionChangeEvent();
            this.ResumePondering();
        }

        /// <summary> Redo a move. </summary>
        public void RedoMove()
        {
            this.SuspendPondering();
            this.RedoMoveInternal();
            this.SaveBackup();
            this.SendBoardPositionChangeEvent();
            this.ResumePondering();
        }

        /// <summary> Resume the game. </summary>
        public void ResumePlay()
        {
            this.PlayerToPlay.Clock.Start();
            this.GameResumed();
            if (this.PlayerToPlay.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                this.MakeNextComputerMove();
            }
            else
            {
                this.ResumePondering();
            }
        }

        /// <summary> Resume pondering. </summary>
        public void ResumePondering()
        {
            if (this.IsPaused)
            {
                return;
            }

            if (!this.EnablePondering)
            {
                return;
            }

            if (!this.PlayerToPlay.CanMove)
            {
                return;
            }

            if (this.PlayerWhite.Intellegence == Player.PlayerIntellegenceNames.Computer
                && this.PlayerBlack.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                return;
            }

            if (this.PlayerToPlay.OpposingPlayer.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                if (!this.PlayerToPlay.Brain.IsPondering)
                {
                    this.PlayerToPlay.Brain.StartPondering();
                }
            }
        }

        /// <summary> Save the game as a file name. </summary>
        /// <param name="fileName"> The file name. </param>
        public void Save(string fileName)
        {
            this.SuspendPondering();
            this.SaveBackup();
            this.SaveGame(fileName);
            this.saveGameFileName = fileName;
            this.GameSaved();
            this.ResumePondering();
        }

        /// <summary> Called when settings have been changed in the UI. </summary>
        public void SettingsUpdate()
        {
            this.SuspendPondering();
            //if (!WinBoard.Active)
            //{
            //    SaveBackup();
            //}

            this.SettingsUpdated();
            this.ResumePondering();
        }

        // TODO
        // Figure out why this method was commented out
        /*
                /// <summary> Start normal game. </summary>
                public static void StartNormalGame()
                {
                    PlayerToPlay.Clock.Start();
                    ResumePondering();
                }
        */

        /// <summary> Suspend pondering. </summary>
        public void SuspendPondering()
        {
            if (this.PlayerToPlay.Brain.IsPondering)
            {
                this.PlayerToPlay.Brain.ForceImmediateMove();
            }
            else if (this.PlayerToPlay.Brain.IsThinking)
            {
                this.PlayerToPlay.Brain.ForceImmediateMove();
                this.UndoMove();
            }
        }

        /// <summary> Terminate the game. </summary>
        public void TerminateGame()
        {
            // WinBoard.StopListener();

            this.SuspendPondering();
            this.PlayerWhite.Brain.AbortThinking();
            this.PlayerBlack.Brain.AbortThinking();

            #region Commented out: Save Settings to Registry

            //RegistryKey registryKeySoftware = Registry.CurrentUser.OpenSubKey("Software", true);
            //if (registryKeySoftware != null)
            //{
            //    RegistryKey registryKeySharpChess = registryKeySoftware.CreateSubKey(@"PeterHughes.org\SharpChess");

            //    if (registryKeySharpChess != null)
            //    {
            //        registryKeySharpChess.SetValue("FileName", saveGameFileName);
            //        registryKeySharpChess.SetValue("ShowThinking", ShowThinking ? "1" : "0");
            //    }
            //}

            #endregion Save Settings to Registry
        }

        /// <summary>
        ///   Instruct the computer to begin thinking, and take its turn.
        /// </summary>
        public void Think()
        {
            this.SuspendPondering();
            this.MakeNextComputerMove();
        }

        /// <summary> Toggle edit mode. </summary>
        public void ToggleEditMode() => this.EditModeActive = !this.EditModeActive;

        /// <summary> Undo all moves. </summary>
        public void UndoAllMoves()
        {
            this.SuspendPondering();
            this.UndoAllMovesInternal();
            this.SaveBackup();
            this.SendBoardPositionChangeEvent();
            this.ResumePondering();
        }

        /// <summary> Undo the last move. </summary>
        public void UndoMove()
        {
            this.SuspendPondering();
            this.UndoMoveInternal();
            this.SaveBackup();
            this.SendBoardPositionChangeEvent();
            this.ResumePondering();
        }

        /// <summary> Add a move node to the save game XML document. </summary>
        /// <param name="xmldoc"> Xml document representing the save game file. </param>
        /// <param name="xmlnodeGame"> Parent game xmlnode. </param>
        /// <param name="move"> Move to append to the save game Xml document. </param>
        private static void AddSaveGameNode(XmlDocument xmldoc, XmlElement xmlnodeGame, Move move)
        {
            XmlElement xmlnodeMove = xmldoc.CreateElement("Move");
            xmlnodeGame.AppendChild(xmlnodeMove);
            xmlnodeMove.SetAttribute("MoveNo", move.MoveNo.ToString(CultureInfo.InvariantCulture));
            xmlnodeMove.SetAttribute("Name", move.Name.ToString());
            xmlnodeMove.SetAttribute("From", move.From.Name);
            xmlnodeMove.SetAttribute("To", move.To.Name);
            xmlnodeMove.SetAttribute("SecondsElapsed", Convert.ToInt32(move.TimeStamp.TotalSeconds).ToString(CultureInfo.InvariantCulture));
        }

        /// <summary> Start then next move automatically, if it is the computers turn. </summary>
        private void CheckIfAutoNextMove()
        {
            if (this.PlayerWhite.Intellegence == Player.PlayerIntellegenceNames.Computer
                && this.PlayerBlack.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                // Dont want an infinite loop of Computer moves
                return;
            }

            if (this.PlayerToPlay.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                if (this.PlayerToPlay.CanMove)
                {
                    this.MakeNextComputerMove();
                }
            }
        }

        /// <summary> Enable or disable SharpChess's features </summary>
        private void EnableFeatures()
        {
            this.EnableExtensions = true;
            this.EnableHistoryHeuristic = true;
            this.EnableKillerMoves = true;
            this.EnableNullMovePruning = true;
            this.EnablePvsSearch = true;
            this.EnableQuiescense = true;
            this.EnableReductions = true;
            this.EnableReductionFutilityFixedDepth = true;
            this.EnableReductionLateMove = true;
            this.EnableTranspositionTable = true;

            this.EnableAspiration = false;
            this.EnableReductionFutilityMargin = false;
        }

        /// <summary> Load game from the specified file name. </summary>
        /// <param name="strFileName"> The file name. </param>
        /// <returns> True if load was successful. </returns>
        private bool LoadGame(string strFileName)
        {
            this.MoveRedoList.Clear();
            var xmldoc = new XmlDocument();
            try
            {
                xmldoc.Load(strFileName);
            }
            catch
            {
                return false;
            }

            XmlNode? xmlGame = xmldoc.SelectSingleNode("/Game");
            if ( xmlGame is not XmlElement xmlnodeGame )
            {
                return false;
            }

            if (xmlnodeGame.GetAttribute("FEN") != string.Empty)
            {
                this.NewInternal(xmlnodeGame.GetAttribute("FEN"));
            }

            if (xmlnodeGame.GetAttribute("WhitePlayer") != string.Empty)
            {
                this.PlayerWhite.Intellegence = 
                    xmlnodeGame.GetAttribute("WhitePlayer") == "Human"
                        ? Player.PlayerIntellegenceNames.Human
                        : Player.PlayerIntellegenceNames.Computer;
            }

            if (xmlnodeGame.GetAttribute("BlackPlayer") != string.Empty)
            {
                this.PlayerBlack.Intellegence = 
                    xmlnodeGame.GetAttribute("BlackPlayer") == "Human"
                        ? Player.PlayerIntellegenceNames.Human
                        : Player.PlayerIntellegenceNames.Computer;
            }

            if (xmlnodeGame.GetAttribute("BoardOrientation") != string.Empty)
            {
                this.Board.Orientation = 
                    xmlnodeGame.GetAttribute("BoardOrientation") == "White"
                        ? Board.OrientationNames.White
                        : Board.OrientationNames.Black;
            }

            if (xmlnodeGame.GetAttribute("DifficultyLevel") != string.Empty)
            {
                this.DifficultyLevel = int.Parse(xmlnodeGame.GetAttribute("DifficultyLevel"));
            }

            if (xmlnodeGame.GetAttribute("ClockMoves") != string.Empty)
            {
                this.ClockMaxMoves = int.Parse(xmlnodeGame.GetAttribute("ClockMoves"));
            }

            if (xmlnodeGame.GetAttribute("ClockMinutes") != string.Empty)
            {
                this.ClockTime = new TimeSpan(0, int.Parse(xmlnodeGame.GetAttribute("ClockMinutes")), 0);
            }

            if (xmlnodeGame.GetAttribute("ClockSeconds") != string.Empty)
            {
                this.ClockTime = new TimeSpan(0, 0, int.Parse(xmlnodeGame.GetAttribute("ClockSeconds")));
            }

            if (xmlnodeGame.GetAttribute("MaximumSearchDepth") != string.Empty)
            {
                this.MaximumSearchDepth = int.Parse(xmlnodeGame.GetAttribute("MaximumSearchDepth"));
            }

            if (xmlnodeGame.GetAttribute("Pondering") != string.Empty)
            {
                this.EnablePondering = xmlnodeGame.GetAttribute("Pondering") == "1";
            }

            if (xmlnodeGame.GetAttribute("UseRandomOpeningMoves") != string.Empty)
            {
                this.UseRandomOpeningMoves = xmlnodeGame.GetAttribute("UseRandomOpeningMoves") == "1";
            }

            XmlNodeList? xmlnodelist = xmldoc.SelectNodes("/Game/Move");
            if (xmlnodelist != null)
            {
                foreach (XmlElement xmlnode in xmlnodelist)
                {
                    Square? from;
                    Square? to;
                    if (xmlnode.GetAttribute("FromFile") != string.Empty)
                    {
                        from = this.Board.GetSquare(
                            Convert.ToInt32(xmlnode.GetAttribute("FromFile")),
                            Convert.ToInt32(xmlnode.GetAttribute("FromRank")));
                        to = this.Board.GetSquare(
                            Convert.ToInt32(xmlnode.GetAttribute("ToFile")),
                            Convert.ToInt32(xmlnode.GetAttribute("ToRank")));
                    }
                    else
                    {
                        from = this.Board.GetSquare(xmlnode.GetAttribute("From"));
                        to = this.Board.GetSquare(xmlnode.GetAttribute("To"));
                    }

                    if ( from is null || to is null || from.Piece is null)
                    {
                        // Invalid move squares or no piece on from square
                        return false;
                    }

                    this.MakeAMoveInternal(Move.MoveNameFromString(xmlnode.GetAttribute("Name")), from.Piece, to);
                    TimeSpan tsnTimeStamp;
                    if (xmlnode.GetAttribute("SecondsElapsed") == string.Empty)
                    {
                        if (this.MoveHistory.Count <= 2)
                        {
                            tsnTimeStamp = new TimeSpan(0);
                        }
                        else
                        {
                            tsnTimeStamp = this.MoveHistory.PenultimateForSameSide.TimeStamp + (new TimeSpan(0, 0, 30));
                        }
                    }
                    else
                    {
                        tsnTimeStamp = new TimeSpan(0, 0, int.Parse(xmlnode.GetAttribute("SecondsElapsed")));
                    }

                    this.MoveHistory.Last.TimeStamp = tsnTimeStamp;
                    this.MoveHistory.Last.Piece.Player.Clock.TimeElapsed = tsnTimeStamp;
                }

                int intTurnNo = 
                    xmlnodeGame.GetAttribute("TurnNo") != string.Empty
                        ? int.Parse(xmlnodeGame.GetAttribute("TurnNo"))
                        : xmlnodelist.Count;

                for (int intIndex = xmlnodelist.Count; intIndex > intTurnNo; intIndex--)
                {
                    this.UndoMoveInternal();
                }
            }

            return true;
        }

        /// <summary> Make the specified move. For internal use only. </summary>
        /// <param name="moveName"> The move name. </param>
        /// <param name="piece"> The piece to move. </param>
        /// <param name="square"> The square to move to. </param>
        private void MakeAMoveInternal(Move.MoveNames moveName, Piece piece, Square square)
        {
            this.MoveRedoList.Clear();
            Move move = piece.Move(moveName, square);
            move.EnemyStatus = move.Piece.Player.OpposingPlayer.Status;
            this.PlayerToPlay.Clock.Stop();
            this.MoveHistory.Last.TimeStamp = this.PlayerToPlay.Clock.TimeElapsed;
            if (this.PlayerToPlay.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                // WinBoard.SendMove(move);
                //if (!PlayerToPlay.OpposingPlayer.CanMove)
                //{
                //    if (PlayerToPlay.OpposingPlayer.IsInCheckMate)
                //    {
                //        WinBoard.SendCheckMate();
                //    }
                //    else if (!PlayerToPlay.OpposingPlayer.IsInCheck)
                //    {
                //        WinBoard.SendCheckStaleMate();
                //    }
                //}
                //else if (PlayerToPlay.OpposingPlayer.CanClaimThreeMoveRepetitionDraw)
                //{
                //    WinBoard.SendDrawByRepetition();
                //}
                //else if (PlayerToPlay.OpposingPlayer.CanClaimFiftyMoveDraw)
                //{
                //    WinBoard.SendDrawByFiftyMoveRule();
                //}
                //else if (PlayerToPlay.OpposingPlayer.CanClaimInsufficientMaterialDraw)
                //{
                //    WinBoard.SendDrawByInsufficientMaterial();
                //}
            }

            this.PlayerToPlay = this.PlayerToPlay.OpposingPlayer;
            this.PlayerToPlay.Clock.Start();
        }

        /// <summary> Instruct the computer to make its next move. </summary>
        private void MakeNextComputerMove()
        {
            if (this.PlayerToPlay.CanMove)
            {
                this.PlayerToPlay.Brain.StartThinking();
            }
        }

        /// <summary> Start a new game. For internal use only. </summary>
        private void NewInternal() => this.NewInternal(string.Empty);

        /// <summary> Start a new game from the specified FEN string position. For internal use only. </summary>
        /// <param name="fenString"> The str fen. </param>
        public void NewInternal(string fenString)
        {
            if (string.IsNullOrWhiteSpace(fenString))
            {
                fenString = Fen.GameStartPosition;
            }

            Fen.Validate(fenString);

            this.HashTable.Clear();
            this.HashTablePawn.Clear();
            this.HashTableCheck.Clear();
            this.KillerMoves.Clear();
            this.HistoryHeuristic.Clear();

            this.UndoAllMovesInternal();
            this.MoveRedoList.Clear();
            saveGameFileName = string.Empty;
            Fen.SetBoardPosition(this, fenString);
            this.PlayerWhite.Clock.Reset();
            this.PlayerBlack.Clock.Reset();
        }

        /// <summary> Called when the computer has finished thinking, and is ready to make its move. </summary>
        /// <exception cref="ApplicationException">Raised when principal variation is empty.</exception>
        private void PlayerReadyToMakeMove()
        {
            Move? move;
            if (this.PlayerToPlay.Brain.PrincipalVariation.Count > 0)
            {
                move = this.PlayerToPlay.Brain.PrincipalVariation[0];
                if ( move is null)
                {
                    throw new ApplicationException("Player_ReadToMakeMove: Principal Variation first move is null.");
                }
            }
            else
            {
                throw new ApplicationException("Player_ReadToMakeMove: Principal Variation is empty.");
            }

            this.MakeAMoveInternal(move.Name, move.Piece, move.To);
            this.SaveBackup();
            this.SendBoardPositionChangeEvent();
            this.ResumePondering();
        }

        /// <summary> Redo move. For internal use only. </summary>
        private void RedoMoveInternal()
        {
            if (this.MoveRedoList.Count > 0)
            {
                Move? moveRedo = 
                    this.MoveRedoList[^1] ?? 
                    throw new ApplicationException("RedoMoveInternal: MoveRedoList last move is null.");
                this.PlayerToPlay.Clock.Revert();
                moveRedo.Piece.Move(moveRedo.Name, moveRedo.To);
                this.PlayerToPlay.Clock.TimeElapsed = moveRedo.TimeStamp;
                Move? last = 
                    this.MoveHistory.Last ??
                    throw new ApplicationException("RedoMoveInternal: MoveRedoList last move is null.");
                
                last.TimeStamp = moveRedo.TimeStamp;
                last.EnemyStatus = moveRedo.Piece.Player.OpposingPlayer.Status; // 14Mar05 Nimzo
                this.PlayerToPlay = this.PlayerToPlay.OpposingPlayer;
                this.MoveRedoList.RemoveLast();
                if (!this.IsPaused)
                {
                    this.PlayerToPlay.Clock.Start();
                }
            }
        }

        /// <summary> Save a backup of the current game. </summary>
        private void SaveBackup()
        {
            //if (!WinBoard.Active)
            {
                // Only save backups if not using WinBoard.
                this.SaveGame(this.BackupGamePath);
            }
        }

        /// <summary> Save game using the specified file name. </summary>
        /// <param name="fileName"> The file name. </param>
        private void SaveGame(string fileName)
        {
            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            string assemblyVersionString = assemblyVersion != null ? assemblyVersion.ToString() : "1.0";
            var xmldoc = new XmlDocument();
            XmlElement xmlnodeGame = xmldoc.CreateElement("Game");

            xmldoc.AppendChild(xmlnodeGame);

            xmlnodeGame.SetAttribute("FEN", this.FenStartPosition == Fen.GameStartPosition ? string.Empty : this.FenStartPosition);
            xmlnodeGame.SetAttribute("TurnNo", this.TurnNo.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute(
                "WhitePlayer", this.PlayerWhite.Intellegence == Player.PlayerIntellegenceNames.Human ? "Human" : "Computer");
            xmlnodeGame.SetAttribute(
                "BlackPlayer", this.PlayerBlack.Intellegence == Player.PlayerIntellegenceNames.Human ? "Human" : "Computer");
            xmlnodeGame.SetAttribute(
                "BoardOrientation", this.Board.Orientation == Board.OrientationNames.White ? "White" : "Black");
            xmlnodeGame.SetAttribute("Version", assemblyVersionString);
            xmlnodeGame.SetAttribute("DifficultyLevel", this.DifficultyLevel.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("ClockMoves", this.ClockMaxMoves.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("ClockSeconds", this.ClockTime.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("MaximumSearchDepth", this.MaximumSearchDepth.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("Pondering", this.EnablePondering ? "1" : "0");
            xmlnodeGame.SetAttribute("UseRandomOpeningMoves", this.UseRandomOpeningMoves ? "1" : "0");

            foreach (Move move in this.MoveHistory)
            {
                Game.AddSaveGameNode(xmldoc, xmlnodeGame, move);
            }

            // Redo moves
            for (int intIndex = this.MoveRedoList.Count - 1; intIndex >= 0; intIndex--)
            {
                var move = this.MoveRedoList[intIndex];
                if (move is not null)
                {
                    Game.AddSaveGameNode(xmldoc, xmlnodeGame, move);
                }
            }

            xmldoc.Save(fileName);
        }

        /// <summary> Raises the send board position change event. </summary>
        private void SendBoardPositionChangeEvent() => this.BoardPositionChanged();

        /// <summary> Undo all moves. For internal use only. </summary>
        private void UndoAllMovesInternal()
        {
            while (this.MoveHistory.Count > 0)
            {
                this.UndoMoveInternal();
            }
        }

        /// <summary> Undo move. For internal use only. </summary>
        private void UndoMoveInternal()
        {
            if (this.MoveHistory.Count > 0)
            {
                Move? moveUndo =
                    this.MoveHistory.Last ?? 
                    throw new ApplicationException("UndoMoveInternal: MoveHistory last move is null.");
                this.PlayerToPlay.Clock.Revert();
                this.MoveRedoList.Add(moveUndo);
                moveUndo.Undo(moveUndo);
                this.PlayerToPlay = this.PlayerToPlay.OpposingPlayer;
                if (this.MoveHistory.Count > 1)
                {
                    Move? movePenultimate =
                        this.MoveHistory[^2] ?? 
                        throw new ApplicationException("UndoMoveInternal: MoveHistory penultimate move is null.");
                    this.PlayerToPlay.Clock.TimeElapsed = movePenultimate.TimeStamp;
                }
                else
                {
                    this.PlayerToPlay.Clock.TimeElapsed = new TimeSpan(0);
                }

                if (!this.IsPaused)
                {
                    this.PlayerToPlay.Clock.Start();
                }
            }
        }
    }
}