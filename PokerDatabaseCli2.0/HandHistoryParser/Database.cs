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

  
}

public record CommandContext {
    public Database Database { get; init; }

    public CommandContext(Database database) {
        Database = database;
            }
}
