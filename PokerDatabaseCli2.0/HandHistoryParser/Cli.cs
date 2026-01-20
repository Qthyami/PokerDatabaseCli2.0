namespace PokerDatabaseCli2._0.HandHistoryParser;

public interface ICommand;
public interface IResult;

public record CommandContext {
    public Database Database { get; init; }
    public IResult? Result { get; init; }

    public CommandContext(Database database, IResult? result) {
        Database = database;
        Result = result;
    }
}

[Name("ShowStats"), Description("Counting all hands and players in the database.")]
public record GetOverallStatsCommand() : ICommand;

[Name("DeleteHand"), Description("Deletes a hand by its ID from the database.")]
public record DeleteHandCommand(long HandId) : ICommand;

[Name("AddHands"), Description("Adds hand histories from a specified directory to the database.")]
public record AddHandsCommand(string DirectoryPath) : ICommand;

[Name("GetLastHands"), Description("Extracts the last N hands of Hero from the database.")]
public record GetLastHandsCommand(int HandCount = 10) : ICommand;

[Name("ShowDeletedHands"), Description("Displays all deleted hand IDs from the database.")]
public record ShowDeletedHandsCommand() : ICommand;

//Results
public record OverallStatsResult(long HandCount, long PlayersCount) : IResult;
public record LastHandsResult(ImmutableList<(long HandId, SeatLine HeroLine)> LastHands) : IResult;
public record AddHandsResult(int AddedHandsCount) : IResult;
public record ConsoleResult(string Text) : IResult;
public record DeleteHandResult(long HandId) : IResult;
public record DeletedHandsResult(ImmutableList<long> HandId) : IResult;
public record UnknownCommandResult(string CommandName) : IResult;





