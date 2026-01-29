using System.Data;
using System.Reflection;

namespace PokerDatabaseCli2._0.HandHistoryParser;

public static class CliFunctions {
    public static IEnumerable<Type>
      AllCommandsTypes = typeof(CliFunctions)
              .Assembly
              .GetTypes()
              .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);


    public static ICommand
    ParseCommand(this string inputCommand) {
        inputCommand.ValidateInput();
        var commandParts = inputCommand.SplitCommand();
        var commandName = commandParts.GetCommandNameFromParts();
        var commandType = commandName.FindCommandType();

        return CreateCommandInstance(commandType, commandParts);
    }

    public static CommandEnvironment
    ExecuteCommand(this ICommand execuingCommand, CommandEnvironment context) =>
         execuingCommand switch {
             AddHandsCommand command => context.ExecuteAddHands(command),
             DeleteHandCommand command => context.ExecuteDeleteHand(command),
             GetOverallStatsCommand command => context.ExecuteGetOverallStats(command),
             GetLastHandsCommand command => context.ExecuteGetLastHands(command),
             ShowDeletedHandsCommand command => context.ExecuteGetDeletedHands(command),
             _ => context.ExecuteUnknownCommand(execuingCommand)
         };

    public static CommandEnvironment
    ExecuteAddHands(this CommandEnvironment context, AddHandsCommand command) {
        var hands = command.DirectoryPath.GetHandHistoriesFromDirectory().ToImmutableList();
        var (newDatabase, addedHandsCount) = context.Database.AddHands(hands);
        //var result = new AddHandsResult(addedHandsCount);
        context.WriteOutput($"{addedHandsCount} hands imported into the database");
        return context.WithDatabase(newDatabase);
    }

    public static CommandEnvironment
    ExecuteDeleteHand(this CommandEnvironment context, DeleteHandCommand command) {
        var newDatabase = context.Database.DeleteHandById(command.HandId);
        context.WriteOutput($"Hand with ID: {command.HandId} has been deleted.");
        return context.WithDatabase(newDatabase);

    }

    public static CommandEnvironment
    ExecuteGetOverallStats(this CommandEnvironment context, GetOverallStatsCommand command) {
        context.WriteOutput($"Total Hands: {context.Database.HandCount}, Total Players: {context.Database.PlayersCount} in the database");
        return context;
    }

    public static CommandEnvironment
    ExecuteGetLastHands(this CommandEnvironment context, GetLastHandsCommand command) {
        foreach (var (hand, hero) in context.Database.GetLastHeroHandsWithHero(command.HandCount)) {
            context.WriteOutput(
                $"Hand {hand.HandId} | " +
                $"Hero: {hero.Nickname} | " +
                $"Cards: {string.Join(", ", hero.DealtCards)} | " +
                $"Stack: {hero.StackSize}"
            );
        }
        return context;
    }

    public static CommandEnvironment
    ExecuteGetDeletedHands(this CommandEnvironment context, ShowDeletedHandsCommand command) {
        context.WriteOutput($"Deleted hands IDs: {string.Join(", ", context.Database.DeletedHandsIds)}");
        return context;
    }

    public static CommandEnvironment
    ExecuteUnknownCommand(this CommandEnvironment context, ICommand command) {
        context.WriteOutput($"Unknown command: {command}");
        return context;
    }

    public static void
    RunCli() {
        var context = new CommandEnvironment(Database.Empty());
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
        var type = AllCommandsTypes
            .FirstOrDefault(type =>
                type.TryGetAttribute<NameAttribute>(out var attr)
                && attr.Value.Equals(commandName, StringComparison.OrdinalIgnoreCase)
            );

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
        // use simple positional reader similar to FluentParser instead of ad-hoc index math
        if (commandParts.Length - 1 < parameters.Length)
            throw new InvalidOperationException(
                $"Expected {parameters.Length} arguments, got {commandParts.Length - 1}");

        var reader = new CommandPartsReader(commandParts, 1); // skip command name
        var parameterValuesObject = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++) {
            var parameter = parameters[i];
            var rawValue = reader.ReadNext();
            parameterValuesObject[i] = Convert.ChangeType(rawValue, parameter.ParameterType);
        }

        return parameterValuesObject;
    }

 

    public static class OutputDestination {
        public static void WriteOutput(string message) {
        Console.WriteLine(message);
    }
    }
}







