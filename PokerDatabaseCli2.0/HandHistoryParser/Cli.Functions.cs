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
    ParseCommand (string inputCommand) {
    }
    
    public static void
    ExecuteCommand (ICommand command, CommandContext context) {

    }

    public static IEnumerable<ICommand>
    GetAllCommands() {
        return typeof(CliFunctions)
            .Assembly
            .GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => (ICommand)Activator.CreateInstance(t)!);
    }


    }


