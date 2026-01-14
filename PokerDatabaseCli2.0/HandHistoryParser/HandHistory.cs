namespace PokerDatabaseCli2._0.HandHistoryParser;

public class
HandHistory {
    public long HandId { get; }
    public ImmutableList<HandHistoryPlayer> Players { get; }
    public HandHistory(long handId, ImmutableList<HandHistoryPlayer> players) {
        if (players.Count < 2)
            throw new InvalidOperationException("There must be at least two players");
        HandId = handId;
        Players = players;
    }
        public HandHistoryPlayer 
    GetPlayer(string nickname) => 
        Players.TryGet(player => player.Nickname == nickname, out var result) ? result 
            : throw new InvalidOperationException($"Player {nickname.Quoted()} not found in hand {HandId.Quoted()}");

    public bool ContainsPlayer(string nickname) => Players.Any(player => player.Nickname == nickname);
    public IEnumerable<string> PlayerNicknames => Players.Select(player => player.Nickname);


    public bool TryGetHeroPlayer(out HandHistoryPlayer heroPlayer) {
        heroPlayer = Players.First(player => player.DealtCards.Count > 0);
        return true;
    }
    public override string ToString() =>
        $"HandId={HandId}, Players={Players.Count}";
}

public class
HandHistoryPlayer {
    public int SeatNumber { get; }
    public string Nickname { get; }
    public double StackSize { get; }
    public ImmutableList<Card> DealtCards { get; }
    public HandHistoryPlayer(int seatNumber, string nickName, double stackSize, ImmutableList<Card> dealtCards) {
        SeatNumber = seatNumber;
        Nickname = nickName;
        StackSize = stackSize;
        DealtCards = dealtCards;
    }
    public override string ToString() =>
        $"Seat {SeatNumber}: {Nickname} (${StackSize}) cards: {string.Join(' ', DealtCards.Select(c => c.ToString()))}";
}

// public class
//Card {
//    public CardRank Rank { get; }
//    public Suit Suit { get; }
//    public Card(CardRank rank, Suit suit) {
//        Rank = rank;
//        Suit = suit;
//    }
//    public override string
//    ToString() => $"{Rank.GetSymbol()}{Suit.GetSymbol()}";
//}
//public enum
//CardRank {
//    [Symbol('2')] Two = 2,
//    [Symbol('3')] Three,
//    [Symbol('4')] Four,
//    [Symbol('5')] Five,
//    [Symbol('6')] Six,
//    [Symbol('7')] Seven,
//    [Symbol('8')] Eight,
//    [Symbol('9')] Nine,
//    [Symbol('T')] Ten,
//    [Symbol('J')] Jack,
//    [Symbol('Q')] Queen,
//    [Symbol('K')] King,
//    [Symbol('A')] Ace
//}

//public enum
//Suit {
//    [Symbol('c')] Clubs,
//    [Symbol('d')] Diamonds,
//    [Symbol('h')] Hearts,
//    [Symbol('s')] Spades
//} 







