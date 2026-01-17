using System;
using System.Data;
using System.Reflection.Metadata;

namespace PokerDatabaseCli2._0.HandHistoryParser;

public static class CliFunctions {

    public static ICommand
    ParseCommand(this string inputCommand) {
        if (string.IsNullOrWhiteSpace(inputCommand))
            throw new InvalidOperationException("Empty input");

        var commandParts = inputCommand.SplitWords().ToArray();
        if (commandParts.Length == 0)
            throw new InvalidOperationException("Empty command");

        var commandName = commandParts[0];
        var commandType = GetAllCommandsTypes()
            .FirstOrDefault(type => type.GetCustomAttributes(typeof(NameAttribute), false)
            .OfType<NameAttribute>()
            .Any(attribute => attribute.Value.Equals(commandName, StringComparison.OrdinalIgnoreCase)));

        if (commandType == null)
            throw new InvalidOperationException($"Unknown command: {commandName}");

        return CreateCommandInstance(commandType: commandType, commandParts: commandParts);
    }

    public static CommandContext
    ExecuteCommand(this ICommand command, CommandContext context) =>
         command switch {
             AddHandsCommand AddHandsFromDirectory => context.ExecuteAddHands(AddHandsFromDirectory),
             DeleteHandCommand DeleteHand => context.ExecuteDeleteHand(DeleteHand),
             GetOverallStatsCommand GetOverallStats => context.ExecuteGetOverallStats(GetOverallStats),
             GetLastHandsCommand GetLastHands => context.ExecuteGetLastHands(GetLastHands),
             ShowDeletedHandsCommand ShowDeletedHands => context.ExecuteGetDeletedHands(ShowDeletedHands),
             ICommand unknown => context.ExecuteUnknownCommand(command)
         };
    
    //возвращает типы комманд DeleteHandCommand(0), т.е без Activator.CreateInstance
    public static IEnumerable<Type>
    GetAllCommandsTypes() {
        return typeof(CliFunctions)
            .Assembly
            .GetTypes()
            .Where(type => typeof(ICommand).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
    }

    public static ICommand CreateCommandInstance(Type commandType, string[] commandParts) {
        // Got the command constructor, there is only one.
        var ctor = commandType.GetConstructors().First();
        // Get constructor parameters (like : DirectoryPath)
        var parameters = ctor.GetParameters();
        // Create an array of constructor parameters, which will have 1 parameter, like DirectoryPath etc.
        var parameterValuesObject = new object?[parameters.Length];
        for (int i = 0; i < parameters.Length; i++) {
            // Got the constructor parameter, in case of directory path argument it is {System.String DirectoryPath}
            var param = parameters[i];
            // Second part of the command, i.e. argument, because [0] is the command name
            var valuePart = commandParts[i + 1];
            // parameterValues[0] = change type (variable, what type we will convert to) "C:\Poker\1" -> Object "C:\Poker\1" {System.String DirectoryPath}
            // and put the obtained value into an empty object at index 0
            parameterValuesObject[i] = Convert.ChangeType(valuePart, param.ParameterType);
        }
        // Return an instance of the class ... Invoke does new AddHandsCommand("C:\\Poker\\1");
        var instance = ctor.Invoke(parameterValuesObject);
        if (instance is not ICommand comand)
            throw new InvalidOperationException($"{commandType.Name} does not implement ICommand");
        return comand;
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
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    private static bool ShouldExit(string? input) =>
    input == null || input.Trim().Equals("exit", StringComparison.OrdinalIgnoreCase);

    public static string
    GetCommandName(this ICommand command) {
        var type = command.GetType();
        var nameAttribute = (NameAttribute?)Attribute.GetCustomAttribute(type, typeof(NameAttribute));
        return nameAttribute?.Value ?? type.Name;
    }
}




