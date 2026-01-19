using System.Data;
using System.Reflection;

namespace PokerDatabaseCli2._0.HandHistoryParser;

public static class CliFunctions {

    public static ICommand
    ParseCommand(this string inputCommand) {
        inputCommand.ValidateInput();
        var commandParts = inputCommand.SplitCommand();
        var commandName = commandParts.GetCommandNameFromParts();
        var commandType = commandName.FindCommandType();

        return CreateCommandInstance(commandType, commandParts);
    }

    public static CommandContext
    ExecuteCommand(this ICommand command, CommandContext context) =>
         command switch {
             AddHandsCommand AddHandsFromDirectory => context.ExecuteAddHands(AddHandsFromDirectory),
             DeleteHandCommand DeleteHand => context.ExecuteDeleteHand(DeleteHand),
             GetOverallStatsCommand GetOverallStats => context.ExecuteGetOverallStats(GetOverallStats),
             GetLastHandsCommand GetLastHands => context.ExecuteGetLastHands(GetLastHands),
             ShowDeletedHandsCommand ShowDeletedHands => context.ExecuteGetDeletedHands(ShowDeletedHands),
             _ => context.ExecuteUnknownCommand(command)
         };

    public static IEnumerable<Type>
    GetAllCommandsTypes() {
        return typeof(CliFunctions)
            .Assembly
            .GetTypes()
            .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
    }

    public static CommandContext
    ExecuteAddHands(this CommandContext context, AddHandsCommand command) {
        var hands = command.DirectoryPath.GetHandHistoriesFromDirectory().ToImmutableList();
        var newDatabase = context.Database.AddHands(hands);
        return context with { Database = newDatabase };
    }

    public static CommandContext
    ExecuteDeleteHand(this CommandContext context, DeleteHandCommand command) {
        var newDatabase = context.Database.DeleteHandById(command.HandId);
        Console.WriteLine($"Hand with number: {command.HandId} has been deleted.");
        return context with { Database = newDatabase };
    }

    public static CommandContext
    ExecuteGetOverallStats(this CommandContext context, GetOverallStatsCommand command) {
        Console.WriteLine($"Total Hands: {context.Database.HandCount}, Total Players: {context.Database.PlayersCount} in the database");
        return context;
    }
    public static CommandContext
    ExecuteGetLastHands(this CommandContext context, GetLastHandsCommand command) {
        var lasthands = context.Database.GetLastHeroHands(requiredHands: command.HandCount);
        foreach (var (handId, heroSeatLine) in lasthands) {
            var cards = string.Join(" ", heroSeatLine.DealtCards.Select(card => card.ToString()));
            Console.WriteLine($"HandId: {handId}, Hero nickname: {heroSeatLine.Nickname}, Cards: {cards}, StackSize: {heroSeatLine.StackSize}");
        }
        return context;
    }

    public static CommandContext
    ExecuteGetDeletedHands(this CommandContext context, ShowDeletedHandsCommand command) {
        Console.WriteLine($"Deleted hands Numbers:{string.Join(", ", context.Database.DeletedHandsIds)}");
        return context;
    }

    public static CommandContext
    ExecuteUnknownCommand(this CommandContext context, ICommand command) {
        Console.WriteLine($"Unknown command: {command.GetType().Name}");
        return context;
    }

    public static void
    RunCli() {
        var context = new CommandContext(Database.CreateEmpty());
        Console.WriteLine("Welcome to Poker Database CLI! \n");
        Console.WriteLine("Type command (or 'exit')");
        while (true) {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (ShouldExit(input))
                break;
            try {
                var command = input.ParseCommand();
                context = command.ExecuteCommand(context); //ExecuteCommand(command, context) читабельнее
                //проблема такого синтаксиса, что читающий не понимает, есть ли у command метод ExecuteCommand
                // или это у ExecuteCommand 2 параметра
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static bool ShouldExit(string? input) =>
    input!.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase);

    public static string
    GetCommandName(this ICommand command) {
        var type = command.GetType();
        var nameAttribute = type.GetAttribute<NameAttribute>();
        return nameAttribute?.Value ?? type.Name;
    }

    private static void
    ValidateInput(this string input) {
        if (string.IsNullOrWhiteSpace(input))
            throw new InvalidOperationException("Empty input");
    }

    private static string[]
    SplitCommand(this string input) => input.SplitWords().ToArray();

    private static string
    GetCommandNameFromParts(this string[] parts) {
        if (parts.Length == 0)
            throw new InvalidOperationException("Empty command");
        return parts[0];
    }

    private static Type
    FindCommandType(this string commandName) {
        var type = GetAllCommandsTypes()
            .FirstOrDefault(type => {
                var attribute= type.GetAttribute<NameAttribute>();
                return attribute != null && attribute.Value.Equals(commandName, StringComparison.OrdinalIgnoreCase);
            });
         
        if (type == null)
            throw new InvalidOperationException($"Unknown command: {commandName}");
        return type;
    }

    public static ICommand
    CreateCommandInstance(this Type commandType, string[] commandParts) {
        var constructor = commandType.GetMainConstructor();

        var parameters = constructor.GetParameters();
        var parameterValuesObject = parameters.GetParameterValues(commandParts);

        var instance = constructor.Invoke(parameterValuesObject);
        if (instance is not ICommand command)
            throw new InvalidOperationException($"{commandType.Name} does not implement ICommand");

        return command;
    }

    private static ConstructorInfo
    GetMainConstructor(this Type type) => type.GetConstructors().First();

    private static object?[]
    GetParameterValues(this ParameterInfo[] parameters, string[] commandParts) {
        if (commandParts.Length - 1 < parameters.Length)
            throw new InvalidOperationException(
                $"Expected {parameters.Length} arguments, got {commandParts.Length - 1}");
        // Create an array of constructor parameters, which will have 1 parameter, like DirectoryPath, now empty
        var parameterValuesObject = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++) {
            var param = parameters[i]; // сразу первый параметр и будет нужный т.е [0]
            var valuePart = commandParts[i + 1]; // а параметр комманды сидит в [0+1] т.е. "C:\Poker\1"
            // пихаем в [0] c конвертацией "C:\Poker\1" -> Object "C:\Poker\1" {System.String DirectoryPath}
            parameterValuesObject[i] = Convert.ChangeType(valuePart, param.ParameterType);
        }
        return parameterValuesObject;
    }
}




