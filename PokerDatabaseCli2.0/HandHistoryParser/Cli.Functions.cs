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

        if (commandType == null)
            throw new InvalidOperationException($"Unknown command: {commandName}");

        var ctor = commandType.GetConstructors().First(); //получили конструктор команды, который там и есть один.
        // Получаем параметры конструктора (например: DirectoryPath)
        var parameters = ctor.GetParameters();
        var parameterValuesObject = new object?[parameters.Length]; //создали массив параметров внутри конструктора, который будет иметь 1 параметр, типо DirectoryPath итд 
        for (int i = 0; i < parameters.Length; i++) {
            //получили параметр конструктора в случае аргмента directory path он {System.String DirectoryPath}
            var param = parameters[i];
            // вторая часть команды, т.е аргумент, потому что под [0] идет имя команды
            var valuePart = commandParts[i + 1]; 
            //parameterValues[0]=меняем тип (переменная, к какому типу будем приводить, "C:\Poker\1" -> Object "C:\Poker\1"  {System.String DirectoryPath}
            parameterValuesObject[i] = Convert.ChangeType(valuePart, param.ParameterType);
        }
        //возвращаем инстанс класса ... Invoke делает new AddHandsCommand("C:\\Poker\\1");
        var instance = ctor.Invoke(parameterValuesObject);
        if (instance is not ICommand comand)
            throw new InvalidOperationException($"{commandType.Name} does not implement ICommand");
            return comand;
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
            .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
           }
}


