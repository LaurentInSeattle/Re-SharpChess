namespace Lyt.Chess.Workflow.Play;

using global::Avalonia.Styling;

public partial class BoardView : View
{
    // No need to localize these strings (FIDE official notation)
    private static readonly string[] fileStrings = ["a", "b", "c", "d", "e", "f", "g", "h"];
    private static readonly string[] rankStrings = ["1", "2", "3", "4", "5", "6", "7", "8"];

    internal void AddSquareView(SquareViewModel squareViewModel)
    {
        var squareView = squareViewModel.View;
        this.BoardGrid.Children.Add(squareView);
        squareView.SetValue(Grid.RowProperty, 7 - squareViewModel.Rank);
        squareView.SetValue(Grid.ColumnProperty, 7 - squareViewModel.File);
    }

    internal void AddRankFileTextBoxes(int index)
    {
        var controlTheme = Lyt.Avalonia.Controls.Utilities.FindResource<ControlTheme>("Medium"); 
        string fileText = fileStrings[index];
        var textFileBottom = new TextBlock()
        {
            Theme = controlTheme,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center, 
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            Text = fileText,
        };

        this.FileLabelsGridBottom.Children.Add(textFileBottom);
        textFileBottom.SetValue(Grid.ColumnProperty, index);

        var textFileTop = new TextBlock()
        {
            Theme = controlTheme,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            Text = fileText,
        };

        this.FileLabelsGridTop.Children.Add(textFileTop);
        textFileTop.SetValue(Grid.ColumnProperty, index);

        string rankText = rankStrings[index];
        var textRankLeft = new TextBlock()
        {
            Theme = controlTheme,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            Text = rankText,
        };

        this.RankLabelsGridLeft.Children.Add(textRankLeft);
        textRankLeft.SetValue(Grid.RowProperty, 7 - index);

        var textRankRight = new TextBlock()
        {
            Theme = controlTheme,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            Text = rankText,
        };

        this.RankLabelsGridRight.Children.Add(textRankRight);
        textRankRight.SetValue(Grid.RowProperty, 7 - index);
    }

    internal void AddPieceView(PieceViewModel whitePieceViewModel, int rank, int file)
    {
        var pieceView = whitePieceViewModel.View;
        this.BoardGrid.Children.Add(pieceView);
        pieceView.SetValue(Grid.RowProperty, 7 - rank);
        pieceView.SetValue(Grid.ColumnProperty, 7 - file);
        pieceView.AttachBehavior(this.BoardCanvas); 
    }
}