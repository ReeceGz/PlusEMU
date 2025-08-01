using Plus.HabboHotel.Items;
using Plus.HabboHotel.Users.Inventory.Bots;
using Plus.HabboHotel.Rooms.AI;
using Plus.Utilities.DependencyInjection;

namespace Plus.HabboHotel.Catalog.Utilities;

[Singleton]
public interface IBotFactory
{
    Bot? CreateBot(ItemDefinition itemDefinition, int ownerId);
}
