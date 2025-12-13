namespace SharpChess.Model;

// TODO : Implement as derived from List<Piece>
//
/// <summary> A list of pieces. </summary>
public class Pieces : IEnumerable
{
    /// <summary> Internal List of pieces. </summary>
    private readonly List<Piece> pieces = new(16);
    private static PieceSort sorter = new ();

    /// <summary>
    /// Gets Count.
    /// </summary>
    public int Count
    {
        get
        {
            return this.pieces.Count;
        }
    }

    /// <summary>
    /// The add.
    /// </summary>
    /// <param name="piece">
    /// The piece.
    /// </param>
    public void Add(Piece piece)
    {
        this.pieces.Add(piece);
    }

    /// <summary>
    /// Return a clone of this list.
    /// </summary>
    /// <returns>
    /// The clone.
    /// </returns>
    public object Clone()
    {
        return this.pieces.ToList();
    }

    /// <summary>
    /// Get the enumerator for this list.
    /// </summary>
    /// <returns>
    /// The enumerator.
    /// </returns>
    public IEnumerator GetEnumerator()
    {
        return this.pieces.GetEnumerator();
    }

    /// <summary>
    /// Searches for the specified piece and returns its index.
    /// </summary>
    /// <param name="piece">
    /// The piece to search for.
    /// </param>
    /// <returns>
    /// Index value of the found piece. or null if not found.
    /// </returns>
    public int IndexOf(Piece piece)
    {
        return this.pieces.IndexOf(piece);
    }

    /// <summary>
    /// Insert a piece into the list. at the specified index position.
    /// </summary>
    /// <param name="ordinal">
    /// The ordinal index position where the piece will be inserted.
    /// </param>
    /// <param name="piece">
    /// The piece.
    /// </param>
    public void Insert(int ordinal, Piece piece)
    {
        this.pieces.Insert(ordinal, piece);
    }

    /// <summary>
    /// Returns the piece at the specified index position in the list.
    /// </summary>
    /// <param name="intIndex">
    /// Index position.
    /// </param>
    /// <returns>
    /// The piece at the specified index.
    /// </returns>
    public Piece Item(int intIndex)
    {
        return this.pieces[intIndex];
    }

    /// <summary>
    /// Remove the piece from the list.
    /// </summary>
    /// <param name="piece">
    /// The piece to remove.
    /// </param>
    public void Remove(Piece piece)
    {
        this.pieces.Remove(piece);
    }

    /// <summary>
    /// The sort the pieces by their score value.
    /// </summary>
    public void SortByScore()
    {
        this.pieces.Sort(sorter);
    }
}

public class PieceSort : IComparer<Piece>
{
    public int Compare(Piece? x, Piece? y)
    {
        if (y == null)
        {
            return 1;
        }
        
        if (x == null)
        {
            return -1;
        }
        
        if (x.Value > y.Value)
        {
            return 1;
        }
        else if (x.Value < y.Value)
        {
            return -1;
        }
        else if (x.Value == y.Value)
        {
            return y.Name == Piece.PieceNames.Knight ? 1 : -1;
        }

        return 1;
    }
}