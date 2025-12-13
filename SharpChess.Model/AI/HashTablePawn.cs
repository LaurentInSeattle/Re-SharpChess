namespace SharpChess.Model.AI;

/// <summary>
/// The hash table purely for pawn position. Used to optimised evalulation of score for pawn positions.
/// Position values are cachable if they are affected *exclusively* to pawn position.
/// http://chessprogramming.wikispaces.com/Pawn+Hash+Table
/// </summary>
public sealed class HashTablePawn
{
    /// <summary> Indicates that a position was not found in the Hash Table. </summary>
    public const int NotFoundInHashTable = int.MinValue;

    /// <summary> the array of the entries in the HashTable </summary>
    private readonly HashEntry[] hashTableEntries;

    /// <summary> Size of the HashTable. </summary>
    private readonly uint hashTableSize;

    public HashTablePawn()
    {
        this.hashTableSize = Game.AvailableMegaBytes * 3000;
        this.hashTableEntries = new HashEntry[hashTableSize];
        this.Clear();
    }

    /// <summary> Gets the number of hash table Collisions that have occured. </summary>
    public int Collisions { get; private set; }

    /// <summary> Gets the number of hash table Hits that have occured. </summary>
    public int Hits { get; private set; }

    /// <summary> Gets the number of hash table Writes that have occured. </summary>
    public static int Writes { get; private set; }

    /// <summary> Gets the number of hash table Overwrites that have occured. </summary>
    public int Overwrites { get; private set; }

    /// <summary> Gets the number of hash table Probes that have occured. </summary>
    public int Probes { get; private set; }

    /// <summary> Gets the number of hash table slots used. </summary>
    public int SlotsUsed
    {
        get
        {
            int intCounter = 0;
            for (uint intIndex = 0; intIndex < hashTableSize; intIndex++)
            {
                if (hashTableEntries[intIndex].HashCodeA != 0)
                {
                    intCounter++;
                }
            }

            return intCounter;
        }
    }

    /// <summary> Clears all entries in the hash table. </summary>
    public void Clear()
    {
        ResetStats();
        for (uint intIndex = 0; intIndex < hashTableSize; intIndex++)
        {
            hashTableEntries[intIndex].HashCodeA = 0;
            hashTableEntries[intIndex].HashCodeB = 0;
            hashTableEntries[intIndex].Points = NotFoundInHashTable;
        }
    }

    // TODO: Figure out What is meant about pawn AND KING ....
    //
    /// <summary> Search pawn and king hash table for a pawn and king specific score for the specific position hash. </summary>
    /// <param name="hashCodeA"> Hash Code for Board position A </param>
    /// <param name="hashCodeB"> Hash Code for Board position B </param>
    /// <param name="colour"> The player colour. </param>
    /// <returns> Pawn and king specific score for the specified position. </returns>
    public unsafe int ProbeHash(ulong hashCodeA, ulong hashCodeB, Player.PlayerColourNames colour)
    {
        if (colour == Player.PlayerColourNames.Black)
        {
            hashCodeA |= 0x1;
            hashCodeB |= 0x1;
        }
        else
        {
            hashCodeA &= 0xFFFFFFFFFFFFFFFE;
            hashCodeB &= 0xFFFFFFFFFFFFFFFE;
        }

        Probes++;

        fixed (HashEntry* phashBase = &hashTableEntries[0])
        {
            HashEntry* phashEntry = phashBase;
            phashEntry += (uint)(hashCodeA % hashTableSize);

            if (phashEntry->HashCodeA == hashCodeA && phashEntry->HashCodeB == hashCodeB)
            {
                Hits++;
                return phashEntry->Points;
            }
        }

        return NotFoundInHashTable;
    }

    // TODO: Same here: Figure out What is meant about pawn AND KING ....
    //
    /// <summary> Record the pawn and kind specific positional score in the pawn hash table. </summary>
    /// <param name="hashCodeA"> Hash Code for Board position A </param>
    /// <param name="hashCodeB"> Hash Code for Board position B </param>
    /// <param name="val"> Pawn specific score.  </param>
    /// <param name="colour"> Player colour. </param>
    public unsafe void RecordHash(ulong hashCodeA, ulong hashCodeB, int val, Player.PlayerColourNames colour)
    {
        if (colour == Player.PlayerColourNames.Black)
        {
            hashCodeA |= 0x1;
            hashCodeB |= 0x1;
        }
        else
        {
            hashCodeA &= 0xFFFFFFFFFFFFFFFE;
            hashCodeB &= 0xFFFFFFFFFFFFFFFE;
        }

        fixed (HashEntry* phashBase = &hashTableEntries[0])
        {
            HashEntry* phashEntry = phashBase;
            phashEntry += (uint)(hashCodeA % hashTableSize);
            phashEntry->HashCodeA = hashCodeA;
            phashEntry->HashCodeB = hashCodeB;
            phashEntry->Points = val;
        }

        Writes++;
    }

    /// <summary> Reset all hash table statistics. </summary>
    public void ResetStats()
    {
        Probes = 0;
        Hits = 0;
        Writes = 0;
        Collisions = 0;
        Overwrites = 0;
    }

    /// <summary> Hash Table entry data structure. </summary>
    private struct HashEntry
    {
        /// <summary> Pawn Position Hash code A. </summary>
        public ulong HashCodeA;

        /// <summary> Pawn Position Hash code B. </summary>
        public ulong HashCodeB;

        /// <summary> Pawn positional score. </summary>
        public int Points;
    }
}