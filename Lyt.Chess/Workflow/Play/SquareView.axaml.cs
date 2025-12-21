namespace Lyt.Chess.Workflow.Play;

public partial class SquareView : View
{
    public SquareView() : base()
        => this.PointerPressed +=
            (_, pointerPressedEventArgs) =>
            {
                bool handled = false;
                if (this.DataContext is SquareViewModel squareViewModel)
                {
                    handled = squareViewModel.OnClicked();
                }

                pointerPressedEventArgs.Handled = handled;
            };
}
