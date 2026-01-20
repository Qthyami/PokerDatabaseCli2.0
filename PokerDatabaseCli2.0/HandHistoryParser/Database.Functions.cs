
namespace PokerDatabaseCli2._0.HandHistoryParser;
//ФУНКЦИИ-РЕДЬЮСЕРЫ ДЛЯ DATABASE ЗДЕСЬ, А QERRY-ФУНКЦИИ - В САМОЙ DATABASE
public static class DatabaseFunctions {
    public static (Database NewDatabase, int AddedHandsCount)
    AddHands(this Database database, ImmutableList<HandHistory> handsToAdd){
        var existingHandIds = database.HandHistories.Select(hand => hand.HandId).ToHashSet();
        var newHands = handsToAdd.Where(hand => !existingHandIds.Contains(hand.HandId)).ToImmutableList();
        var newDatabase = database with { HandHistories = database.HandHistories.AddRange(newHands)};
        //It's necessary to actually show how many hands were imported, duplicates are skipped
        return (NewDatabase: newDatabase, AddedHandsCount: newHands.Count);
    }
    
    public static Database
    DeleteHandById(this Database database, long handId) {
        var handToDelete = database.HandHistories.FirstOrDefault(hand => hand.HandId == handId);
        if (handToDelete == null)
            throw new InvalidOperationException($"Hand with id {handId.Quoted()} not found.");

        return database with {
            HandHistories = database.HandHistories.Remove(handToDelete),
            DeletedHandsIds = database.DeletedHandsIds.Add(handId)
        };
    }
}
    


