using Microsoft.Extensions.Logging;
using Plus.Communication.Packets.Outgoing.Moderation;
using Plus.Core.Language;
using Plus.HabboHotel;

namespace Plus.Core;

public class ConsoleCommandHandler : IConsoleCommandHandler
{
    private readonly ILogger<ConsoleCommandHandler> _logger;
    private readonly IGame _game;
    private readonly ILanguageManager _languageManager;
    private readonly IPlusEnvironment _environment;

    public ConsoleCommandHandler(ILogger<ConsoleCommandHandler> logger,
        IGame game,
        ILanguageManager languageManager,
        IPlusEnvironment environment)
    {
        _logger = logger;
        _game = game;
        _languageManager = languageManager;
        _environment = environment;
    }

    public void InvokeCommand(string inputData)
    {
        if (string.IsNullOrEmpty(inputData))
            return;
        try
        {
            var parameters = inputData.Split(' ');
            switch (parameters[0].ToLower())
            {
                case "stop":
                case "shutdown":
                {
                    _logger.LogWarning(
                        "The server is saving users furniture, rooms, etc. WAIT FOR THE SERVER TO CLOSE, DO NOT EXIT THE PROCESS IN TASK MANAGER!!");
                    _environment.PerformShutDown();
                    break;
                }
                case "alert":
                {
                    var notice = inputData.Substring(6);
                    _game.ClientManager.SendPacket(
                        new BroadcastMessageAlertComposer(
                            $"{_languageManager.TryGetValue("server.console.alert")}\n\n{notice}"));
                    _logger.LogInformation("Alert successfully sent.");
                    break;
                }
                default:
                {
                    _logger.LogError(
                        $"{parameters[0].ToLower()} is an unknown or unsupported command. Type help for more information");
                    break;
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in command [{inputData}]");
        }
    }
}
