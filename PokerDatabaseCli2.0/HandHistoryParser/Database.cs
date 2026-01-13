using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PokerDatabaseCli2._0.HandHistoryParser;

public record
Database {
    public ImmutableList<HandHistory> HandHistories {get; init;}
    public ImmutableList<long> DeletedHandsIds {get ; init;}

    public static Database CreateEmpty() => new Database {
        HandHistories = ImmutableList<HandHistory>.Empty,
        DeletedHandsIds = ImmutableList<long>.Empty
    };

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

public record CommandContext {
    public Database Database { get; }
    public CommandContext(Database database) {
        Database = database;
    }
}
