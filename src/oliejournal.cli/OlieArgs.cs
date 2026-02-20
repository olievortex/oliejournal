namespace oliejournal.cli;

public class OlieArgs
{
    public CommandsEnum Command { get; private set; }

    public enum CommandsEnum
    {
        AudioProcessQueue,
        DeleteOldContent
    }

    public OlieArgs(string[] args)
    {
        if (args.Length == 0) throw new ArgumentException("The command name is missing");

        var command = args[0].ToLower();

        Command = command switch
        {
            "audioprocessqueue" => CommandsEnum.AudioProcessQueue,
            "deleteoldcontent" => CommandsEnum.DeleteOldContent,
            _ => throw new ArgumentException($"Unknown command {command}")
        };
    }
}