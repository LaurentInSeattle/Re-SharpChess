namespace MinimalChessEngine;

public sealed partial class Engine(IUciResponder uciResponder) : IUciRequester
{
    private readonly IUciResponder uciResponder = uciResponder;
    private readonly TimeControl time = new();
    private readonly List<Board> history = [];

    private Board board = new(Board.STARTING_POS_FEN);

    private IterativeSearch? iterativeSearch ;
    private Thread? searchingThread ;
    private Move bestMove = default;
    private int maxSearchDepth;

    public bool Running { get; private set; }

    public PlayerColor SideToMove => board.SideToMove;

    public Board Board => this.board;

    public void Start()
    {
        this.Stop();
        this.Running = true;
    }

    public void Quit()
    {
        this.Stop();
        this.Running = false;
    }

    public void SetupPosition(Board board)
    {
        this.Stop();

        // make a deep copy
        this.board = new Board(board);
        this.history.Clear();
        this.history.Add(new Board(this.board));
    }

    public void Play(Move move)
    {
        this.Stop();
        this.board.Play(move);
        this.history.Add(new Board(board));

        // Emit an info string message providing all legal moves in the new current position
        string legalMoves = this.LegalMoves();
        this.RespondUci(string.Format("info string legal: {0}", legalMoves));

        // TODO: Emit an info string message about the check status in the new current position
        // TODO: Emit an info string message about the castle status in the new current position
        // TODO: Emit an info string message about the squares under attack in the new current position
    }

    public void Go(int maxDepth, int maxTime, long maxNodes)
    {
        this.Stop();
        this.time.Go(maxTime);
        this.StartSearch(maxDepth, maxNodes);
    }

    public void Go(int maxTime, int increment, int movesToGo, int maxDepth, long maxNodes)
    {
        this.Stop();
        this.time.Go(maxTime, increment, movesToGo);
        this.StartSearch(maxDepth, maxNodes);
    }

    public void Stop()
    {
        if (searchingThread != null)
        {
            // this will cause the thread to terminate via CheckTimeBudget
            time.Stop();
            searchingThread.Join();
            searchingThread = null;
        }
    }

    private void StartSearch(int maxDepth, long maxNodes)
    {
        // do the first iteration. it's cheap, no time check, no thread
        this.UciLog($"Search scheduled to take {time.TimePerMoveWithMargin}ms!");

        //add all history positions with a score of 0 (Draw through 3-fold repetition) and freeze them by setting a depth that is never going to be overwritten
        foreach (var position in history)
        {
            Transpositions.Store(position.ZobristHash, Transpositions.HISTORY, 0, SearchWindow.Infinite, 0, default);
        }

        iterativeSearch = new IterativeSearch(board, maxNodes);
        time.StartInterval();
        iterativeSearch.SearchDeeper();
        this.Collect();

        // start the search thread
        this.maxSearchDepth = maxDepth;
        this.searchingThread = new Thread(this.Search) { Priority = ThreadPriority.BelowNormal };
        this.searchingThread.Start();
    }

    private void Search()
    {
        while (this.CanSearchDeeper())
        {
            time.StartInterval();
            if (iterativeSearch is null)
            {
                break;
            }

            iterativeSearch.SearchDeeper(time.CheckTimeBudget);

            //aborted?
            if (iterativeSearch.Aborted)
            {
                break;
            }

            //collect PV
            this.Collect();
        }

        // Done searching!
        this.BestMove(bestMove);

        iterativeSearch = null;
    }

    private bool CanSearchDeeper()
    {
        // max depth reached or game over?
        if (iterativeSearch is null)
        {
            return false;
        }

        if (iterativeSearch.Depth >= maxSearchDepth)
        {
            return false;
        }

        //otherwise it's only time that can stop us!
        return time.CanSearchDeeper();
    }

    private void Collect()
    {
        if (iterativeSearch is null)
        {
            return;
        }

        bestMove = iterativeSearch.PrincipalVariation[0];

        this.Info(
            depth:  iterativeSearch.Depth, 
            score:  (int)this.SideToMove * iterativeSearch.Score, 
            nodes:  iterativeSearch.NodesVisited, 
            timeMs: time.Elapsed, 
            pv:     this.GetPrintablePV(iterativeSearch.PrincipalVariation, iterativeSearch.Depth)
        );
    }

    private Move[] GetPrintablePV(Move[] pv, int depth)
    {
        List<Move> result = [.. pv];

        //Try to extend from TT to reach the desired depth?
        if (result.Count < depth)
        {
            var position = new Board(board);
            foreach (Move move in pv)
            {
                position.Play(move);
            }

            while (result.Count < depth && Transpositions.GetBestMove(position, out Move move))
            {
                position.Play(move);
                result.Add(move);
            }
        }

        return [.. result];
    }

}
