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

        public readonly Board Board;
        public readonly Fen Fen;

        /// <summary> The file name.
        private string saveGameFileName = string.Empty;

        /// <summary> Initializes members of the <see cref="Game" /> class. </summary>
        public Game(Board board, Fen Fen)
        {
            this.Board = board;
            this.Fen = Fen;

            EnableFeatures();
            ClockIncrementPerMove = new TimeSpan(0, 0, 0);
            ClockFixedTimePerMove = new TimeSpan(0, 0, 0);
            DifficultyLevel = 1;
            ClockTime = new TimeSpan(0, 5, 0);
            ClockMaxMoves = 40;
            UseRandomOpeningMoves = true;
            MoveRedoList = [];
            MaximumSearchDepth = 1;
            MoveAnalysis = [];
            MoveHistory = [];
            FenStartPosition = string.Empty;
            HashTable.Initialise();
            HashTablePawn.Initialise();
            HashTableCheck.Initialise();

            PlayerWhite = new PlayerWhite(this);
            PlayerBlack = new PlayerBlack(this);
            PlayerToPlay = PlayerWhite;
            Board.EstablishHashKey();
            OpeningBookSimple.Initialise();

            PlayerWhite.Brain.ReadyToMakeMoveEvent += PlayerReadyToMakeMove;
            PlayerBlack.Brain.ReadyToMakeMoveEvent += PlayerReadyToMakeMove;

            BackupGamePath = string.Empty;

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
        public bool IsPaused => !PlayerToPlay.Clock.IsTicking;

        /// <summary> Gets the lowest material count for black or white. </summary>
        public int LowestMaterialCount
        {
            get
            {
                int intWhiteMaterialCount = PlayerWhite.MaterialCount;
                int intBlackMaterialCount = PlayerBlack.MaterialCount;
                return intWhiteMaterialCount < intBlackMaterialCount ? intWhiteMaterialCount : intBlackMaterialCount;
            }
        }

        /// <summary> Gets the largest valid Material Count. </summary>
        public int MaxMaterialCount => 7;

        /// <summary> Gets or sets the maximum search depth. </summary>
        public int MaximumSearchDepth { get; set; }

        /// <summary> Gets or sets the list of move-analysis moves. </summary>
        public Moves MoveAnalysis { get; set; }

        /// <summary> Gets the current move history. </summary>
        public Moves MoveHistory { get; private set; }

        /// <summary> Gets the current move number. </summary>
        public int MoveNo =>  TurnNo >> 1;

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
                if (LowestMaterialCount >= MaxMaterialCount)
                {
                    return GameStageNames.Opening;
                }

                return LowestMaterialCount <= 3 ? GameStageNames.End : GameStageNames.Middle;
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
            PlayerWhite.CaptureAllPieces();
            PlayerBlack.CaptureAllPieces();
        }

        /// <summary> Demotes all pieces. </summary>
        public void DemoteAllPieces()
        {
            PlayerWhite.DemoteAllPieces();
            PlayerBlack.DemoteAllPieces();
        }

        /// <summary> Load a saved game. </summary>
        /// <param name="fileName"> File name. </param>
        /// <returns> Returns True is game loaded successfully. </returns>
        public bool Load(string fileName)
        {
            SuspendPondering();

            NewInternal();
            saveGameFileName = fileName;
            bool blnSuccess = LoadGame(fileName);
            if (blnSuccess)
            {
                SaveBackup();
                SendBoardPositionChangeEvent();
            }

            PausePlay();

            return blnSuccess;
        }

        /// <summary> Load backup game. </summary>
        /// <returns> Returns True is game loaded successfully. </returns>
        public bool LoadBackup() =>  LoadGame(BackupGamePath);

        /// <summary> Make a move.</summary>
        /// <param name="moveName"> The move name. </param>
        /// <param name="piece"> The piece to move. </param>
        /// <param name="square"> The square to move to. </param>
        public void MakeAMove(Move.MoveNames moveName, Piece piece, Square square)
        {
            SuspendPondering();
            MakeAMoveInternal(moveName, piece, square);
            SaveBackup();
            SendBoardPositionChangeEvent();
            CheckIfAutoNextMove();
        }

        /// <summary> Start a new game. </summary>
        public void New() =>  New(string.Empty);

        /// <summary> Start a new game using a FEN string. </summary>
        /// <param name="fenString"> The FEN string. </param>
        public void New(string fenString)
        {
            SuspendPondering();
            NewInternal(fenString);
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary> Pause the game. </summary>
        public void PausePlay()
        {
            PlayerToPlay.Clock.Stop();
            PlayerToPlay.Brain.ForceImmediateMove();
            GamePaused();
        }

        /// <summary> Redo all moves. </summary>
        public void RedoAllMoves()
        {
            SuspendPondering();
            while (MoveRedoList.Count > 0)
            {
                RedoMoveInternal();
            }

            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary> Redo a move. </summary>
        public void RedoMove()
        {
            SuspendPondering();
            RedoMoveInternal();
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary> Resume the game. </summary>
        public void ResumePlay()
        {
            PlayerToPlay.Clock.Start();
            GameResumed();
            if (PlayerToPlay.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                MakeNextComputerMove();
            }
            else
            {
                ResumePondering();
            }
        }

        /// <summary> Resume pondering. </summary>
        public void ResumePondering()
        {
            if (IsPaused)
            {
                return;
            }

            if (!EnablePondering)
            {
                return;
            }

            if (!PlayerToPlay.CanMove)
            {
                return;
            }

            if (PlayerWhite.Intellegence == Player.PlayerIntellegenceNames.Computer
                && PlayerBlack.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                return;
            }

            if (PlayerToPlay.OpposingPlayer.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                if (!PlayerToPlay.Brain.IsPondering)
                {
                    PlayerToPlay.Brain.StartPondering();
                }
            }
        }

        /// <summary> Save the game as a file name. </summary>
        /// <param name="fileName"> The file name. </param>
        public void Save(string fileName)
        {
            SuspendPondering();

            SaveBackup();
            SaveGame(fileName);
            saveGameFileName = fileName;

            GameSaved();

            ResumePondering();
        }

        /// <summary> Called when settings have been changed in the UI. </summary>
        public void SettingsUpdate()
        {
            SuspendPondering();
            //if (!WinBoard.Active)
            //{
            //    SaveBackup();
            //}

            SettingsUpdated();
            ResumePondering();
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
            if (PlayerToPlay.Brain.IsPondering)
            {
                PlayerToPlay.Brain.ForceImmediateMove();
            }
            else if (PlayerToPlay.Brain.IsThinking)
            {
                PlayerToPlay.Brain.ForceImmediateMove();
                UndoMove();
            }
        }

        /// <summary> Terminate the game. </summary>
        public void TerminateGame()
        {
            // WinBoard.StopListener();

            SuspendPondering();
            PlayerWhite.Brain.AbortThinking();
            PlayerBlack.Brain.AbortThinking();

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
            SuspendPondering();
            MakeNextComputerMove();
        }

        /// <summary> Toggle edit mode. </summary>
        public void ToggleEditMode()
        {
            EditModeActive = !EditModeActive;
        }

        /// <summary> Undo all moves. </summary>
        public void UndoAllMoves()
        {
            SuspendPondering();
            UndoAllMovesInternal();
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary> Undo the last move. </summary>
        public void UndoMove()
        {
            SuspendPondering();
            UndoMoveInternal();
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary> Add a move node to the save game XML document. </summary>
        /// <param name="xmldoc"> Xml document representing the save game file. </param>
        /// <param name="xmlnodeGame"> Parent game xmlnode. </param>
        /// <param name="move"> Move to append to the save game Xml document. </param>
        private void AddSaveGameNode(XmlDocument xmldoc, XmlElement xmlnodeGame, Move move)
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
            if (PlayerWhite.Intellegence == Player.PlayerIntellegenceNames.Computer
                && PlayerBlack.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                // Dont want an infinate loop of Computer moves
                return;
            }

            if (PlayerToPlay.Intellegence == Player.PlayerIntellegenceNames.Computer)
            {
                if (PlayerToPlay.CanMove)
                {
                    MakeNextComputerMove();
                }
            }
        }

        /// <summary> Enable or disable SharpChess's features </summary>
        private void EnableFeatures()
        {
            EnableAspiration = false;
            EnableExtensions = true;
            EnableHistoryHeuristic = true;
            EnableKillerMoves = true;
            EnableNullMovePruning = true;
            EnablePvsSearch = true;
            EnableQuiescense = true;
            EnableReductions = true;
            EnableReductionFutilityMargin = false;
            EnableReductionFutilityFixedDepth = true;
            EnableReductionLateMove = true;
            EnableTranspositionTable = true;
        }

        /// <summary> Load game from the specified file name. </summary>
        /// <param name="strFileName"> The file name. </param>
        /// <returns> True if load was successful. </returns>
        private bool LoadGame(string strFileName)
        {
            MoveRedoList.Clear();
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
                NewInternal(xmlnodeGame.GetAttribute("FEN"));
            }

            if (xmlnodeGame.GetAttribute("WhitePlayer") != string.Empty)
            {
                PlayerWhite.Intellegence = xmlnodeGame.GetAttribute("WhitePlayer") == "Human"
                                               ? Player.PlayerIntellegenceNames.Human
                                               : Player.PlayerIntellegenceNames.Computer;
            }

            if (xmlnodeGame.GetAttribute("BlackPlayer") != string.Empty)
            {
                PlayerBlack.Intellegence = xmlnodeGame.GetAttribute("BlackPlayer") == "Human"
                                               ? Player.PlayerIntellegenceNames.Human
                                               : Player.PlayerIntellegenceNames.Computer;
            }

            if (xmlnodeGame.GetAttribute("BoardOrientation") != string.Empty)
            {
                Board.Orientation = xmlnodeGame.GetAttribute("BoardOrientation") == "White"
                                        ? Board.OrientationNames.White
                                        : Board.OrientationNames.Black;
            }

            if (xmlnodeGame.GetAttribute("DifficultyLevel") != string.Empty)
            {
                DifficultyLevel = int.Parse(xmlnodeGame.GetAttribute("DifficultyLevel"));
            }

            if (xmlnodeGame.GetAttribute("ClockMoves") != string.Empty)
            {
                ClockMaxMoves = int.Parse(xmlnodeGame.GetAttribute("ClockMoves"));
            }

            if (xmlnodeGame.GetAttribute("ClockMinutes") != string.Empty)
            {
                ClockTime = new TimeSpan(0, int.Parse(xmlnodeGame.GetAttribute("ClockMinutes")), 0);
            }

            if (xmlnodeGame.GetAttribute("ClockSeconds") != string.Empty)
            {
                ClockTime = new TimeSpan(0, 0, int.Parse(xmlnodeGame.GetAttribute("ClockSeconds")));
            }

            if (xmlnodeGame.GetAttribute("MaximumSearchDepth") != string.Empty)
            {
                MaximumSearchDepth = int.Parse(xmlnodeGame.GetAttribute("MaximumSearchDepth"));
            }

            if (xmlnodeGame.GetAttribute("Pondering") != string.Empty)
            {
                EnablePondering = xmlnodeGame.GetAttribute("Pondering") == "1";
            }

            if (xmlnodeGame.GetAttribute("UseRandomOpeningMoves") != string.Empty)
            {
                UseRandomOpeningMoves = xmlnodeGame.GetAttribute("UseRandomOpeningMoves") == "1";
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
                        from = Board.GetSquare(
                            Convert.ToInt32(xmlnode.GetAttribute("FromFile")),
                            Convert.ToInt32(xmlnode.GetAttribute("FromRank")));
                        to = Board.GetSquare(
                            Convert.ToInt32(xmlnode.GetAttribute("ToFile")),
                            Convert.ToInt32(xmlnode.GetAttribute("ToRank")));
                    }
                    else
                    {
                        from = Board.GetSquare(xmlnode.GetAttribute("From"));
                        to = Board.GetSquare(xmlnode.GetAttribute("To"));
                    }

                    if ( from is null || to is null || from.Piece is null)
                    {
                        // Invalid move squares or no piece on from square
                        return false;
                    }

                    MakeAMoveInternal(Move.MoveNameFromString(xmlnode.GetAttribute("Name")), from.Piece, to);
                    TimeSpan tsnTimeStamp;
                    if (xmlnode.GetAttribute("SecondsElapsed") == string.Empty)
                    {
                        if (MoveHistory.Count <= 2)
                        {
                            tsnTimeStamp = new TimeSpan(0);
                        }
                        else
                        {
                            tsnTimeStamp = MoveHistory.PenultimateForSameSide.TimeStamp + (new TimeSpan(0, 0, 30));
                        }
                    }
                    else
                    {
                        tsnTimeStamp = new TimeSpan(0, 0, int.Parse(xmlnode.GetAttribute("SecondsElapsed")));
                    }

                    MoveHistory.Last.TimeStamp = tsnTimeStamp;
                    MoveHistory.Last.Piece.Player.Clock.TimeElapsed = tsnTimeStamp;
                }

                int intTurnNo = xmlnodeGame.GetAttribute("TurnNo") != string.Empty
                                    ? int.Parse(xmlnodeGame.GetAttribute("TurnNo"))
                                    : xmlnodelist.Count;

                for (int intIndex = xmlnodelist.Count; intIndex > intTurnNo; intIndex--)
                {
                    UndoMoveInternal();
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
            MoveRedoList.Clear();
            Move move = piece.Move(moveName, square);
            move.EnemyStatus = move.Piece.Player.OpposingPlayer.Status;
            PlayerToPlay.Clock.Stop();
            MoveHistory.Last.TimeStamp = PlayerToPlay.Clock.TimeElapsed;
            if (PlayerToPlay.Intellegence == Player.PlayerIntellegenceNames.Computer)
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

            PlayerToPlay = PlayerToPlay.OpposingPlayer;
            PlayerToPlay.Clock.Start();
        }

        /// <summary> Instruct the computer to make its next move. </summary>
        private void MakeNextComputerMove()
        {
            if (PlayerToPlay.CanMove)
            {
                PlayerToPlay.Brain.StartThinking();
            }
        }

        /// <summary> Start a new game. For internal use only. </summary>
        private void NewInternal() => NewInternal(string.Empty);

        /// <summary> Start a new game from the specified FEN string position. For internal use only. </summary>
        /// <param name="fenString"> The str fen. </param>
        private void NewInternal(string fenString)
        {
            if (string.IsNullOrWhiteSpace(fenString))
            {
                fenString = Fen.GameStartPosition;
            }

            Fen.Validate(fenString);

            HashTable.Clear();
            HashTablePawn.Clear();
            HashTableCheck.Clear();
            KillerMoves.Clear();
            HistoryHeuristic.Clear();

            UndoAllMovesInternal();
            MoveRedoList.Clear();
            saveGameFileName = string.Empty;
            Fen.SetBoardPosition(fenString);
            PlayerWhite.Clock.Reset();
            PlayerBlack.Clock.Reset();
        }

        /// <summary> Called when the computer has finished thinking, and is ready to make its move. </summary>
        /// <exception cref="ApplicationException">Raised when principal variation is empty.</exception>
        private void PlayerReadyToMakeMove()
        {
            Move? move;
            if (PlayerToPlay.Brain.PrincipalVariation.Count > 0)
            {
                move = PlayerToPlay.Brain.PrincipalVariation[0];
                if ( move is null)
                {
                    throw new ApplicationException("Player_ReadToMakeMove: Principal Variation first move is null.");
                }
            }
            else
            {
                throw new ApplicationException("Player_ReadToMakeMove: Principal Variation is empty.");
            }

            MakeAMoveInternal(move.Name, move.Piece, move.To);
            SaveBackup();
            SendBoardPositionChangeEvent();
            ResumePondering();
        }

        /// <summary> Redo move. For internal use only. </summary>
        private void RedoMoveInternal()
        {
            if (MoveRedoList.Count > 0)
            {
                Move? moveRedo = MoveRedoList[MoveRedoList.Count - 1];
                if ( moveRedo is null)
                {
                    throw new ApplicationException("RedoMoveInternal: MoveRedoList last move is null.");
                }

                PlayerToPlay.Clock.Revert();
                moveRedo.Piece.Move(moveRedo.Name, moveRedo.To);
                PlayerToPlay.Clock.TimeElapsed = moveRedo.TimeStamp;
                Move? last = MoveHistory.Last;
                if ( last is null)
                {
                    throw new ApplicationException("RedoMoveInternal: MoveRedoList last move is null.");
                }
                
                last.TimeStamp = moveRedo.TimeStamp;
                last.EnemyStatus = moveRedo.Piece.Player.OpposingPlayer.Status; // 14Mar05 Nimzo
                PlayerToPlay = PlayerToPlay.OpposingPlayer;
                MoveRedoList.RemoveLast();
                if (!IsPaused)
                {
                    PlayerToPlay.Clock.Start();
                }
            }
        }

        /// <summary> Save a backup of the current game. </summary>
        private void SaveBackup()
        {
            //if (!WinBoard.Active)
            {
                // Only save backups if not using WinBoard.
                SaveGame(BackupGamePath);
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

            xmlnodeGame.SetAttribute("FEN", FenStartPosition == Fen.GameStartPosition ? string.Empty : FenStartPosition);
            xmlnodeGame.SetAttribute("TurnNo", TurnNo.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute(
                "WhitePlayer", PlayerWhite.Intellegence == Player.PlayerIntellegenceNames.Human ? "Human" : "Computer");
            xmlnodeGame.SetAttribute(
                "BlackPlayer", PlayerBlack.Intellegence == Player.PlayerIntellegenceNames.Human ? "Human" : "Computer");
            xmlnodeGame.SetAttribute(
                "BoardOrientation", Board.Orientation == Board.OrientationNames.White ? "White" : "Black");
            xmlnodeGame.SetAttribute("Version", assemblyVersionString);
            xmlnodeGame.SetAttribute("DifficultyLevel", DifficultyLevel.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("ClockMoves", ClockMaxMoves.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("ClockSeconds", ClockTime.TotalSeconds.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("MaximumSearchDepth", MaximumSearchDepth.ToString(CultureInfo.InvariantCulture));
            xmlnodeGame.SetAttribute("Pondering", EnablePondering ? "1" : "0");
            xmlnodeGame.SetAttribute("UseRandomOpeningMoves", UseRandomOpeningMoves ? "1" : "0");

            foreach (Move move in MoveHistory)
            {
                AddSaveGameNode(xmldoc, xmlnodeGame, move);
            }

            // Redo moves
            for (int intIndex = MoveRedoList.Count - 1; intIndex >= 0; intIndex--)
            {
                var move = MoveRedoList[intIndex];
                if (move is not null)
                {
                    AddSaveGameNode(xmldoc, xmlnodeGame, move);
                }
            }

            xmldoc.Save(fileName);
        }

        /// <summary> Raises the send board position change event. </summary>
        private void SendBoardPositionChangeEvent()
        {
            BoardPositionChanged();
        }

        /// <summary> Undo all moves. For internal use only. </summary>
        private void UndoAllMovesInternal()
        {
            while (MoveHistory.Count > 0)
            {
                UndoMoveInternal();
            }
        }

        /// <summary> Undo move. For internal use only. </summary>
        private void UndoMoveInternal()
        {
            if (MoveHistory.Count > 0)
            {
                Move? moveUndo = 
                    MoveHistory.Last ?? 
                    throw new ApplicationException("UndoMoveInternal: MoveHistory last move is null.");
                PlayerToPlay.Clock.Revert();
                MoveRedoList.Add(moveUndo);
                moveUndo.Undo(moveUndo);
                PlayerToPlay = PlayerToPlay.OpposingPlayer;
                if (MoveHistory.Count > 1)
                {
                    Move? movePenultimate = 
                        MoveHistory[MoveHistory.Count - 2] ?? 
                        throw new ApplicationException("UndoMoveInternal: MoveHistory penultimate move is null.");
                    PlayerToPlay.Clock.TimeElapsed = movePenultimate.TimeStamp;
                }
                else
                {
                    PlayerToPlay.Clock.TimeElapsed = new TimeSpan(0);
                }

                if (!IsPaused)
                {
                    PlayerToPlay.Clock.Start();
                }
            }
        }
    }
}