using System;
using System.Collections.Generic;
using System.Text;

namespace PokerDatabaseCli2._0.HandHistoryParser;

public class
Card {
    public CardRank Rank { get; }
    public Suit Suit { get; }
    public Card(CardRank rank, Suit suit) {
        Rank = rank;
        Suit = suit;
    }
    public override string
    ToString() => $"{Rank.GetSymbol()}{Suit.GetSymbol()}";
}
public enum
CardRank {
    [Symbol('2')] Two = 2,
    [Symbol('3')] Three,
    [Symbol('4')] Four,
    [Symbol('5')] Five,
    [Symbol('6')] Six,
    [Symbol('7')] Seven,
    [Symbol('8')] Eight,
    [Symbol('9')] Nine,
    [Symbol('T')] Ten,
    [Symbol('J')] Jack,
    [Symbol('Q')] Queen,
    [Symbol('K')] King,
    [Symbol('A')] Ace
}

public enum
Suit {
    [Symbol('c')] Clubs,
    [Symbol('d')] Diamonds,
    [Symbol('h')] Hearts,
    [Symbol('s')] Spades
}
