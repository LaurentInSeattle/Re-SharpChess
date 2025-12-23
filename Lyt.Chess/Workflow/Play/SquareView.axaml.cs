namespace Lyt.Chess.Workflow.Play;

public sealed partial class SquareView : View
{
    public SquareView() : base() => this.PointerPressed += this.OnPointerPressed;

    public void OnPointerPressed(object? _, PointerPressedEventArgs pointerPressedEventArgs) 
    {
        bool handled = false;
        if (this.DataContext is SquareViewModel squareViewModel)
        {
            handled = squareViewModel.OnClicked();
        }

        pointerPressedEventArgs.Handled = handled;
    }

    internal void DisableClicks() => this.PointerPressed += this.OnPointerPressed;
}
