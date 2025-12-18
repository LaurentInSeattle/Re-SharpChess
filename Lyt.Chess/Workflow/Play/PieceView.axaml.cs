namespace Lyt.Chess.Workflow.Play;

public sealed partial class PieceView : View
{
    private DragMovable? dragMovable;

    public void AttachBehavior(Canvas canvas)
    {
        this.dragMovable = new DragMovable(canvas, adjustPosition: true);
        this.dragMovable.Attach(this);
    }

    ~PieceView()
    {
        this.dragMovable?.Detach();
    }
}
