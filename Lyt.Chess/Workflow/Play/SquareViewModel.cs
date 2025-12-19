namespace Lyt.Chess.Workflow.Play;

internal partial class SquareViewModel : ViewModel<SquareView>
{
    private readonly BoardViewModel boardViewModel;
    private PieceViewModel? pieceViewModel;

    [ObservableProperty]
    private SolidColorBrush background;

    [ObservableProperty]
    private bool isValidMove;

    [ObservableProperty]
    private bool isInvalidMove;

    [ObservableProperty]
    private bool isInCheck;

    public SquareViewModel(BoardViewModel boardViewModel, int rank, int file)
    {
        this.boardViewModel = boardViewModel;
        this.Rank = rank;
        this.File = file;
        this.pieceViewModel = null;

        // TODO: Use images textures for the squares
        // For now we use simple colors for the squares
        PlayerColor squareColor =
            (this.Rank + this.File) % 2 == 0 ?
                PlayerColor.White :
                PlayerColor.Black;
        this.Background =
            squareColor == PlayerColor.White ?
                new SolidColorBrush(Colors.BurlyWood) :
                new SolidColorBrush(Colors.SaddleBrown);
        this.IsValidMove = false;
        this.IsInvalidMove = false;
        this.IsInCheck = false;
    }

    internal int Rank { get; private set; }

    internal int File { get; private set; }

    internal bool IsEmpty => this.pieceViewModel is null;

    internal void Select(bool select) => this.pieceViewModel?.Select(select);

    internal bool OnClicked()
    {
        if (this.boardViewModel.HasSelectedSquare)
        {
            this.boardViewModel.ClearSelection();
        }
        else
        {
            this.Select(select: true);
        }

        return true;
    }

    internal void PlacePiece(PieceViewModel pieceViewModel)
    {
        if (this.pieceViewModel is not null)
        {
            throw new Exception("Trying to place a piece on an already occupied square.");
        }

        this.pieceViewModel = pieceViewModel;
    }

    internal PieceViewModel RemovePiece()
    {
        if (this.pieceViewModel is null)
        {
            throw new Exception("Trying to remove the piece of an empty square.");
        }

        var vm = this.pieceViewModel;
        this.pieceViewModel = null;
        return vm;
    }
}
