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
        var (newDatabase, addedHandsCount) = context.Database.AddHands(hands);
        var result = new AddHandsResult(addedHandsCount);
        return context with { Database = newDatabase, Result = result };
    }

    public static CommandContext
    ExecuteDeleteHand(this CommandContext context, DeleteHandCommand command) {
        var newDatabase = context.Database.DeleteHandById(command.HandId);
        var result = new DeleteHandResult(command.HandId);
        return context with { Database = newDatabase, Result = result };
    }

    public static CommandContext
    ExecuteGetOverallStats(this CommandContext context, GetOverallStatsCommand command) {
        var result = new OverallStatsResult(context.Database.HandCount, context.Database.PlayersCount);
        return context with { Result = result };
    }

    public static CommandContext
    ExecuteGetLastHands(this CommandContext context, GetLastHandsCommand command) {
        var lasthands = context.Database.GetLastHeroHands(requiredHands: command.HandCount).ToImmutableList();
        var result = new LastHandsResult(lasthands);
        return context with { Result = result };
    }

    public static CommandContext
    ExecuteGetDeletedHands(this CommandContext context, ShowDeletedHandsCommand command) {
        var result = new DeletedHandsResult(context.Database.DeletedHandsIds);
        return context with { Result = result };
    }

    public static CommandContext
    ExecuteUnknownCommand(this CommandContext context, ICommand command) {
        var result = new UnknownCommandResult(command.GetType().Name);
        return context with { Result = result };
    }

    public static void
    RunCli() {
        var context = new CommandContext(Database.CreateEmpty(), result: null);
        Console.WriteLine("Welcome to Poker Database CLI! \n");
        Console.WriteLine("Type command (or 'exit')");
        while (true) {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (ShouldExit(input))
                break;
            try {
                var command = input.ParseCommand();
                context = command.ExecuteCommand(context);
                context.PrintResult();
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static bool ShouldExit(string? input) =>
    input!.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase);

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
                var attribute = type.GetAttribute<NameAttribute>();
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
            var parameterFound = parameters[i]; // сразу первый параметр и будет нужный т.е [0], цикла не будет
            var valuePart = commandParts[i + 1]; // а параметр комманды сидит в [0+1] т.е. "C:\Poker\1"
            // пихаем в [0] c конвертацией "C:\Poker\1" -> Object "C:\Poker\1" {System.String DirectoryPath}
            parameterValuesObject[i] = Convert.ChangeType(valuePart, parameterFound.ParameterType);
        }
        return parameterValuesObject;
    }

    public static void
    PrintResult(this CommandContext context) {
        if (context.Result == null) {
            Console.WriteLine("No result to display.");
            return;
        }
        switch (context.Result) {
            case AddHandsResult addHandsResult:
                Console.WriteLine($"{addHandsResult.AddedHandsCount} hands imported into the database");
                break;
            case DeleteHandResult deleteHandResult:
                Console.WriteLine($"Hand with ID: {deleteHandResult.HandId} has been deleted.");
                break;
            case OverallStatsResult overallStatsResult:
                Console.WriteLine($"Total Hands: {overallStatsResult.HandCount}, Total Players: {overallStatsResult.PlayersCount} in the database");
                break;
            case LastHandsResult lastHandsResult:
                foreach (var hand in lastHandsResult.LastHands) {
                    var heroSeatLine = hand.HeroLine;
                    var cards = string.Join(", ", heroSeatLine.DealtCards);
                    Console.WriteLine($"HandId: {hand.HandId}, Hero nickname: {heroSeatLine.Nickname}, Cards: {cards}, StackSize: {heroSeatLine.StackSize}");
                }
                break;
            case DeletedHandsResult deletedHandsResult:
                Console.WriteLine($"Deleted hands IDs: {string.Join(", ", deletedHandsResult.HandId)}");
                break;
            case UnknownCommandResult unknownCommandResult:
                Console.WriteLine($"Unknown command: {unknownCommandResult.CommandName}");
                break;
            default:
                Console.WriteLine("Unknown result type.");
                break;
        }
    }
}




