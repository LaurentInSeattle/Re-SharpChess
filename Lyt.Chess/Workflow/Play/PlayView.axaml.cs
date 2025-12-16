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
}
