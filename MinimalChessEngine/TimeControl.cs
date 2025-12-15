namespace MinimalChessEngine; 

public sealed class TimeControl
{
    private const int TIME_MARGIN = 20;
    private const int BRANCHING_FACTOR_ESTIMATE = 3;
    private const int MAX_TIME_REMAINING = int.MaxValue / 3; //large but not too large to cause overflow issues

    private int movesToGo;
    private int increment;
    private int remaining;
    private long t0 = -1;
    private long tN = -1;

    public int TimePerMoveWithMargin => (remaining + (movesToGo - 1) * increment) / movesToGo - TIME_MARGIN;

    public int TimeRemainingWithMargin => remaining - TIME_MARGIN;

    private static long Now => Stopwatch.GetTimestamp();

    public int Elapsed => MilliSeconds(Now - t0);
    
    public int ElapsedInterval => MilliSeconds(Now - tN);

    private static int MilliSeconds(long ticks)
    {
        double dt = ticks / (double)Stopwatch.Frequency;
        return (int)(1000 * dt);
    }

    private void Reset()
    {
        this.movesToGo = 1;
        this.increment = 0;
        this.remaining = MAX_TIME_REMAINING; 
        this.t0 = Now;
        this.tN = t0;
    }

    public void StartInterval() => tN = Now;

    public void Stop() =>
        //this will cause CanSearchDeeper() and CheckTimeBudget() to evaluate to 'false'
        remaining = 0;

    internal void Go(int timePerMove)
    {
        this.Reset();
        remaining = Math.Min(timePerMove, MAX_TIME_REMAINING);
    }

    internal void Go(int time, int increment, int movesToGo)
    {
        this.Reset();
        remaining = Math.Min(time, MAX_TIME_REMAINING);
        this.increment = increment;
        this.movesToGo = movesToGo;
    }

    public bool CanSearchDeeper()
    {
        // estimate the branching factor, if only one move to go we yolo with a low estimate
        int multi = (movesToGo == 1) ? 1 : BRANCHING_FACTOR_ESTIMATE;
        int estimate = multi * this.ElapsedInterval;
        int elapsed = this.Elapsed;
        int total = elapsed + estimate;

        //no increment... we need to stay within the per-move time budget
        if (increment == 0 && total > this.TimePerMoveWithMargin)
        {
            return false;
        }

        //we have already exceeded the average move
        if (elapsed > this.TimePerMoveWithMargin)
        {
            return false;
        }

        //shouldn't spend more then the 2x the average on a move
        if (total > 2 * this.TimePerMoveWithMargin)
        {
            return false;
        }

        //can't afford the estimate
        if (total > this.TimeRemainingWithMargin)
        {
            return false;
        }

        // all conditions fulfilled
        return true;
    }

    public bool CheckTimeBudget()
    {
        if (increment == 0)
        {
            return this.Elapsed > this.TimePerMoveWithMargin;
        }
        else
        {
            return this.Elapsed > this.TimeRemainingWithMargin;
        }
    }
}
