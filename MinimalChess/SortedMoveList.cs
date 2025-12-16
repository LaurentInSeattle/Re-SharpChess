namespace MinimalChess; 

public sealed class SortedMoveList : List<SortedMove>
{
    internal static SortedMoveList Quiets(Board position)
    {
        var quietMoves = new SortedMoveList();
        position.CollectQuiets(m => quietMoves.Add(m, 0));
        return quietMoves;
    }

    internal static SortedMoveList SortedCaptures(Board position)
    {
        var captures = new SortedMoveList();
        position.CollectCaptures(m => captures.Add(m, ScoreMvvLva(m, position)));
        captures.Sort();
        return captures;
    }

    public static SortedMoveList SortedQuiets(Board position, History history)
    {
        var quiets = new SortedMoveList();
        position.CollectQuiets(m => quiets.Add(m, history.Value(position, m)));
        quiets.Sort();
        return quiets;
    }

    private static int ScoreMvvLva(Move move, Board context)
    {
        Piece victim = context[move.ToSquare];
        Piece attacker = context[move.FromSquare];
        return Pieces.MaxOrder * Pieces.Order(victim) - Pieces.Order(attacker);
    }

    private void Add(Move move, float priority) 
        => this.Add(new SortedMove { Move = move, Priority = priority });
}
