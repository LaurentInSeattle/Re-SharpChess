namespace Lyt.Chess.Workflow.Play;

public sealed partial class PlayView: View
{
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            e.Handled = true;
            new ToolbarCommandMessage(ToolbarCommandMessage.ToolbarCommand.PlayWindowed).Publish();
        } 

        base.OnKeyDown(e);
    }

    internal void AddChildren(View boardView, View scoreView)
    {
        this.InnerGrid.Children.Add(boardView);
        this.InnerGrid.Children.Add(scoreView);
        scoreView.SetValue(Grid.ColumnProperty, 1);
    }
}
