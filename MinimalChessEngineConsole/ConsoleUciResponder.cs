namespace MinimalChessEngineConsole;

internal class ConsoleUciResponder : IUciResponder
{
    public void UciResponse(string response) 
    { 
        Console.WriteLine(response); 
    }
}
