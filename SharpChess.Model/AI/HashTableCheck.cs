namespace SharpChess.Model.AI; 

/// <summary> The hash table (also know as Transposition table) specifically for check positions. </summary>
public sealed class HashTableCheck
{
    /// <summary> The hash table size. </summary>
    private readonly uint hashTableSize;

    /// <summary> The array of hash entries. </summary>
    private readonly HashEntry[] hashTableEntries;

    public HashTableCheck()
    {
        this.hashTableSize = Game.AvailableMegaBytes * 4000;
        this.hashTableEntries = new HashEntry[hashTableSize];
        this.Clear();
    }

    /// <summary> Gets the number of hash table Hits that have occured. </summary>
    public int Hits { get; private set; }

    /// <summary> Gets the number of hash table Overwrites that have occured. </summary>
    public int Overwrites { get; private set; }

    /// <summary> Gets the number of hash table Probes that have occured. </summary>
    public int Probes { get; private set; }

    /// <summary> Gets the number of hash table Writes that have occured.</summary>
    public int Writes { get; private set; }

    /// <summary> Clears all entries in the hash table. </summary>
    public void Clear()
    {
        this.ResetStats();
        for (uint intIndex = 0; intIndex < hashTableSize; intIndex++)
        {
            hashTableEntries[intIndex].HashCodeA = 0;
            hashTableEntries[intIndex].HashCodeB = 0;
            hashTableEntries[intIndex].IsInCheck = false;
        }
    }

    /// <summary> Checks if the player is in check for the specified position, and caches the result. </summary>
    /// <param name="hashCodeA"> Hash Code for Board position A </param>
    /// <param name="hashCodeB"> Hash Code for Board position B </param>
    /// <param name="player"> The player.  </param>
    /// <returns> Returns whether the player in check. </returns>
    public unsafe bool QueryandCachePlayerInCheckStatusForPosition(ulong hashCodeA, ulong hashCodeB, Player player)
    {
        fixed (HashEntry* phashBase = &hashTableEntries[0])
        {
            if (player.Colour == Player.PlayerColourNames.Black)
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

            HashEntry* phashEntry = phashBase;
            phashEntry += (uint)(hashCodeA % hashTableSize);

            if (phashEntry->HashCodeA != hashCodeA || phashEntry->HashCodeB != hashCodeB)
            {
                if (phashEntry->HashCodeA != 0)
                {
                    Overwrites++;
                }

                phashEntry->HashCodeA = hashCodeA;
                phashEntry->HashCodeB = hashCodeB;
                phashEntry->IsInCheck = player.DetermineCheckStatus();
                Writes++;
            }
            else
            {
                Hits++;
            }

            return phashEntry->IsInCheck;
        }
    }

    /// <summary> Resets the hashtable statistics. </summary>
    public void ResetStats()
    {
        this.Probes = 0;
        this.Hits = 0;
        this.Writes = 0;
        this.Overwrites = 0;
    }

    /// <summary> Content of the hashtable </summary>
    private struct HashEntry
    {
        /// <summary> The hash code a. </summary>
        public ulong HashCodeA;

        /// <summary> The hash code b. </summary>
        public ulong HashCodeB;

        /// <summary> Boolean indicating that the player is in check. </summary>
        public bool IsInCheck;
    }
}