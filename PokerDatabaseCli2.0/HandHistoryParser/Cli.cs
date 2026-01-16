using System.Reflection.Metadata;

namespace PokerDatabaseCli2._0.HandHistoryParser;
public interface ICommand;

public record CommandContext {
    public Database Database { get; init; }
    public CommandContext(Database database) {
        Database = database;
    }
}

public class NameAttribute: Attribute {
    public string Value { get; }
    public NameAttribute(string value) => Value = value;
   
}

public class DescriptionAttribute : Attribute {
        public string Value {get;}
    public DescriptionAttribute (string value) => Value = value;
}
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
public class AliasAttribute : Attribute {
    public string Value {get;}
    public AliasAttribute (string value) => Value = value;
}

[Name("ShowStats"), Description ("Counting all hands and players in the database.")]

public record ShowStatsCommand() : ICommand;

[Name("DeleteHand"), Description ("Deletes a hand by its ID from the database.")]
public record DeleteHandCommand( [Alias("n")] [Alias("HandNumber")]  long HandId):ICommand;
   
[Name("AddHands"), Description ("Adds hand histories from a specified directory to the database.")]
public record AddHandsCommand([Alias("p")] [Alias("--Path")] string DirectoryPath) : ICommand;

[Name("GetLastHands"), Description ("Extracts the last N hands of Hero from the database.")]
public record GetLastHandsCommand([Alias("n")] int HandCount = 10) : ICommand;

[Name("ShowDeletedHands"), Description ("Displays all deleted hand IDs from the database.")]
public record ShowDeletedHandsCommand (): ICommand;


    


