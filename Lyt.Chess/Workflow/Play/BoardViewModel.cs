namespace Lyt.Chess.Workflow.Play;

internal class BoardViewModel : ViewModel<BoardView>
{
    public void CreateBoard()
    {
        // Initialize square view models
        for (int index = 0; index < 64; index++)
        {
            int rank = index / 8;
            int file = index % 8;
            var squareViewModel = new SquareViewModel(rank, file);
            _ = squareViewModel.CreateViewAndBind();
            this.View.AddSquareView(squareViewModel);
        }

        for (int index = 0; index < 8; index++)
        {
            this.View.AddRankFileTextBoxes(index);
        }
    }
}