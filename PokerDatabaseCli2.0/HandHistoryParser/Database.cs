
namespace PokerDatabaseCli2._0.HandHistoryParser;

public record
Database {
    public ImmutableList<HandHistory> HandHistories { get; init; } = ImmutableList<HandHistory>.Empty;
    public ImmutableList<long> DeletedHandsIds { get; init; } = ImmutableList<long>.Empty;
    public string HeroNickname { get; init; } = string.Empty;


    public static Database Empty() => new Database();
    public long HandCount => HandHistories.Count;

    public long PlayersCount => HandHistories
        .SelectMany(hand => hand.Players)
        .DistinctBy(playerLine => playerLine.Nickname)
        .Count();

 
    public IEnumerable<HandHistory>
    GetLastHeroHands(int requiredHands) {
        if (!TryGetHeroNickname(out var heroNickname))
            return Enumerable.Empty<HandHistory>();
        if (heroNickname == null)
            return Enumerable.Empty<HandHistory>();

        return GetPlayerHands(heroNickname)
            .OrderByHandIdDescending()
            .Take(requiredHands);
    }

    public IEnumerable<HandHistory>
    GetPlayerHands(string heroName) {
        return HandHistories.WithPlayer(heroName);

    }
    
    public bool
    TryGetHeroNickname(out string? result) {
        foreach (var hand in HandHistories.OrderByDescending(hand => hand.HandId)) {
            if (hand.TryGetHeroPlayer(out var heroLine)) {
                result = heroLine.Nickname;
                return true;
            }
        }
        result = default;
        return false;
    }

    public IEnumerable<(HandHistory Hand, HandHistoryPlayer Hero)>
GetLastHeroHandsWithHero(int requiredHands) {
    if (!TryGetHeroNickname(out var heroNickname) || heroNickname is null)
        return Enumerable.Empty<(HandHistory, HandHistoryPlayer)>();

    return GetLastHeroHands(requiredHands)
        .Select(hand => (hand, hand.GetPlayer(heroNickname)));
}
}

