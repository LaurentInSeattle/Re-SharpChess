namespace MinimalChessEngineConsole;

public static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Minimal Chess Engine UCI Console started.");
        var engine = new Engine(new ConsoleUciResponder());
        GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
        engine.Start();
        while (engine.Running)
        {
            string? input = await Task.Run(function: Console.ReadLine);
            if (!string.IsNullOrWhiteSpace(input))
            {
                engine.UciCommand(input);
            } 
        }

        Console.WriteLine("Minimal Chess Engine UCI Console terminated.");
    }
}
