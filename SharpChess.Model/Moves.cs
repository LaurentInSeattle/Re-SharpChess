namespace SharpChess.Model;

// TODO :
//      Re-Implement as derived from List<Move> 
//      Sort with standard List.Sort()

/// <summary> Holds a list of moves. </summary>
public class Moves : IEnumerable
{
    /// <summary>
    /// The m_col moves.
    /// </summary>
    private readonly List<Move> moves = new(64);
 
    /// <summary>
    /// Initializes a new instance of the <see cref="Moves"/> class.
    /// </summary>
    public Moves()
    {
    }

    // TODO: Not referenced ??? 
    //
    /// <summary>
    /// Initializes a new instance of the <see cref="Moves"/> class.
    /// </summary>
    /// <param name="pieceParent">
    /// The piece parent.
    /// </param>
    public Moves(Piece pieceParent) => this.Parent = pieceParent;
    

    /// <summary> Indicates how the move list was generated. </summary>
    public enum MoveListNames
    {
        All, 
        Recaptures,
        CapturesPromotions,
        CapturesChecksPromotions
    }

    /// <summary> Gets the number of moves contained in the move list. </summary>
    public int Count => this.moves.Count;

    /// <summary> Gets the Last move, can be null. </summary>
    public Move? Last => this.moves.Count > 0 ? (Move)this.moves[this.moves.Count - 1] : null;

    // TODO: Not referenced ??? 
    //
    /// <summary> Gets the parent piece that is holding this move list. </summary>
    public Piece? Parent { get; private set; }

    /// <summary> Gets the penultimate move in this list. </summary>
    public Move? Penultimate => this.moves.Count > 1 ? (Move)this.moves[this.moves.Count - 2] : null;

    /// <summary> Gets the penultimate move in this list For the same side. </summary>
    public Move? PenultimateForSameSide 
        => this.moves.Count > 2 ? (Move)this.moves[this.moves.Count - 3] : null;

    /// <summary> Returns the move specified by the provided index, can be null. </summary>
    /// <param name="intIndex"> The index value. </param>
    /// <returns> The move at the specified index position.</returns>
    public Move? this[int intIndex]
    {
        get
        {
            return (Move?)this.moves[intIndex];
        }

        set
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            this.moves[intIndex] = value;
        }
    }

    /// <summary> Creates a move and adds it to tle list </summary>
    /// <param name="turnNo"> The turn number. </param>
    /// <param name="lastMoveTurnNo"> The last move turn number.</param>
    /// <param name="moveName">The move name. </param>
    /// <param name="piece"> The piece moving. </param>
    /// <param name="from"> The square the piece is moving from. </param>
    /// <param name="to"> The square the peice is moving to. </param>
    /// <param name="pieceCaptured"> The piece being captured, or null.</param>
    /// <param name="pieceCapturedOrdinal">Ordinal position of the piece being captured. </param>
    /// <param name="score"> The positional score.</param>
    public void Add(int turnNo, int lastMoveTurnNo, Move.MoveNames moveName, Piece piece, Square from, Square to, Piece? pieceCaptured, int pieceCapturedOrdinal, int score)
    {
        this.moves.Add(new Move(turnNo, lastMoveTurnNo, moveName, piece, from, to, pieceCaptured, pieceCapturedOrdinal, score));
    }

    /// <summary> Add a new move to this list. </summary>
    /// <param name="move"> The move to be added. </param>
    public void Add(Move move) =>  this.moves.Add(move);
    
    /// <summary> Clear all moves in the list. </summary>
    public void Clear() => this.moves.Clear();
   
    /// <summary> Gest the enumerator for this list. </summary>
    /// <returns> The enumerator for this list. </returns>
    public IEnumerator GetEnumerator() =>  this.moves.GetEnumerator();
    

    /// <summary> Insert a move into this list at the specified index position. </summary>
    /// <param name="intIndex"> The index position.</param>
    /// <param name="move"> The move to insert. </param>
    public void Insert(int intIndex, Move move) =>  this.moves.Insert(intIndex, move);
   
    /// <summary> Remove a move from this list. </summary>
    /// <param name="move"> The move to remove. </param>
    public void Remove(Move move) => this.moves.Remove(move);

    /// <summary> Remove the move at the specified index from this list position. </summary>
    /// <param name="index"> The index position. </param>
    public void RemoveAt(int index) => this.moves.RemoveAt(index);

    /// <summary> Remove the last move from this list. </summary>
    public void RemoveLast() => this.moves.RemoveAt(this.moves.Count - 1);
    
    /// <summary> Replace the move at ths specified index position with the provided move. </summary>
    /// <param name="intIndex"> The index position to replace. </param>
    /// <param name="moveNew"> The new move. </param>
    public void Replace(int intIndex, Move moveNew) => this.moves[intIndex] = moveNew;
    
    /// <summary> Sort this list by score. </summary>
    public void SortByScore()
    {
        // m_colMoves.Sort();
        QuickSort(this.moves, 0, this.moves.Count - 1);
    }

    // QuickSort implementation

    // QuickSort partition implementation

    /// <summary>
    /// Partition method of QuickSort function.
    /// </summary>
    /// <param name="moveArray">
    /// The move array.
    /// </param>
    /// <param name="lower">
    /// The n lower.
    /// </param>
    /// <param name="upper">
    /// The n upper.
    /// </param>
    /// <returns>
    /// The partition.
    /// </returns>
    private static int Partition(List<Move> moveArray, int lower, int upper)
    {
        // Pivot with first element
        int left = lower + 1;
        int pivot = moveArray[lower].Score;
        int right = upper;

        // Partition array elements
        Move moveSwap;
        while (left <= right)
        {
            // Find item out of place
            while (left <= right && moveArray[left].Score >= pivot)
            {
                left = left + 1;
            }

            while (left <= right && moveArray[right].Score < pivot)
            {
                right = right - 1;
            }

            // Swap values if necessary
            if (left < right)
            {
                moveSwap = moveArray[left];
                moveArray[left] = moveArray[right];
                moveArray[right] = moveSwap;
                left = left + 1;
                right = right - 1;
            }
        }

        // Move pivot element
        moveSwap = moveArray[lower];
        moveArray[lower] = moveArray[right];
        moveArray[right] = moveSwap;
        return right;
    }

    /// <summary>
    /// Quicksort an array .
    /// </summary>
    /// <param name="moveArray">
    /// Array of moves.
    /// </param>
    /// <param name="lower">
    /// Lower bound.
    /// </param>
    /// <param name="upper">
    /// Upper bound
    /// </param>
    private static void QuickSort(List<Move> moveArray, int lower, int upper)
    {
        // Check for non-base case
        if (lower < upper)
        {
            // Split and sort partitions
            int split = Partition(moveArray, lower, upper);
            QuickSort(moveArray, lower, split - 1);
            QuickSort(moveArray, split + 1, upper);
        }
    }
}