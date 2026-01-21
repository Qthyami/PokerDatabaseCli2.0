namespace PokerDatabaseCli2._0.Tests;

using NUnit.Framework;


[TestFixture]
public class DatabaseTests {

    private const string TestHandHistories = """
        PokerStars Hand #255725031222:  Hold'em No Limit ($1/$2 USD) - 2025/04/14 1:52:42 MSK [2025/04/13 18:52:42 ET]
        Table 'Alterf II' 6-max Seat #1 is the button
        Seat 1: LamanJohn ($20 in chips)
        Seat 4: niñopoker69 ($200 in chips)
        Seat 5: Eredar5 ($20 in chips)
        Seat 6: G.Andrei95 ($200 in chips)
        niñopoker69: posts small blind $1
        Eredar5: posts big blind $2
        *** HOLE CARDS ***
        Dealt to LamanJohn [Ks Kc]
        G.Andrei95: folds
        LamanJohn: raises $2 to $4
        niñopoker69: folds
        Eredar5: raises $16 to $20 and is all-in
        LamanJohn: calls $16 and is all-in
        *** FLOP *** [9c 8d 2c]
        *** TURN *** [9c 8d 2c] [3d]
        *** RIVER *** [9c 8d 2c 3d] [7c]
        *** SHOW DOWN ***
        Eredar5: shows [6c Ac] (a flush, Ace high)
        LamanJohn: shows [Ks Kc] (a pair of Kings)
        Eredar5 collected $39.75 from pot
        *** SUMMARY ***
        Total pot $41 | Rake $1.25
        Board [9c 8d 2c 3d 7c]
        Seat 1: LamanJohn (button) showed [Ks Kc] and lost with a pair of Kings
        Seat 4: niñopoker69 (small blind) folded before Flop
        Seat 5: Eredar5 (big blind) showed [6c Ac] and won ($39.75) with a flush, Ace high
        Seat 6: G.Andrei95 folded before Flop (didn't bet)

        PokerStars Hand #255725038185:  Hold'em No Limit ($1/$2 USD) - 2025/04/14 1:53:15 MSK [2025/04/13 18:53:15 ET]
        Table 'Alterf II' 6-max Seat #4 is the button
        Seat 1: LamanJohn ($20 in chips)
        Seat 4: niñopoker69 ($200 in chips)
        Seat 5: Eredar5 ($39.75 in chips)
        Seat 6: G.Andrei95 ($200 in chips)
        Eredar5: posts small blind $1
        G.Andrei95: posts big blind $2
        *** HOLE CARDS ***
        Dealt to LamanJohn [3h 6h]
        LamanJohn: folds
        niñopoker69: folds
        Eredar5: raises $3 to $5
        G.Andrei95: raises $195 to $200 and is all-in
        Eredar5: folds
        Uncalled bet ($195) returned to G.Andrei95
        G.Andrei95 collected $10 from pot
        G.Andrei95: doesn't show hand
        *** SUMMARY ***
        Total pot $10 | Rake $0
        Seat 1: LamanJohn folded before Flop (didn't bet)
        Seat 4: niñopoker69 (button) folded before Flop (didn't bet)
        Seat 5: Eredar5 (small blind) folded before Flop
        Seat 6: G.Andrei95 (big blind) collected ($10)

        PokerStars Hand #255725043455:  Hold'em No Limit ($1/$2 USD) - 2025/04/14 1:53:39 MSK [2025/04/13 18:53:39 ET]
        Table 'Alterf II' 6-max Seat #5 is the button
        Seat 1: LamanJohn ($20 in chips)
        Seat 4: niñopoker69 ($200 in chips)
        Seat 5: Eredar5 ($34.75 in chips)
        Seat 6: G.Andrei95 ($205 in chips)
        G.Andrei95: posts small blind $1
        LamanJohn: posts big blind $2
        *** HOLE CARDS ***
        Dealt to LamanJohn [4c 5h]
        niñopoker69: raises $2 to $4
        Eredar5: folds
        G.Andrei95: folds
        LamanJohn: folds
        Uncalled bet ($2) returned to niñopoker69
        niñopoker69 collected $5 from pot
        niñopoker69: doesn't show hand
        *** SUMMARY ***
        Total pot $5 | Rake $0
        Seat 1: LamanJohn (big blind) folded before Flop
        Seat 4: niñopoker69 collected ($5)
        Seat 5: Eredar5 (button) folded before Flop (didn't bet)
        Seat 6: G.Andrei95 (small blind) folded before Flop
        """;

    [Test]
    public void 
    AddHands_AddsHandsToEmptyDatabase_ReturnsNewDatabaseWithHands() {
        var emptyDatabase = Database.Empty();
        var handsToAdd = TestHandHistories
            .GetLines()
            .SplitByEmptyLines()
            .Select(text => text.ParseHandHistory())
            .ToImmutableList();

        var (result, _) = emptyDatabase.AddHands(handsToAdd);

        result.HandHistories.AssertCount(3);
        result.HandHistories[0].HandId.Assert(255725031222);
        result.HandHistories[1].HandId.Assert(255725038185);
        result.HandHistories[2].HandId.Assert(255725043455);
    }

    [Test]
    public void
    AddHands_AddsHandsToExistingDatabase_CombinesHands() {
        var handsToAdd = TestHandHistories
            .GetLines()
            .SplitByEmptyLines()
            .Select(text => text.ParseHandHistory())
            .ToImmutableList();

        var (databaseWithOneHand, _) = Database.Empty().AddHands(handsToAdd.Take(1).ToImmutableList());
        var (result, _) = databaseWithOneHand.AddHands(handsToAdd.Skip(1).ToImmutableList());

        result.HandHistories.AssertCount(3);
    }

    [Test]
    public void
    AddHands_EmptyList_ReturnsUnchangedDatabase() {
        var database = Database.Empty();
        var emptyList = ImmutableList<HandHistory>.Empty;
        var (result, _) = database.AddHands(emptyList);

        result.HandHistories.AssertCount(0);
    }

   
    [Test]
    public void
    GetDatabaseStats_WithHands_ReturnsCorrectCounts() {
        var handsToAdd = TestHandHistories
            .GetLines()
            .SplitByEmptyLines()
            .Select(text => text.ParseHandHistory())
            .ToImmutableList();

        var (database, _) = Database.Empty().AddHands(handsToAdd);

        database.HandCount.Assert(3);
        database.PlayersCount.Assert(4);
    }

    [Test]
    public void
    GetDatabaseStats_EmptyDatabase_ReturnsZeros() {
        var database = Database.Empty();

        database.HandCount.Assert(0);
        database.PlayersCount.Assert(0);
    }

    [Test]
    public void
    DeleteHandById_EmptyDatabase_Throws() {
        var database = Database.Empty();

        Assert.Throws<InvalidOperationException>(
            () => database.DeleteHandById(123456L));
    }

    [Test]
    public void 
    DeleteHandById_NonExistingHand_Throws() {
        var database = Database.Empty();

        Assert.Throws<InvalidOperationException>(
            () => database.DeleteHandById(999999999L));
    }

    [Test]
    public void 
    GetDatabaseStats_AfterDeletingHand_ReturnsUpdatedCounts() {
        // Arrange
        var handsToAdd = TestHandHistories
            .GetLines()
            .SplitByEmptyLines()
            .Select(text => text.ParseHandHistory())
            .ToImmutableList();

        var existingHandId = handsToAdd[0].HandId;

        var (database, addedHandsCount) = Database.Empty().AddHands(handsToAdd);
        var updatedDatabase = database.DeleteHandById(existingHandId);

        updatedDatabase.HandCount.Assert(handsToAdd.Count - 1);
    }

    [Test]
    public void 
    GetLastHeroHands_ReturnsRequestedNumberOfHands() {
         var handsToAdd = TestHandHistories
            .GetLines()
            .SplitByEmptyLines()
            .Select(text => text.ParseHandHistory())
            .ToImmutableList();

        var (database, _) = Database.Empty().AddHands(handsToAdd);
        var result = database.GetLastHeroHands(2).ToList();

        result.AssertCount(2);
        result[0].TryGetHeroPlayer(out var hero0).Assert(true);
        hero0.Nickname.Assert("LamanJohn");
        result[1].TryGetHeroPlayer(out var hero1).Assert(true);
        hero1.Nickname.Assert("LamanJohn");
    }

    [Test]
    public void 
    GetLastHeroHands_ReturnsHandsInDescendingOrder() {
         var handsToAdd = TestHandHistories
            .GetLines()
            .SplitByEmptyLines()
            .Select(text => text.ParseHandHistory())
            .ToImmutableList();

        var (database, _) = Database.Empty().AddHands(handsToAdd);
        var result = database.GetLastHeroHands(3).ToList();

        result.AssertCount(3);
        result[0].HandId.Assert(255725043455L);
        result[1].HandId.Assert(255725038185L);
        result[2].HandId.Assert(255725031222L);
    }

    [Test]
    public void 
    GetLastHeroHands_RequestMoreThanExists_ReturnsAllAvailable() {
         var handsToAdd = TestHandHistories
            .GetLines()
            .SplitByEmptyLines()
            .Select(text => text.ParseHandHistory())
            .ToImmutableList();

        var (database, _) = Database.Empty().AddHands(handsToAdd);
        var result = database.GetLastHeroHands(100).ToList();

        result.AssertCount(3);
    }

    [Test]
    public void 
    GetLastHeroHands_EmptyDatabase_ReturnsEmpty() {
        var database = Database.Empty();
        var result = database.GetLastHeroHands(5).ToList();

        result.AssertCount(0);
    }

    [Test]
    public void 
    GetLastHeroHands_HeroCardsArePreserved() {
         var handsToAdd = TestHandHistories
            .GetLines()
            .SplitByEmptyLines()
            .Select(text => text.ParseHandHistory())
            .ToImmutableList();

        var (database, _) = Database.Empty().AddHands(handsToAdd);
        var result = database.GetLastHeroHands(1).ToList();

        result[0].TryGetHeroPlayer(out var heroPlayer).Assert(true);
        heroPlayer.DealtCards.AssertCount(2);
        heroPlayer.DealtCards[0].AssertCard(CardRank.Four, Suit.Clubs);
        heroPlayer.DealtCards[1].AssertCard(CardRank.Five, Suit.Hearts);
    }
}