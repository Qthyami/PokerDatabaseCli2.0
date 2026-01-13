namespace PokerDatabaseCli2._0.HandHistoryParser;
public interface ICommand;
public record GetLastHandsCommand(int HandCount):ICommand;

[Name("DeleteHand"), Description ("Deletes a hand by its ID from the database.")]
public record DeleteHandCommand(long HandId) :ICommand;

public class NameAttribute: Attribute {
    public string Value { get; }
    public NameAttribute(string value) => Value = value;
}
public class DescriptionAttribute : Attribute;





