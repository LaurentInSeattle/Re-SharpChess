namespace MinimalChess; 

internal struct KillSwitch
{
    private readonly Func<bool>? killSwitch;
    private bool aborted;

    public KillSwitch(Func<bool>? killSwitch = null)
    {
        this.killSwitch = killSwitch;
        this.aborted = this.killSwitch != null && this.killSwitch();
    }

    public bool Get(bool update)
    {
        if (!this.aborted && update && this.killSwitch != null)
        {
            this.aborted = this.killSwitch();
        }

        return this.aborted;
    }
}
