namespace PokerDatabaseCli2._0.Common;

public static class
Reflection {
    public static TAttribute? GetAttribute<TAttribute>(this Enum value)
        where TAttribute : Attribute {
        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString());
        if (memberInfo.Length > 0) {
            return memberInfo[0].GetCustomAttributes(typeof(TAttribute), false).FirstOrDefault() as TAttribute;
        }
        throw new ArgumentException($"Enum value '{value}' not found in type '{type.FullName}'.");
    }
}
