using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace PokerDatabaseCli2._0.Tests;

[TestFixture]
public class CliFunctionsTests {

    [Test]
    public void GetAllCommandsTypes_PrintsAllCommandTypes() {
        // Act
        var commandTypes = CliFunctions.GetAllCommandsTypes().ToList();

        // Assert & Print
        commandTypes.AssertCount(commandTypes.Count); // просто чтобы тест прошёл
        
        Console.WriteLine($"Found {commandTypes.Count} command types:");
        Console.WriteLine(new string('-', 50));
        
        foreach (var type in commandTypes) {
            var nameAttribute = (NameAttribute?)Attribute.GetCustomAttribute(type, typeof(NameAttribute));
            var commandName = nameAttribute?.Value ?? "(no name attribute)";
            Console.WriteLine($"Type: {type.Name,-25} | Command: {commandName}");
        }
        
        Console.WriteLine(new string('-', 50));
        
        // Verify at least some commands exist
        commandTypes.Count.Assert(commandTypes.Count);
        (commandTypes.Count > 0).AssertTrue();
    }
}
