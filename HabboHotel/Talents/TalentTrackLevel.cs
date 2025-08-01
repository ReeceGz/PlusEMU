using System.Data;
using Plus.Database;

namespace Plus.HabboHotel.Talents;

public class TalentTrackLevel
{
    private readonly Dictionary<int, TalentTrackSubLevel> _subLevels;
    private readonly IDatabase _database;

    public TalentTrackLevel(string type, int level, string dataActions, string dataGifts, IDatabase database)
    {
        Type = type;
        Level = level;
        _database = database;
        foreach (var str in dataActions.Split('|'))
        {
            if (Actions == null) Actions = new();
            Actions.Add(str);
        }
        foreach (var str in dataGifts.Split('|'))
        {
            if (Gifts == null) Gifts = new();
            Gifts.Add(str);
        }
        _subLevels = new();
        Init();
    }

    public string Type { get; set; }
    public int Level { get; set; }

    public List<string> Actions { get; }

    public List<string> Gifts { get; }

    public void Init()
    {
        DataTable getTable = null;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery("SELECT `sub_level`,`badge_code`,`required_progress` FROM `talents_sub_levels` WHERE `talent_level` = @TalentLevel");
            dbClient.AddParameter("TalentLevel", Level);
            getTable = dbClient.GetTable();
        }
        if (getTable != null)
        {
            foreach (DataRow row in getTable.Rows)
            {
                _subLevels.Add(Convert.ToInt32(row["sub_level"]),
                    new(Convert.ToInt32(row["sub_level"]), Convert.ToString(row["badge_code"]), Convert.ToInt32(row["required_progress"])));
            }
        }
    }

    public ICollection<TalentTrackSubLevel> GetSubLevels() => _subLevels.Values;
}