namespace Lyt.Chess.Workflow.Setup;

using static Lyt.Chess.Messaging.ToolbarCommandMessage;

public sealed partial class SetupViewModel :
    ViewModel<SetupView>,
    IRecipient<ToolbarCommandMessage>,
    IRecipient<LanguageChangedMessage>,
    IRecipient<ModelLoadedMessage>
{
    private const int DecodeToWidthThumbnail = 420;

    private enum PlayStatus
    {
        Unprepared,
        ReadyForRestart,
        ReadyForNew,
    }

    #region Observable Properties 

    [ObservableProperty]
    private ThumbnailsPanelViewModel thumbnailsPanelViewModel;

    [ObservableProperty]
    // Either copy of sourceImage and possibly cropped and with requested contrast applied
    private WriteableBitmap? puzzleImage;

    [ObservableProperty]
    private string pieceCountString;

    [ObservableProperty]

    private int pieceCountMin;

    [ObservableProperty]
    private int pieceCountMax;

    [ObservableProperty]
    private double pieceCountSliderValue;

    [ObservableProperty]
    private string rotationsString;

    [ObservableProperty]
    private double rotationsSliderValue;

    [ObservableProperty]
    private string snapString;

    [ObservableProperty]
    private double snapSliderValue;

    [ObservableProperty]
    private string contrastString;

    [ObservableProperty]
    private double contrastSliderValue;

    [ObservableProperty]
    private string hintsString;

    [ObservableProperty]
    private double hintsSliderValue;

    [ObservableProperty]
    private bool parametersVisible;

    [ObservableProperty]
    private bool parametersEnabled;

    [ObservableProperty]
    private double parametersOpacity;

    [ObservableProperty]
    private string howToPlay;

    #endregion Observable Properties 

    private readonly ChessModel chessModel;

    // NOT cropped and NO contrast applied
    private WriteableBitmap? sourceImage;

    private bool loaded;
    private PlayStatus state;
    private int pieceCount;
    private int rotations;
    private int snap;
    private int contrast;
    private int hints;
    private List<int> pieceCounts;
    private byte[]? imageBytes;

    public SetupViewModel(ChessModel chessModel)
    {
        this.chessModel = chessModel;
        this.ThumbnailsPanelViewModel = new ThumbnailsPanelViewModel(this, chessModel);
        this.state = PlayStatus.Unprepared;
        this.Subscribe<ToolbarCommandMessage>();
        this.Subscribe<LanguageChangedMessage>();
        this.Subscribe<ModelLoadedMessage>();

        // Initial UI Setup
        this.PieceCountString = string.Empty;
        this.RotationsString = string.Empty;
        this.SnapString = string.Empty;
        this.ContrastString = string.Empty;
        this.HintsString = string.Empty;
        this.HowToPlay = string.Empty;
        //"1. Drag and drop an image onto the drop area or select an image from your collection.\n" +
        //"2. Adjust the puzzle parameters as desired.\n" +
        //"3. Click 'Play' to start solving the puzzle!\n" +
        //"4. Enjoy your jigsaw puzzle experience!";
        this.ParametersVisible = false;
        this.ParametersEnabled = false;
        this.ParametersOpacity = 1.0;
        this.SetUiParametersToDefaults();
    }

    public override void Activate(object? activationParameters)
    {
        base.Activate(activationParameters);

        this.ThumbnailsPanelViewModel.LoadThumnails();
        this.loaded = true;
    }

    public void Receive(LanguageChangedMessage message)
    {
        this.LocalizeRotations();
        this.LocalizeSnap();
        this.LocalizeHints();
        this.LocalizeContrast();
    }

    public void Receive(ModelLoadedMessage _)
    {
        if (!this.loaded)
        {
            this.ThumbnailsPanelViewModel.LoadThumnails();
            this.loaded = true;
        }
    }

    public void Receive(ToolbarCommandMessage message)
    {
        switch (message.Command)
        {
            case ToolbarCommandMessage.ToolbarCommand.RemoveFromCollection:
                this.DeleteGame();
                break;

            // Ignore all other commands 
            default:
                break;
        }
    }

    private void DeleteGame()
    {
        var game = this.chessModel.GameInProgress;
        if (game is null)
        {
            return;
        }

        if (!this.chessModel.DeleteGame(game.Name, out string message))
        {
            Debug.WriteLine(message);
        }

        this.ThumbnailsPanelViewModel.LoadThumnails();
    }

    [RelayCommand]
    public void OnPlay() => this.Play();

    private void Play()
    {
        if (this.state == PlayStatus.ReadyForNew)
        {
            this.StartNewGame();
        }
        else if (this.state == PlayStatus.ReadyForRestart)
        {
            this.ResumeSavedGame();
        }
        // else: Not ready: do nothing 
    }

    private void ResumeSavedGame()
    {
        try
        {
            //if ((this.chessModel.Puzzle is not null) && (this.PuzzleImage is not null))
            //{
            //    var vm = App.GetRequiredService<PuzzleViewModel>();
            //    vm.ResumePuzzle(this.chessModel.Puzzle, this.PuzzleImage);
            //    ActivatePuzzleView();
            //}
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private void StartNewGame()
    {
        if ((this.PuzzleImage is null) ||
            (this.imageBytes == null) || (this.imageBytes.Length == 0) ||
            (this.pieceCount == 0))
        {
            return;
        }

        try
        {
            ////PuzzleImageSetup setup = this.setups[this.pieceCount];
            ////PuzzleParameters puzzleParameters = new()
            ////{
            ////    PieceCount = this.pieceCount,
            ////    Rows = setup.Rows,
            ////    Columns = setup.Columns,
            ////    RotationSteps = this.rotations,
            ////    Snap = this.snap,
            ////    Hints = this.hints,
            ////};
            ////var writeableBitmap =
            ////    WriteableBitmap.DecodeToWidth(new MemoryStream(this.imageBytes), DecodeToWidthThumbnail);
            ////byte[] thumbnailBytes = writeableBitmap.EncodeToJpeg();
            ////var vm = App.GetRequiredService<PuzzleViewModel>();
            //vm.StartNewGame(
            //    this.imageBytes, thumbnailBytes, this.PuzzleImage, setup, puzzleParameters);
            ActivateGameView();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.ToString());
        }
    }

    private static void ActivateGameView()
    {
        ViewSelector<ActivatedView>.Enable(ActivatedView.Play);
        ApplicationMessagingExtensions.Select(ActivatedView.Play);
    }

    internal void ClearSelection()
    {
        this.ParametersVisible = false;
        this.PuzzleImage = null;
        this.state = PlayStatus.Unprepared;
    }

    internal void Select(Model.GameObjects.Game game)
    {
        // Load the puzzle image regardless of completion status
        string gameKey = game.Name;
        byte[]? imageBytes = this.chessModel.LoadGame(gameKey);
        if (imageBytes is null)
        {
            return;
        }

        try
        {
        //    int decodeToWidth = 1920; //  1024 + 512;
        //    var image =
        //        WriteableBitmap.DecodeToWidth(
        //            new MemoryStream(imageBytes), decodeToWidth, BitmapInterpolationMode.HighQuality);
        //    this.PuzzleImage = image;
        //    this.imageBytes = imageBytes;
        //    this.ParametersVisible = game.IsCompleted;

        //    // Load game parameters into the UI: We need to know about possible setups
        //    // if we want to restart it
        //    var imageSize = image.PixelSize;
        //    this.setups = Puzzle.GenerateSetups(new IntSize(imageSize.Height, imageSize.Width));
        //    this.SetUiParametersFromGame(game.PuzzleParameters);

        //    if (game.IsCompleted)
        //    {
        //        // Completed game: allow to redo it with different parameters
        //        this.ParametersVisible = true;
        //        this.ParametersEnabled = true;
        //        this.ParametersOpacity = 1.0;
        //        this.HowToPlay = this.Localize("Collection.StartAgain");
        //        // Need to duplicate the image 
        //        this.sourceImage = image.Duplicate();

        //        // Show settings 
        //        this.SetupUiForNewGame();
        //    }
        //    else
        //    {
        //        // Game in progress : Show parameters but dont allow to change them 
        //        this.ParametersVisible = true;
        //        this.ParametersEnabled = false;
        //        this.ParametersOpacity = 0.5;
        //        this.HowToPlay = this.Localize("Collection.Continue");

        //        // No need to duplicate the image 
        //        this.state = PlayStatus.ReadyForRestart;
        //    }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    #region Game Parameters 

    private void SetUiParametersToDefaults()
    {
        this.PieceCountSliderValue = this.PieceCountMin;
        this.RotationsSliderValue = 0.0;
        this.SnapSliderValue = 2.0;
        this.ContrastSliderValue = 4.0;

        // Force property changed 
        this.HintsSliderValue = 1.0;
        this.HintsSliderValue = 0.0;
    }

    //private void SetUiParametersFromGame(PuzzleParameters puzzleParameters)
    //{
    //    // Need to 'remember' the maximum piece count
    //    this.PieceCountMin = 9;
    //    this.PieceCountMax = 1080;
    //    this.PieceCountSliderValue = puzzleParameters.PieceCount;
    //    this.RotationsSliderValue = puzzleParameters.RotationSteps;
    //    this.SnapSliderValue = puzzleParameters.Snap;
    //    this.ContrastSliderValue = 4.0;
    //    this.HintsSliderValue = puzzleParameters.Hints;

    //    // Do it here in case we have not loaded the pieceCounts setups yet
    //    // See OnPieceCountSliderValueChanged below
    //    this.PieceCountString = string.Format("{0:D}", puzzleParameters.PieceCount);
    //}

    partial void OnPieceCountSliderValueChanged(double value)
    {
        if (this.pieceCounts.Count == 0)
        {
            return;
        }

        int closest = 0;
        double minDistance = double.MaxValue;
        foreach (int count in this.pieceCounts)
        {
            double distance = Math.Abs(value - count);
            if (distance < minDistance)
            {
                closest = count;
                minDistance = distance;
            }
        }

        this.pieceCount = closest;
        this.PieceCountString = string.Format("{0:D}", this.pieceCount);
    }

    partial void OnRotationsSliderValueChanged(double value)
    {
        this.rotations = (int)value;
        this.LocalizeRotations(); 
    }

    private void LocalizeRotations () =>
        this.RotationsString =
            this.rotations <= 1 ?
                this.Localize("Collection.None") :
                string.Format("{0:D}", this.rotations);

    partial void OnSnapSliderValueChanged(double value)
    {
        this.snap = (int)value;
        this.LocalizeSnap(); 
    }

    private void LocalizeSnap() =>
        this.SnapString =
            this.snap == 0 ?
                this.Localize("Collection.SnapMighty") :
                this.snap == 1 ?
                    this.Localize("Collection.SnapStrong") :
                    this.snap == 2 ?
                        this.Localize("Collection.SnapNormal") :
                        this.Localize("Collection.SnapWeak");

    partial void OnHintsSliderValueChanged(double value)
    {
        this.hints = (int)value;
        this.LocalizeHints();
    }

    private void LocalizeHints() =>
        this.HintsString =
            this.hints == 0 ?
                this.Localize("Collection.HintsInfinite") :
                this.hints == 1 ?
                    this.Localize("Collection.HintsThree") :
                    this.hints == 2 ?
                        this.Localize("Collection.HintsOnlyOne") :
                        this.Localize("Collection.None");

    partial void OnContrastSliderValueChanged(double value)
    {
        this.contrast = (int)value;
        this.LocalizeContrast();

        if (this.contrast == 4)
        {
            // No contrast adjustment 
            this.PuzzleImage = this.sourceImage;
            return;
        }
        else
        {
        }
    }

    private void LocalizeContrast() =>
        this.ContrastString =
            this.contrast == 0 ?
                            this.Localize("Collection.ContrastExtreme") :
                this.contrast == 1 ?
                            this.Localize("Collection.ContrastStrong") :
                    this.contrast == 2 ?
                            this.Localize("Collection.ContrastMedium") :
                        this.contrast == 3 ?
                            this.Localize("Collection.ContrastWeak") :
                            this.Localize("Collection.None");


    #endregion Game Parameters 

    private bool SetupUiForNewGame()
    {
        //if (this.PuzzleImage is null)
        //{
        //    return false;
        //}

        //var pixelSize = this.PuzzleImage.PixelSize;
        //var maybeSetups = Puzzle.GenerateSetups(new IntSize(pixelSize.Height, pixelSize.Width));
        //var counts = (from count in maybeSetups.Keys orderby count descending select count).ToList();
        //if (counts.Count < 3)
        //{
        //    // Failed ? 
        //    this.state = PlayStatus.Unprepared;
        //    this.ParametersVisible = false;

        //    // TODO: Message
        //    return false;
        //}

        //int max = counts[0];
        //int min = counts[^1];
        //this.setups = maybeSetups;
        //this.pieceCounts = counts;

        //// UI update 
        //this.PieceCountMin = min;
        //this.PieceCountMax = max;
        //this.pieceCount = min;
        //this.SetUiParametersToDefaults();
        //this.ParametersVisible = true;
        //this.ParametersEnabled = true;
        //this.ParametersOpacity = 1.0;
        //this.state = PlayStatus.ReadyForNew;
        return true;
    }

    internal void OnPuzzleCompleted()
    {
        Schedule.OnUiThread(210, () =>
        {
            this.ThumbnailsPanelViewModel.LoadThumnails();
        }, DispatcherPriority.Background);
    }
}
