namespace SharpChess.Model;

/// <summary> Helper methods for debuging board positions. </summary>
public static class BoardDebug
{
    /// <summary> Gets a Debug String representing the current board position.</summary>
    public static string DebugString(Game game, Board board)
    {
            string strOutput = string.Empty;
            int intOrdinal = Board.SquareCount - 1;
            for (int intRank = 0; intRank < Board.RankCount; intRank++)
            {
                for (int intFile = 0; intFile < Board.FileCount; intFile++)
                {
                    var square = board.GetSquare(intOrdinal);
                    if (square != null)
                    {
                        Piece? piece = square.Piece;
                        if (piece is not null)
                        {
                            strOutput += piece.Abbreviation;
                        }
                        else
                        {
                            strOutput += square.Colour == Square.ColourNames.White ? "." : "#";
                        }
                    }

                    strOutput += Convert.ToChar(13) + Convert.ToChar(10);

                    intOrdinal--;
                }
            }

            return strOutput;
    }

    /// <summary> Display the chessboard in the Immediate Windows </summary>
    public static void DebugDisplay(Game game, Board board)
    {
        Debug.Write(DebugGetBoard(game, board));
        Debug.Write(". ");
    }

    /// <summary> Display info on the game at the right of the chessboard </summary>
    /// <param name="indRank"> the rank in the chessboard </param>
    /// <param name="strbBoard"> output buffer </param>
    /// <remarks> Display the captured pieces and the MoveHistory </remarks>
    private static void DebugGameInfo(Game game, Board board, int indRank, ref StringBuilder strbBoard)
    {
        strbBoard.Append(':');
        strbBoard.Append(indRank);
        strbBoard.Append(' ');
        switch (indRank)
        {
            case 0:
            case 7:
                Pieces piecesCaptureList = (indRank == 7) ?
                    game.PlayerWhite.CapturedEnemyPieces :
                    game.PlayerBlack.CapturedEnemyPieces;
                if (piecesCaptureList.Count > 1)
                {
                    strbBoard.Append("x ");
                    foreach (Piece pieceCaptured in piecesCaptureList)
                    {
                        strbBoard.Append(
                            (pieceCaptured.Name == Piece.PieceNames.Pawn)
                                ? string.Empty
                                : pieceCaptured.Abbreviation + pieceCaptured.Square.Name + " ");
                    }
                }

                break;

            case 5:
                int turnNumberOld = game.TurnNo; // Backup TurNo
                game.TurnNo -= game.PlayerToPlay.Brain.Search.SearchDepth;
                for (int indMov = Math.Max(1, game.MoveHistory.Count - game.PlayerToPlay.Brain.Search.MaxSearchDepth);
                     indMov < game.MoveHistory.Count;
                     indMov++)
                {
                    Move? moveThis = game.MoveHistory[indMov];
                    if (moveThis is not null)
                    {
                        if (moveThis.Piece.Player.Colour == Player.PlayerColourNames.White)
                        {
                            strbBoard.Append(indMov >> 1);
                            strbBoard.Append(". ");
                        }

                        // moveThis.PgnSanFormat(false); // Contextual to Game.TurNo
                        strbBoard.Append(moveThis.Description + " ");
                        game.TurnNo++;
                    }
                }

                game.TurnNo = turnNumberOld; // Restore TurNo
                break;
        }

        strbBoard.Append('\n');
    }

    /// <summary> A string representation of the board position - useful for debugging. </summary>
    /// <returns> Board position string. </returns>
    public static string DebugGetBoard(Game game, Board board)
    {
        var strbBoard = new StringBuilder(160);
        strbBoard.Append("  0 1 2 3 4 5 6 7 :PlayerToPlay = ");
        strbBoard.Append((game.PlayerToPlay.Colour == Player.PlayerColourNames.White) ? "White\n" : "Black\n");
        for (int indRank = 7; indRank >= 0; indRank--)
        {
            strbBoard.Append(indRank + 1);
            strbBoard.Append(':');
            for (int indFile = 0; indFile < 8; indFile++)
            {
                Square? square = board.GetSquare(indFile, indRank);
                if (square != null)
                {
                    if (square.Piece == null)
                    {
                        strbBoard.Append(". ");
                    }
                    else
                    {
                        switch (square.Piece.Player.Colour)
                        {
                            case Player.PlayerColourNames.White:
                                strbBoard.Append(square.Piece.Abbreviation);
                                break;
                            default:
                                strbBoard.Append(square.Piece.Abbreviation.ToLower());
                                break;
                        }

                        strbBoard.Append(' ');
                    }
                }
            }

            DebugGameInfo(game, board, indRank, ref strbBoard);
        }

        strbBoard.Append("  a b c d e f g h :TurnNo = ");
        strbBoard.Append(game.TurnNo);
        return strbBoard.ToString();
    }
}