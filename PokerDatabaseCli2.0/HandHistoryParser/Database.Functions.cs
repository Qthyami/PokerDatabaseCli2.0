
namespace PokerDatabaseCli2._0.HandHistoryParser;

    public static class DatabaseaFunctions
{
    public static Database
    AddHands(this Database database, ImmutableList<HandHistory> handsToAdd)
    {
        return database with { HandHistories = database.HandHistories.AddRange(handsToAdd) };
    }

    public static Database
    DeleteHandById(this Database database, long handId) {
        var handToDelete = database.HandHistories.FirstOrDefault(hand => hand.HandId == handId);
        if (handToDelete is null)
            return database;

        return database with
        {
            HandHistories = database.HandHistories.Remove(handToDelete),
            DeletedHandsIds = database.DeletedHandsIds.Add(handId)
        };
    }
}
    


