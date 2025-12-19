namespace Lyt.Chess.Workflow.Play;

public partial class SquareView : View
{
    public SquareView() : base() => this.PointerPressed += this.OnPointerPressed;

    private void OnPointerPressed(object? sender, PointerPressedEventArgs pointerPressedEventArgs)
    {
        // Debug.WriteLine("Sqaure Pressed");
        bool handled = false;
        if ( this.DataContext is SquareViewModel squareViewModel)
        {
            handled = squareViewModel.OnClicked ();
        }

        pointerPressedEventArgs.Handled = handled;
    }
}
