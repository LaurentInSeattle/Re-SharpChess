namespace MinimalChess;

public sealed partial class Evaluation
{
    private const short Midgame = 5255;
    private const short Endgame = 435;

    public struct Eval
    {
        public short MidgameScore;

        public short EndgameScore;
        
        public short Phase;

        public readonly int Score
        {
            get
            {
                //linearily interpolate between midGame and endGame score based on current phase (tapered eval)
                double phase = Linstep(Midgame, Endgame, Phase);
                double score = MidgameScore + phase * (EndgameScore - MidgameScore);
                return (int)score;
            }
        }

        public Eval(Board board) : this()
        {
            for (int i = 0; i < 64; i++)
            {
                if (board[i] != Piece.None)
                {
                    this.AddPiece(board[i], i);
                }
            }
        }

        public void Update(Piece oldPiece, Piece newPiece, int index)
        {
            if (oldPiece != Piece.None)
            {
                this.RemovePiece(oldPiece, index);
            }

            if (newPiece != Piece.None)
            {
                this.AddPiece(newPiece, index);
            }
        }

        private void AddPiece(Piece piece, int squareIndex)
        {
            int pieceIndex = PieceIndex(piece);
            Phase += PhaseValues[pieceIndex];

            if (piece.IsWhite())
            {
                this.AddScore(pieceIndex, squareIndex ^ 56);
            }
            else
            {
                this.SubtractScore(pieceIndex, squareIndex);
            }
        }

        private void RemovePiece(Piece piece, int squareIndex)
        {
            int pieceIndex = PieceIndex(piece);
            Phase -= PhaseValues[pieceIndex];

            if (piece.IsWhite())
            {
                this.SubtractScore(pieceIndex, squareIndex ^ 56);
            }
            else
            {
                this.AddScore(pieceIndex, squareIndex);
            }
        }

        private void AddScore(int pieceIndex, int squareIndex)
        {
            int tableIndex = (pieceIndex << 6) | squareIndex;
            MidgameScore += MidgameTables[tableIndex];
            EndgameScore += EndgameTables[tableIndex];
        }

        private void SubtractScore(int pieceIndex, int squareIndex)
        {
            int tableIndex = (pieceIndex << 6) | squareIndex;
            MidgameScore -= MidgameTables[tableIndex];
            EndgameScore -= EndgameTables[tableIndex];
        }
    }


}
