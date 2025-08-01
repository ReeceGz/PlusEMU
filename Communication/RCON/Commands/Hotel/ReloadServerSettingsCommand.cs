using Plus.Core.Settings;

namespace Plus.Communication.RCON.Commands.Hotel;

internal class ReloadServerSettingsCommand : IRconCommand
{
    public string Description => "This command is used to reload the server settings.";

    public string Key => "reload_server_settings";
    public string Parameters => "";
    private readonly ISettingsManager _settingsManager;

    public ReloadServerSettingsCommand(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    public Task<bool> TryExecute(string[] parameters)
    {
        _settingsManager.Reload();
        return Task.FromResult(true);
    }
}