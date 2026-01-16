using System.Windows.Input;

class Program {
    static void 

    Main(string[] args) {
        var context= new CommandContext(Database.CreateEmpty());
         Console.WriteLine("Welcome to Poker Database CLI! \n");
        Console.WriteLine("Type command (or 'exit')");

        
        while (true) {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (input == null || input.Trim().ToLower() == "exit") {
                break;
            }
            try {
                var command= input.ParseCommand();
                context= command.ExecuteCommand(context);
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }




        // Application logic here
    }
}