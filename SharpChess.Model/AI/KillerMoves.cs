namespace SharpChess.Model.AI; 

/// <summary>
/// Represents the Killer Heuristic used to improve move ordering.
///   http://chessprogramming.wikispaces.com/Killer+Heuristic
/// </summary>
public sealed class KillerMoves
{
    /// <summary> List of primary (A) Killer Moves indexed by search depth. </summary>
    private readonly Move?[] PrimaryKillerMovesA ;

    /// <summary>
    ///   List of secondary (B) Killer Moves indexed by search depth.
    /// </summary>
    private readonly Move?[] SecondaryKillerMovesB ;

    private readonly Game game; 

    /// <summary>
    /// Initializes a new instance of the KillerMoves class for the specified game position.    
    /// </summary>
    /// <remarks>Killer moves are a heuristic used in game search algorithms to improve move ordering. This
    /// constructor prepares the data structures for tracking killer moves associated with the given game.</remarks>
    /// <param name="game">The game position for which killer moves will be tracked. Cannot be null.</param>
    public KillerMoves(Game game)
    {
        this.game = game;
        PrimaryKillerMovesA = new Move[64];
        SecondaryKillerMovesB = new Move[64];
        this.Clear();
    }

    /// <summary> Clears all tables. </summary>
    public void Clear()
    {
        for (int intIndex = 0; intIndex < 64; intIndex++)
        {
            PrimaryKillerMovesA[intIndex] = null;
            SecondaryKillerMovesB[intIndex] = null;
        }
    }

    /// <summary>
    /// Adds the move made to the appropriate killer move slot, if it's better than the current killer moves
    /// </summary>
    /// <param name="ply"> Search depth </param>
    /// <param name="moveMade"> Move to be added </param>
    public void RecordPossibleKillerMove(int ply, Move moveMade)
    {
        // Disable if this feature when switched off.
        if (!this.game.EnableKillerMoves)
        {
            return;
        }

        bool blnAssignedA = false; // Have we assign Slot A?

        Move moveKillerA = RetrieveA(ply);
        Move moveKillerB = RetrieveB(ply);

        if (moveKillerA == null)
        {
            // Slot A is blank, so put anything in it.
            AssignA(ply, moveMade);
            blnAssignedA = true;
        }
        else if ((moveMade.Score > moveKillerA.Score && !Move.MovesMatch(moveMade, moveKillerB)) || Move.MovesMatch(moveMade, moveKillerA))
        {
            // Move's score is better than A and isn't B, or the move IS A, 
            blnAssignedA = true;
            if (Move.MovesMatch(moveMade, moveKillerA))
            {
                // Re-record move in Slot A, but only if it's better
                if (moveMade.Score > moveKillerA.Score)
                {
                    AssignA(ply, moveMade);
                }
            }
            else
            {
                // Score is better than Slot A

                // transfer move in Slot A to Slot B...
                AssignB(ply, moveKillerA);

                // record move is Slot A
                AssignA(ply, moveMade);
            }

            moveKillerA = RetrieveA(ply);
        }

        // If the move wasn't assigned to Slot A, then see if it is good enough to go in Slot B, or if move IS B
        if (!blnAssignedA)
        {
            // Slot B is empty, so put anything in!
            if (moveKillerB == null)
            {
                AssignB(ply, moveMade);
            }
            else if (moveMade.Score > moveKillerB.Score)
            {
                // Score is better than Slot B, so
                // record move is Slot B
                AssignB(ply, moveMade);
            }

            moveKillerB = RetrieveB(ply);
        }

        // Finally check if B score is better than and A score, and if so swap.
        if (moveKillerA != null && moveKillerB != null && moveKillerB.Score > moveKillerA.Score)
        {
            Move swap = moveKillerA;
            AssignA(ply, moveKillerB);
            AssignB(ply, swap);
        }
    }

    /// <summary> Retrieve primary (A) killer move for specified search depth. </summary>
    /// <param name="depth"> Search depth (ply). </param>
    /// <returns> Move for specified depth </returns>
    public Move? RetrieveA(int depth)
    {
        Move? move = this.PrimaryKillerMovesA[depth + 32];
        return move;
    }

    /// <summary> Retrieve secondary (B) killer move for specified search depth. </summary>
    /// <param name="depth"> Search depth (ply). </param>
    /// <returns> Move for specified depth </returns>
    public Move? RetrieveB(int depth)
    {
        return this.SecondaryKillerMovesB[depth + 32];
    }

    /// <summary> Assign killer move A (primary) </summary>
    /// <param name="depth"> The search depth (ply). </param>
    /// <param name="move"> The move to assign.
    /// </param>
    private void AssignA(int depth, Move move)
        => this.PrimaryKillerMovesA[depth + 32] = move;

    /// <summary> Assign killer move B (secondary) </summary>
    /// <param name="depth"> The search depth (ply). </param>
    /// <param name="move"> The move to assign. </param>
    private void AssignB(int depth, Move move)
        => this.SecondaryKillerMovesB[depth + 32] = move;
}