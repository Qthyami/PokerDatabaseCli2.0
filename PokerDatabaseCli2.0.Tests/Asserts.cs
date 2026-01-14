using NUnit.Framework;


namespace PokerDatabaseCli2._0.Tests;

public static class Asserts {
    public static T
    Assert<T> (this T value, T expected) {
        NUnit.Framework.Assert.That(value, Is.EqualTo(expected));
        return value;
    }

    public static void
AssertCount<T>(this IEnumerable<T> collection, int expectedCount) {
    NUnit.Framework.Assert.That(collection.Count(), Is.EqualTo(expectedCount));
    }

public static void 
AssertTrue(this bool value) => NUnit.Framework.Assert.That(value, Is.True);

public static Card
AssertCard(this Card card, CardRank expectedRank, Suit expectedSuit) {
    card.Rank.Assert(expectedRank);
    card.Suit.Assert(expectedSuit);
    return card;

    }
}
