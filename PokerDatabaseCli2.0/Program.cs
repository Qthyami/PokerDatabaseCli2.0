using System.Windows.Input;

class Program {
    static void 

    Main(string[] args) {
        var database = Database.CreateEmpty();
        var commandTypes =CliFunctions.GetAllCommandsTypes().ToList();
        var context= new CommandContext(database);
        // Application logic here
    }
}