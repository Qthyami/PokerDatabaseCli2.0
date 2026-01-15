
namespace PokerDatabaseCli2._0.HandHistoryParser;

public record
Database {
    public ImmutableList<HandHistory> HandHistories { get; init; }
    public ImmutableList<long> DeletedHandsIds { get; init; }

    public static Database CreateEmpty() => new Database {
        HandHistories = ImmutableList<HandHistory>.Empty,
        DeletedHandsIds = ImmutableList<long>.Empty
    };
    
    public long HandCount => HandHistories.Count;
    
    public long PlayersCount => HandHistories
        .SelectMany(hand => hand.Players)
        .DistinctBy(playerLine => playerLine.Nickname)
        .Count();

    public IEnumerable<(long HandId, SeatLine heroLine)>
    GetLastHeroHands(int requiredHands) {
        var heroNickname = DefineHeroNickname();

        if (heroNickname == null)
            return Enumerable.Empty<(long HandId, SeatLine heroLine)>();

        return GetHeroHands(heroNickname)
            .OrderByDescending(hand => hand.HandId)
            .Take(requiredHands);
    }

    public IEnumerable<(long HandId, SeatLine heroLine)>
    GetHeroHands(string heroName) {
            foreach (var hand in HandHistories) {
                var heroLine = hand.Players.FirstOrDefault(player => player.Nickname.Equals(heroName, StringComparison.OrdinalIgnoreCase));
                if (heroLine != null) {
                    yield return (hand.HandId, heroLine);
                }
            }
        }

    public string?
    DefineHeroNickname() {
        return HandHistories
            .OrderByDescending(hand => hand.HandId)
            .Select(hand => {
                hand.TryGetHeroPlayer(out var heroLine);
                return heroLine?.Nickname;
            })
            .FirstOrDefault(nickname => nickname != null);
    }
}
