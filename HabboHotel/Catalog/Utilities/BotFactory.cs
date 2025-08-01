using System.Data;
using Plus.Database;
using Plus.HabboHotel.Catalog;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users.Inventory.Bots;
using Plus.HabboHotel.Rooms.AI;

namespace Plus.HabboHotel.Catalog.Utilities;

public class BotFactory : IBotFactory
{
    private readonly ICatalogManager _catalogManager;
    private readonly IDatabase _database;

    public BotFactory(ICatalogManager catalogManager, IDatabase database)
    {
        _catalogManager = catalogManager;
        _database = database;
    }

    public Bot? CreateBot(ItemDefinition itemDefinition, int ownerId)
    {
        if (!_catalogManager.TryGetBot(itemDefinition.Id, out var catalogBot))
            return null;
        DataRow botRow;
        using (var dbClient = _database.GetQueryReactor())
        {
            dbClient.SetQuery($"INSERT INTO bots (`user_id`,`name`,`motto`,`look`,`gender`,`ai_type`) VALUES ('{ownerId}', '{catalogBot.Name}', '{catalogBot.Motto}', '{catalogBot.Figure}', '{catalogBot.Gender}', '{catalogBot.AiType}')");
            var id = Convert.ToInt32(dbClient.InsertQuery());
            dbClient.SetQuery($"SELECT `id`,`user_id`,`name`,`motto`,`look`,`gender` FROM `bots` WHERE `user_id` = '{ownerId}' AND `id` = '{id}' LIMIT 1");
            botRow = dbClient.GetRow();
        }
        return new(
            Convert.ToInt32(botRow["id"]),
            Convert.ToInt32(botRow["user_id"]),
            Convert.ToString(botRow["name"]),
            Convert.ToString(botRow["motto"]),
            Convert.ToString(botRow["look"]),
            Convert.ToString(botRow["gender"]));
    }
}
