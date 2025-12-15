namespace MinimalChess; 

public struct SearchWindow(int floor, int ceiling)
{
    public static readonly SearchWindow Infinite = new(short.MinValue, short.MaxValue);

    public readonly SearchWindow UpperBound => new(this.Ceiling - 1, this.Ceiling);

    public readonly SearchWindow LowerBound => new(this.Floor, this.Floor + 1);

    //used to quickly determine that a move is not improving the score for color.
    public readonly SearchWindow GetLowerBound(Color color) => color == Color.White ? this.LowerBound : this.UpperBound;

    //used to quickly determine that a move is too good and will not be allowed by the opponent .
    public readonly SearchWindow GetUpperBound(Color color) => color == Color.White ? this.UpperBound : this.LowerBound;

    // Enforce immutability from outside
    public int Floor { get; private set; } = floor; //Alpha

    public int Ceiling { get; private set; } = ceiling; //Beta

    public bool Cut(int score, Color color)
    {
        if (color == Color.White) //Cut floor
        {
            if (score <= this.Floor)
            {
                return false; //outside search window
            }

            this.Floor = score;
            return this.Floor >= this.Ceiling; //Cutoff?
        }
        else
        {
            if (score >= this.Ceiling) //Cut ceiling
            {
                return false; //outside search window
            }

            this.Ceiling = score;
            return this.Ceiling <= this.Floor; //Cutoff?
        }
    }

    public readonly bool FailLow(int score, Color color) 
        => color == Color.White ? (score <= this.Floor) : (score >= this.Ceiling);

    public readonly bool FailHigh(int score, Color color) 
        => color == Color.White ? (score >= this.Ceiling) : (score <= this.Floor);

    public readonly int GetScore(Color color) 
        => color == Color.White ? this.Floor : this.Ceiling;

    public readonly bool CanFailHigh(Color color) 
        => color == Color.White ? (this.Ceiling < short.MaxValue) : (this.Floor > short.MinValue);
}
