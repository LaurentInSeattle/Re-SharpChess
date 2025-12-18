namespace MinimalChess;

// TODO: Make this a sealed record struct
public struct SortedMove : IComparable<SortedMove>
{
    public float Priority;

    public Move Move;

    public readonly int CompareTo(SortedMove other) => other.Priority.CompareTo(Priority);

    public static implicit operator Move(SortedMove m) => m.Move;
}