using Plus.Communication.RCON;
using Plus.Core.FigureData;
using Plus.Core.Language;
using Plus.Core.Settings;
using Plus.Database;
using Plus.HabboHotel;
using Plus.HabboHotel.Users;

namespace Plus;

public interface IPlusEnvironment
{
    Task<bool> Start();
    void PerformShutDown();

    IGame Game { get; }
    ILanguageManager LanguageManager { get; }
    ISettingsManager SettingsManager { get; }
    IDatabase DatabaseManager { get; }
    IRconSocket RconSocket { get; }
    IFigureDataManager FigureManager { get; }
    ICollection<Habbo> CachedUsers { get; }
    bool RemoveFromCache(int id, out Habbo data);
}