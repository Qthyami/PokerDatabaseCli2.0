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

public static class
 Attributes {
    public static char
    GetSymbol<TEnum>(this TEnum value) where TEnum : Enum =>
        value.GetEnumAttribute<SymbolAttribute>().Value;

   public static TEnum
    ParseEnumBySymbol<TEnum>(this char symbol) where TEnum: Enum {
        var enumType = typeof(TEnum).VerifyIsEnum();
        foreach(var enumValue in Enum.GetValues(enumType).Cast<TEnum>()) 
            if (enumValue.GetSymbol() == symbol)
                return enumValue;
        throw new InvalidOperationException($"The symbol '{symbol}' does not correspond to any value of the enum {enumType.Name}");
    }

    public static TAttribute
    GetEnumAttribute<TAttribute>(this object value) 
    where TAttribute: Attribute {
        if (!value.TryGetEnumAttribute<TAttribute>(out var result))
            throw new InvalidOperationException($"The enum value {value} does not have the attribute {typeof(TAttribute).Name}");
        return result;
    }

    public static bool 
    TryGetEnumAttribute<TAttribute>(this object value, out TAttribute result) 
    where TAttribute: Attribute {
        var valueType = value.GetType().VerifyIsEnum();
        var memberInfo = value.GetType().GetMember(value.ToString()!);
        if (memberInfo.Length == 0) {
            result = null!;
            return false;
        }
        var attributes = memberInfo[0].GetCustomAttributes(typeof(TAttribute), false);
        if (attributes.Length == 0) {
            result = null!;
            return false;
        }
        result = (TAttribute)attributes[0];
        return true;
    }

    public static Type 
    VerifyIsEnum(this Type type) {
        if (!type.IsEnum)
            throw new InvalidOperationException($"The type {type.Name} is not an enum");
        return type;
    }

}







