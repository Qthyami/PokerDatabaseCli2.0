namespace PokerDatabaseCli2._0.HandHistoryParser;

public static class HandHistoryFunctions {
    public static IEnumerable<Card>
    ParseDealtCards(this string cardsString) =>
        cardsString
            .SplitWords().Select(card => card.ParseCards());

    public static
    Card ParseCards(this string CardsString) {
        if (CardsString.Length != 2)
            throw new ArgumentException($"Invalid card string: {CardsString}");
        var rank = CardsString[0].ParseCardRank();
        var suit = CardsString[1].ParseSuit();
        return new Card(rank, suit);
    }

    public static CardRank
    ParseCardRank(this char symbol) =>
        symbol.ParseEnumBySymbol<CardRank>();

    public static Suit
    ParseSuit(this char symbol) =>
        symbol.ParseEnumBySymbol<Suit>();
}

