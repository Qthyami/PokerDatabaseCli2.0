using System.Reflection;

namespace PokerDatabaseCli2._0.Common;

public class
SymbolAttribute : Attribute {
    public char Value { get; }
    public SymbolAttribute(char symbol) => Value = symbol;
}

public class NameAttribute: Attribute {
    public string Value { get; }
    public NameAttribute(string value) => Value = value;
}

public class DescriptionAttribute : Attribute {
    public string Value {get;}
    public DescriptionAttribute (string value) => Value = value;
}









