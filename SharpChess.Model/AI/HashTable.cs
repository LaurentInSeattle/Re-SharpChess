namespace SharpChess.Model.AI;

// TODO Incorporate side-to-play colour into hash table key.
// TODO Incorporate 3 move repetition into hash table key.
/// <summary>
/// The hash table, also know as Transposition table. 
/// Stores information about positions previously considered. Stores scores and "best moves".
/// http://chessprogramming.wikispaces.com/Transposition+Table
/// </summary>
public class HashTable
{
    /// <summary> Indicates that a position was not found in the Hash Table. </summary>
    public const int NotFoundInHashTable = int.MinValue;

    /// <summary> The number of chess positions that may be stored against the same hashtable entry key. </summary>
    private const int HashTableSlotDepth = 3;

    /// <summary> Pointer to the HashTable ????? </summary>
    private HashEntry[] hashTableEntries;

    /// <summary> Size of the HashTable. </summary>
    private uint hashTableSize;

    /// <summary> Type of HashTable entry. </summary>
    public enum HashTypeNames
    {
        /// <summary Exact value.</summary>
        Exact, 

        /// <summary> Alpha value. </summary>
        Alpha, 

        /// <summary> Beta value.</summary>
        Beta
    }

    private readonly Game game;
    private readonly Board board;

    public HashTable(Game game)
    {
        this.game = game;
        this.board = game.Board;

        hashTableSize = Game.AvailableMegaBytes * 8_000;
        hashTableEntries = new HashEntry[hashTableSize];
        this.Clear();
    }

    /// <summary> Gets the number of hash table Collisions that have occured. </summary>
    public int Collisions { get; private set; }

    /// <summary> Gets the number of hash table Hits that have occured. </summary>
    public int Hits { get; private set; }

    /// <summary> Gets the number of hash table Writes that have occured. </summary>
    public int Writes { get; private set; }

    /// <summary> Gets the number of hash table Overwrites that have occured. </summary>
    public int Overwrites { get; private set; }

    /// <summary> Gets the number of hash table Probes that have occured. </summary>
    public int Probes { get; private set; }

    // TODO: Consider making this a method.
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
            hashTableEntries[intIndex].BlackFrom = -1;
            hashTableEntries[intIndex].BlackMoveName = Move.MoveNames.NullMove;
            hashTableEntries[intIndex].BlackTo = -1;
            hashTableEntries[intIndex].Colour = Player.PlayerColourNames.White;
            hashTableEntries[intIndex].Depth = sbyte.MinValue;
            hashTableEntries[intIndex].HashCodeA = 0;
            hashTableEntries[intIndex].HashCodeB = 0;
            hashTableEntries[intIndex].Result = int.MinValue;
            hashTableEntries[intIndex].Type = HashTypeNames.Exact;
            hashTableEntries[intIndex].WhiteFrom = -1;
            hashTableEntries[intIndex].WhiteMoveName = Move.MoveNames.NullMove;
            hashTableEntries[intIndex].WhiteTo = -1;
        }
    }

    /// <summary> Search for best move in hash table. </summary>
    /// <param name="hashCodeA"> Hash Code for Board position A </param>
    /// <param name="hashCodeB"> Hash Code for Board position B </param>
    /// <param name="colour"> The player colour. </param>
    /// <returns> Best move, or null. </returns>
    public unsafe Move? ProbeForBestMove(ulong hashCodeA, ulong hashCodeB, Player.PlayerColourNames colour)
    {
        //  Disable if this feature when switched off.
        if (!game.EnableTranspositionTable)
        {
            return null;
        }

        // TODO Unit test Hash Table. What happens when same position stored at different depths in diffenent slots with the same hash?
        fixed (HashEntry* phashBase = &hashTableEntries[0])
        {
            HashEntry* phashEntry = phashBase;
            phashEntry += (uint)(hashCodeA % hashTableSize);

            int intAttempt = 0;
            while (phashEntry >= phashBase
                   && (phashEntry->HashCodeA != hashCodeA || phashEntry->HashCodeB != hashCodeB))
            {
                phashEntry--;
                intAttempt++;
                if (intAttempt == HashTableSlotDepth)
                {
                    break;
                }
            }

            if (phashEntry < phashBase)
            {
                phashEntry = phashBase;
            }

            if (phashEntry->HashCodeA == hashCodeA && phashEntry->HashCodeB == hashCodeB)
            {
                if (colour == Player.PlayerColourNames.White)
                {
                    if (phashEntry->WhiteFrom >= 0)
                    {
                        return new Move(
                            0, 
                            0, 
                            phashEntry->WhiteMoveName, 
                            board.GetPiece(phashEntry->WhiteFrom), 
                            board.GetSquare(phashEntry->WhiteFrom), 
                            board.GetSquare(phashEntry->WhiteTo), 
                            board.GetSquare(phashEntry->WhiteTo).Piece, 
                            0, 
                            phashEntry->Result);
                    }
                }
                else
                {
                    if (phashEntry->BlackFrom >= 0)
                    {
                        return new Move(
                            0, 
                            0, 
                            phashEntry->BlackMoveName, 
                            board.GetPiece(phashEntry->BlackFrom), 
                            board.GetSquare(phashEntry->BlackFrom), 
                            board.GetSquare(phashEntry->BlackTo), 
                            board.GetSquare(phashEntry->BlackTo).Piece, 
                            0, 
                            phashEntry->Result);
                    }
                }
            }
        }

        return null;
    }

    /// <summary> Search Hash table for a previously stored score. </summary>
    /// <param name="hashCodeA"> Hash Code for Board position A </param>
    /// <param name="hashCodeB"> Hash Code for Board position B </param>
    /// <param name="depth"> The search depth. </param>
    /// <param name="alpha"> Apha value. </param>
    /// <param name="beta"> Beta value. </param>
    /// <param name="colour"> The player colour. </param>
    /// <returns> The positional score. </returns>
    public unsafe int ProbeForScore(
        ulong hashCodeA, ulong hashCodeB, int depth, int alpha, int beta, Player.PlayerColourNames colour)
    {
        // Disable if this feature when switched off.
        if (!game.EnableTranspositionTable)
        {
            return NotFoundInHashTable;
        }

        Probes++;

        fixed (HashEntry* phashBase = &hashTableEntries[0])
        {
            HashEntry* phashEntry = phashBase;
            phashEntry += (uint)(hashCodeA % hashTableSize);

            int intAttempt = 0;
            while (phashEntry >= phashBase
                   &&
                   (phashEntry->HashCodeA != hashCodeA || phashEntry->HashCodeB != hashCodeB
                    || phashEntry->Depth < depth))
            {
                phashEntry--;
                intAttempt++;
                if (intAttempt == HashTableSlotDepth)
                {
                    break;
                }
            }

            if (phashEntry < phashBase)
            {
                phashEntry = phashBase;
            }

            if (phashEntry->HashCodeA == hashCodeA && phashEntry->HashCodeB == hashCodeB
                && phashEntry->Depth >= depth)
            {
                if (phashEntry->Colour == colour)
                {
                    if (phashEntry->Type == HashTypeNames.Exact)
                    {
                        Hits++;
                        return phashEntry->Result;
                    }

                    if ((phashEntry->Type == HashTypeNames.Alpha) && (phashEntry->Result <= alpha))
                    {
                        Hits++;
                        return alpha;
                    }

                    if ((phashEntry->Type == HashTypeNames.Beta) && (phashEntry->Result >= beta))
                    {
                        Hits++;
                        return beta;
                    }
                }
            }
        }

        return NotFoundInHashTable;
    }

    /// <summary> Record a hash new hash entry in the hash table. </summary>
    /// <param name="hashCodeA"> Hash Code for Board position A </param>
    /// <param name="hashCodeB"> Hash Code for Board position B </param>
    /// <param name="depth"> The search depth. </param>
    /// <param name="val"> The score of the position to record. </param>
    /// <param name="type"> The position type: alpha, beta or exact value. </param>
    /// <param name="from"> From square ordinal. </param>
    /// <param name="to"> To square ordinal. </param>
    /// <param name="moveName"> The move name. </param>
    /// <param name="colour"> The player colour. </param>
    public unsafe void RecordHash(
        ulong hashCodeA, 
        ulong hashCodeB, 
        int depth, 
        int val, 
        HashTypeNames type, 
        int from, 
        int to, 
        Move.MoveNames moveName, 
        Player.PlayerColourNames colour)
    {
        Writes++;
        fixed (HashEntry* phashBase = &hashTableEntries[0])
        {
            HashEntry* phashEntry = phashBase;
            phashEntry += (uint)(hashCodeA % hashTableSize);

            int intAttempt = 0;
            while (phashEntry >= phashBase && phashEntry->HashCodeA != 0 && phashEntry->Depth > depth)
            {
                phashEntry--;
                intAttempt++;
                if (intAttempt == HashTableSlotDepth)
                {
                    break;
                }
            }

            if (phashEntry < phashBase)
            {
                phashEntry = phashBase;
            }

            if (phashEntry->HashCodeA != 0)
            {
                Collisions++;
                if (phashEntry->HashCodeA != hashCodeA || phashEntry->HashCodeB != hashCodeB)
                {
                    Overwrites++;
                    phashEntry->WhiteFrom = -1;
                    phashEntry->BlackFrom = -1;
                }
            }

            phashEntry->HashCodeA = hashCodeA;
            phashEntry->HashCodeB = hashCodeB;
            phashEntry->Result = val;
            phashEntry->Type = type;
            phashEntry->Depth = (sbyte)depth;
            phashEntry->Colour = colour;
            if (from > -1)
            {
                if (colour == Player.PlayerColourNames.White)
                {
                    phashEntry->WhiteMoveName = moveName;
                    phashEntry->WhiteFrom = (sbyte)from;
                    phashEntry->WhiteTo = (sbyte)to;
                }
                else
                {
                    phashEntry->BlackMoveName = moveName;
                    phashEntry->BlackFrom = (sbyte)from;
                    phashEntry->BlackTo = (sbyte)to;
                }
            }
        }
    }

    /// <summary> Reset the hash table statistics. </summary>
    public void ResetStats()
    {
        this.Probes = 0;
        this.Hits = 0;
        this.Writes = 0;
        this.Collisions = 0;
        this.Overwrites = 0;
    }

    /// <summary> The hash table entry. </summary>
    private struct HashEntry
    {
        /// <summary> Black from square ordinal. </summary>
        public sbyte BlackFrom;

        /// <summary> Black move name. </summary>
        public Move.MoveNames BlackMoveName;

        /// <summary> Black to square ordinal. </summary>
        public sbyte BlackTo;

        /// <summary> Player colour. </summary>
        public Player.PlayerColourNames Colour;

        /// <summary> Search depth. </summary>
        public sbyte Depth;

        /// <summary> The hash code A. </summary>
        public ulong HashCodeA;

        /// <summary> The hash code b. </summary>
        public ulong HashCodeB;

        /// <summary> The result (positional score). </summary>
        public int Result;

        /// <summary> The hash table entry type. </summary>
        public HashTypeNames Type;

        /// <summary> White from square ordinal. </summary>
        public sbyte WhiteFrom;

        /// <summary> White move name. </summary>
        public Move.MoveNames WhiteMoveName;

        /// <summary> White to square ordinal. </summary>
        public sbyte WhiteTo;
    }
}