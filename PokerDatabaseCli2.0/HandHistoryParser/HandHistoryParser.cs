namespace PokerDatabaseCli2._0.HandHistoryParser;

public static class
PokerStarsHandHistoryParser {
    public static IEnumerable<HandHistory>
    GetHandHistoriesFromDirectory(this string directory) => 
        directory
            .VerifyDirectoryExists()
            .GetAllDirectoryFiles()
            .WithFileExtension(".txt")
            .SelectMany(file => file.GetHandHistoriesFromFile());
    
    public static IEnumerable<HandHistory>
    GetHandHistoriesFromFile(this string file) => 
        file.GetHandHistoriesTextFromFile().Select(text => text.ParseHandHistory());
        
  public static IEnumerable<string>
    GetHandHistoriesTextFromFile(this string filePath) =>
        filePath.GetAllTextFromFile().GetLines().SplitByEmptyLines();

    public static HandHistory
    ParseHandHistory(this string handHistoryText) {
        var parser = new FluentParser(handHistoryText);
        var handId = parser.ParseHandId();
        var players = parser.ParsePlayers().ToImmutableList();
        var (heroNickname, dealtCardsString) = parser.ParseHeroAndCards();
        var heroCards = dealtCardsString.ParseDealtCards().ToImmutableList();
        return new HandHistory(
           handId: handId,
    players: [..players.Select(player =>
        new SeatLine(
            seatNumber: player.seatNumber,
            nickName: player.nickName,
            stackSize: player.stackSize,
            dealtCards: player.nickName == heroNickname
                ? heroCards
                : ImmutableList<Card>.Empty))]);
    }

    public static long
    ParseHandId(this FluentParser parser) {
        if (!parser.TrySkipUntil("PokerStars Hand #"))
            throw new FormatException("Not a valid PokerStars hand history.");
        return parser.Skip("PokerStars Hand #".Length).ReadLong();
        }

    public static IEnumerable<(int seatNumber, string nickName, double stackSize)>
    ParsePlayers(this FluentParser parser) {
        if (!parser.TrySkipUntil("\nSeat "))
            yield break;
        parser.Skip("#".Length);
        while (parser.TryParseNextSeat(out var player)) {
            yield return player;
        }
    }

    public static (int seatNumber, string nickName, double stackSize)
    ParseSeatLine(this FluentParser parser) {
        var seatNumber = parser.ReadSeatNumber();
        var nickName = parser.ReadPlayerNick();
        var stackSize = parser.ReadPlayerStack();
        return (
            seatNumber: seatNumber,
            nickName: nickName,
            stackSize: stackSize);
    }

    public static (string heroNickName, string cards)
    ParseHeroAndCards(this FluentParser parser) {
        parser.SkipToHoleCards();
        var heroNickName = parser.ReadHeroNickname();
        var dealtCards = parser.ReadHeroCards();
        return (heroNickName: heroNickName, cards: dealtCards);
    }

    private static bool
    TryParseNextSeat(this FluentParser parser, out (int seatNumber, string nickName, double stackSize) result) {
        result = default;
        if (!parser.Next("Seat "))
            return false;
        var lookahead = parser.Clone();
        if (!lookahead.TryReadUntil('\n', int.MaxValue, out var line) || !line.Contains("in chips"))
            return false;
        result = parser.ParseSeatLine();
        if (parser.HasNext)
            parser.SkipUntilNextLine();
        return true;
    }

    private static void
    SkipToHoleCards(this FluentParser parser) {
        if (!parser.TrySkipUntil("*** HOLE CARDS ***"))
            throw new FormatException("HOLE CARDS section not found");
        parser.Skip("*** HOLE CARDS ***".Length).SkipUntilNextLine();
    }

    private static string
    ReadHeroNickname(this FluentParser parser) {
        parser.VerifyNext("Dealt to ").Skip("Dealt to ".Length);
        if (!parser.TryReadWord(out var heroNick))
            throw new FormatException("Failed to read hero nickname");
        return heroNick;
    }

    private static string
    ReadPlayerNick(this FluentParser parser) {
        parser.VerifyNext(": ").Skip(2).SkipSpaces();
        var nick = parser.ReadUntil('(').TrimEnd();
        if (string.IsNullOrEmpty(nick))
            throw new FormatException("Failed to read player's nickname");
        return nick;
    }

    private static string
    ReadHeroCards(this FluentParser parser) =>
        parser.SkipSpaces().VerifyNext("[").Skip(1).ReadUntil(']');

    private static int
    ReadSeatNumber(this FluentParser parser) {
        parser.VerifyNext("Seat ").Skip("Seat ".Length);
        return parser.ReadInt();
    }

    private static double
    ReadPlayerStack(this FluentParser parser) {
        parser.VerifyNext("(").Skip(1).SkipSpaces().VerifyNext("$").Skip(1);
        return parser.ReadDouble();
    }
}
