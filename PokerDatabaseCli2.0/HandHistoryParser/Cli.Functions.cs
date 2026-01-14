using System;
using System.Collections.Generic;
using System.Text;

namespace PokerDatabaseCli2._0.HandHistoryParser;

public static class CliFunctions {
    public static string
    GetCommandName(this ICommand command) {
        var type = command.GetType();
        var nameAttribute = (NameAttribute?)Attribute.GetCustomAttribute(type, typeof(NameAttribute));
        return nameAttribute?.Value ?? type.Name;
    }
    public static ICommand
    ParseCommand (this string inputCommand) {
        var commandName = inputCommand.Split (' ', StringSplitOptions.RemoveEmptyEntries)[0];
        var commandType = GetAllCommandsTypes()
            .FirstOrDefault(type => type.GetCustomAttributes(typeof(NameAttribute), false)
                .OfType<NameAttribute>()
                .Any(anyType=>anyType.Value.Equals(commandName, StringComparison.OrdinalIgnoreCase)));
          
                if (commandType == null)
            throw new InvalidOperationException($"Unknown command: {commandName}");
                return (ICommand)Activator.CreateInstance(commandType)!;
            }




    public static CommandContext
    ExecuteCommand(this ICommand command, CommandContext context) {
        switch (command) {
            case AddHandsCommand addHands: {
                    var hands = addHands.DirectoryPath.GetHandHistoriesFromDirectory().ToImmutableList();
                    var newDatabase = context.Database.AddHands(hands);
                    return context with { Database = newDatabase };
                }
                case DeleteHandCommand deleteHand: {
                var newDatabase = context.Database.DeleteHandById(deleteHand.HandId);
                    return context with { Database = newDatabase };
                }
                default:
            {
                Console.WriteLine($"Uncnown command: {command.GetType().Name})");
                    return context;
                }


        }

    }

    public static IEnumerable<Type> //возвращает инстансы комманд DeleteHandCommand(0) итд а не типы
    GetAllCommandsTypes() {
        return typeof(CliFunctions)
            .Assembly
            .GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);
            //.Select(t => (ICommand)Activator.CreateInstance(t)!);
    }


    }


