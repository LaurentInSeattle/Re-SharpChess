namespace MinimalChess;

public sealed class LegalMoves : List<Move>
{
    private static readonly Board temporaryBoard = new();

    public LegalMoves(Board referenceBoard) : base(40)
    {
        referenceBoard.CollectMoves(move =>
        {
            //only add if the move doesn't result in a check for active color
            temporaryBoard.DeepCopy(referenceBoard);
            temporaryBoard.Play(move);
            if (!temporaryBoard.IsChecked(referenceBoard.SideToMove))
            {
                this.Add(move);
            }
        });
    }

    public static bool HasMoves(Board referenceBoard)
    {
        bool canMove = false;
        for (int i = 0; i < 64 && !canMove; i++)
        {
            referenceBoard.CollectMoves(i, move =>
            {
                if (canMove)
                {
                    return;
                }

                temporaryBoard.DeepCopy(referenceBoard);
                temporaryBoard.Play(move);
                canMove = !temporaryBoard.IsChecked(referenceBoard.SideToMove);
            });
        }

        return canMove;
    }
}

