using Plus.Utilities;
using Plus.Core.Settings;

namespace Plus.HabboHotel.Rooms;

public class RoomPromotion
{
    private readonly ISettingsManager _settingsManager;

    public RoomPromotion(string name, string description, int categoryId, ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
        Name = name;
        Description = description;
        TimestampStarted = UnixTimestamp.GetNow();
        TimestampExpires = UnixTimestamp.GetNow() + Convert.ToInt32(_settingsManager.TryGetValue("room.promotion.lifespan")) * 60;
        CategoryId = categoryId;
    }

    public RoomPromotion(string name, string description, double started, double expires, int categoryId)
    {
        Name = name;
        Description = description;
        TimestampStarted = started;
        TimestampExpires = expires;
        CategoryId = categoryId;
    }

    public string Name { get; set; }

    public string Description { get; set; }

    public double TimestampStarted { get; }

    public double TimestampExpires { get; set; }

    public bool HasExpired => TimestampExpires - UnixTimestamp.GetNow() < 0;

    public int MinutesLeft => Convert.ToInt32(Math.Ceiling((TimestampExpires - UnixTimestamp.GetNow()) / 60));

    public int CategoryId { get; set; }
}