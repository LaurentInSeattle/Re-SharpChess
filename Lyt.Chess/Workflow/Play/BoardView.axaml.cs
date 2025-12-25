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
        squareView.SetValue(Grid.ColumnProperty, squareViewModel.File);
    }

    internal void AddRankFileTextBoxes(int index, bool showForWhite)
    {
        RotateTransform? rotateTransform = showForWhite ? null : new RotateTransform() { Angle = 180 };

        var controlTheme = Lyt.Avalonia.Controls.Utilities.FindResource<ControlTheme>("Medium");
        string fileText = fileStrings[index];
        var textFileBottom = new TextBlock()
        {
            Theme = controlTheme,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            Text = fileText,
            RenderTransform = rotateTransform,
        };

        this.FileLabelsGridBottom.Children.Add(textFileBottom);
        textFileBottom.SetValue(Grid.ColumnProperty, index);

        var textFileTop = new TextBlock()
        {
            Theme = controlTheme,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            Text = fileText,
            RenderTransform = rotateTransform,
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
            RenderTransform = rotateTransform,
        };

        this.RankLabelsGridLeft.Children.Add(textRankLeft);
        textRankLeft.SetValue(Grid.RowProperty, 7 - index);

        var textRankRight = new TextBlock()
        {
            Theme = controlTheme,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            Text = rankText,
            RenderTransform = rotateTransform,
        };

        this.RankLabelsGridRight.Children.Add(textRankRight);
        textRankRight.SetValue(Grid.RowProperty, 7 - index);
    }

    internal void Empty(bool showForWhite)
    {
        var toRemove = new List<PieceView>(32);
        foreach (var view in this.BoardGrid.Children)
        {
            if (view is PieceView pieceView)
            {
                toRemove.Add(pieceView);
            }
        }

        foreach (var view in toRemove)
        {
            _ = this.BoardGrid.Children.Remove(view);
        }
    }

    internal void AddPieceView(PieceViewModel pieceViewModel, int rank, int file, bool showForWhite)
    {
        RotateTransform? rotateTransform = showForWhite ? null : new RotateTransform() { Angle = 180 };
        var pieceView = pieceViewModel.View;
        this.BoardGrid.Children.Add(pieceView);
        pieceView.SetValue(Grid.RowProperty, 7 - rank);
        pieceView.SetValue(Grid.ColumnProperty, file);
        pieceView.SetValue(RenderTransformProperty, rotateTransform);
        pieceView.AttachBehavior(this.BoardCanvas);
    }

    internal void MovePieceView(PieceViewModel pieceViewModel, int rank, int file)
    {
        var pieceView = pieceViewModel.View;
        pieceView.SetValue(Grid.RowProperty, 7 - rank);
        pieceView.SetValue(Grid.ColumnProperty, file);
    }

    internal void RemovePieceView(PieceViewModel pieceViewModel)
    {
        var pieceView = pieceViewModel.View;
        bool removed = this.BoardGrid.Children.Remove(pieceView);
        if (!removed)
        {
            if (Debugger.IsAttached) { Debugger.Break(); }
        }
    }
}