using System;
using System.Collections.Generic;
using System.Text;

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
        {
            return database;
        }

        return database with
        {
            HandHistories = database.HandHistories.Remove(handToDelete),
            DeletedHandsIds = database.DeletedHandsIds.Add(handId)
        };
    }

    public static (long totalHands, long totalPlayers)
    GetDatabaseStats(this Database database)
    {
        var totalHands = database.HandHistories.Count;
         long totalPlayers = database.HandHistories.SelectMany(hand => hand.Players).DistinctBy(playerLine => playerLine.Nickname).Count();
         
        return (totalHands:totalHands, totalPlayers:totalPlayers);
    }

    public  static IEnumerable<(long HandId,  HandHistoryPlayer heroLine)>
    GetLastHeroHands(this Database database, int requiredHands) {
            var lastHeroLine = database.HandHistories
            .OrderByDescending(hand => hand.HandId)
            .Select (hand=> {
                hand.TryGetHeroPlayer(out var heroLine);
                return heroLine;
            }).FirstOrDefault(heroLine  => heroLine != null);

        if (lastHeroLine is null)
            return Enumerable.Empty<(long HandId, HandHistoryPlayer heroLine)>();
        
        return GetHeroHands(database, lastHeroLine.Nickname)
            .OrderByDescending(hand => hand.HandId)
            .Take(requiredHands);
          }

      public static IEnumerable<(long HandId, HandHistoryPlayer heroLine)>
    GetHeroHands( Database database, string heroName) {
        foreach (var hand in database.HandHistories)
        {
            var heroLine = hand.Players.FirstOrDefault(player => player.Nickname.Equals(heroName, StringComparison.OrdinalIgnoreCase));
            if (heroLine != null)
            {
                yield return (hand.HandId, heroLine);
            }
        }
    }
}
    


