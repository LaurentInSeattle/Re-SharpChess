namespace Lyt.Chess.Workflow.Play;

internal partial class SquareViewModel : ViewModel<SquareView>
{
    [ObservableProperty]
    private SolidColorBrush background;

    [ObservableProperty]
    private bool isValidMove;

    [ObservableProperty]
    private bool isInvalidMove;

    [ObservableProperty]
    private bool isInCheck;

    public SquareViewModel(int rank, int file) 
    {
        this.Rank = rank;
        this.File = file;

        // For now we use simple colors for the squares
        PlayerColor squareColor = 
            (this.Rank + this.File) % 2 == 0 ? 
                PlayerColor.White : 
                PlayerColor.Black;
        this.Background = 
            squareColor == PlayerColor.White ? 
                new SolidColorBrush (Colors.BurlyWood) :
                new SolidColorBrush(Colors.SaddleBrown);
        this.IsValidMove = true;
        this.IsInvalidMove = true;
        this.IsInCheck = true;
    }

    public int Rank { get; private set; } 

    public int File { get; private set; } 
}
