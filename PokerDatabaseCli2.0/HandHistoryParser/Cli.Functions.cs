using System.Reflection.Metadata;

namespace PokerDatabaseCli2._0.HandHistoryParser;

public static class CliFunctions {
    public static string
    GetCommandName(this ICommand command) {
        var type = command.GetType();
        var nameAttribute = (NameAttribute?)Attribute.GetCustomAttribute(type, typeof(NameAttribute));
        return nameAttribute?.Value ?? type.Name;
    }
    public static ICommand
    ParseCommand(this string inputCommand) {
        if (string.IsNullOrWhiteSpace(inputCommand))
            throw new InvalidOperationException("Empty input");
        var commandParts = inputCommand.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (commandParts.Length == 0)
            throw new InvalidOperationException("Empty command");

        var commandName = commandParts[0];
        var commandType = GetAllCommandsTypes()
            .FirstOrDefault(type => type.GetCustomAttributes(typeof(NameAttribute), false)
                .OfType<NameAttribute>()
                .Any(attribute => attribute.Value.Equals(commandName, StringComparison.OrdinalIgnoreCase)));
        
        var ctor = commandType.GetConstructors().First(); //получили конструктор, который там и есть один.
        var parameterValues = new object?[ctor.GetParameters().Length]; //создали массив параметров внутри конструктора, который будет 1, и которы сейчас вообще [null]
        for (int i = 0; i < ctor.GetParameters().Length; i++) {
            var param = ctor.GetParameters()[i]; //получили параметр конструктора
            var aliasAttribute = (AliasAttribute?)Attribute.GetCustomAttribute(param, typeof(AliasAttribute)); //тут будет "n"
            if (aliasAttribute == null)
                continue; //если атрибута нет, то пропускаем
            for (int j = 1; j < commandParts.Length; j++) { //перебираем все части команды, начиная со второй, т.к. первая это первое слово в команде
                var part = commandParts[j]; // дальше тупо сравниваем 
                if (part.Equals("-" + aliasAttribute.Value, StringComparison.OrdinalIgnoreCase)) {
                    if (j + 1 >= commandParts.Length)
                        throw new InvalidOperationException($"No value provided for {part}");
                    var valuePart = commandParts[j + 1];//после сошедшейся проверки берем второе слово в команде
                    //конвертируем в нужный тип
                    object? value = Convert.ChangeType(valuePart, param.ParameterType);
                    parameterValues[i] = value;
                    break;
                }
            }

        }
        return (ICommand)ctor.Invoke(parameterValues);
    }

    public static CommandContext
    ExecuteCommand(this ICommand command, CommandContext context) {
        switch (command) {
            case AddHandsCommand AddHandsFromDirectory: {
                    var hands = AddHandsFromDirectory.DirectoryPath.GetHandHistoriesFromDirectory().ToImmutableList();
                    var newDatabase = context.Database.AddHands(hands);
                    return context with { Database = newDatabase };
                }

            case DeleteHandCommand DeleteHand: {
                    var newDatabase = context.Database.DeleteHandById(DeleteHand.HandId);
                    Console.WriteLine($"Hand with number: {DeleteHand.HandId} has been deleted.");
                    return context with { Database = newDatabase };
                }

            case ShowStatsCommand GetOverallStats: {
                    Console.WriteLine($"Total Hands: {context.Database.HandCount}, Total Players: {context.Database.PlayersCount} in the database");
                    return context;
                }

            case GetLastHandsCommand GetLastHeroHands: {
                    var lasthands = context.Database.GetLastHeroHands(requiredHands: GetLastHeroHands.HandCount);
                    foreach (var (handId, heroSeatLine) in lasthands) {
                        var cards = string.Join(" ", heroSeatLine.DealtCards.Select(card => card.ToString()));
                        Console.WriteLine($"HandId: {handId}, Hero nickname: {heroSeatLine.Nickname}, Cards: {cards}, StackSize: {heroSeatLine.StackSize}");
                    }
                    return context;
                }

            case ShowDeletedHandsCommand ShowDeletedHands: {
                    Console.WriteLine($"Deleted hands Numbers:{string.Join(", ", context.Database.DeletedHandsIds)}");
                    return context;
                }

            default:
                Console.WriteLine($"Unknown command: {command.GetType().Name}");
                return context;
        }
    }
    //возвращает типы комманд DeleteHandCommand(0), т.е без Activator.CreateInstance
    public static IEnumerable<Type>
    GetAllCommandsTypes() {
        return typeof(CliFunctions)
            .Assembly
            .GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
        //.Select(t => (ICommand)Activator.CreateInstance(t)!);
    }


}


