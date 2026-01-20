
namespace PokerDatabaseCli2._0.HandHistoryParser;

public record
Database {
    public ImmutableList<HandHistory> HandHistories { get; init; } = ImmutableList<HandHistory>.Empty;

    public ImmutableList<long> DeletedHandsIds { get; init; } = ImmutableList<long>.Empty;


    public static Database CreateEmpty() => new Database();
    public long HandCount => HandHistories.Count;

    public long PlayersCount => HandHistories
        .SelectMany(hand => hand.Players)
        .DistinctBy(playerLine => playerLine.Nickname)
        .Count();

    //ЭТА ФУНКЦИЯ ОПРЕДЕЛЯЕТ КТО HERO ПО ПОСЛЕДНЕЙ РАЗДАЧЕ И ВОЗВРАЩАЕТ ПОСЛЕДНИЕ N РАЗДАЧ С ЕГО УЧАСТИЕМ
    //ЕСЛИ В ПРЕДПОСЛЕДНЕЙ РАЗДАЧЕ HERO ВНЕЗАПНО ДРУГОЙ - ОНА ЭТУ РАЗДАЧУ ПРОПУСТИТ И ВСЕ РАВНО НАБЕРЕТ N РАЗДАЧ
    //ПРОСТО ТАК НАБРАТЬ (required) ПОСЛЕДНИХ РАЗДАЧ НЕЛЬЗЯ.
    //ХОТЯ НА МАЙНИНГЕ РАБОТАТЬ НЕ БУДЕТ, ТАМ НЕ БУДЕТ DealtCards
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
            if (!hand.ContainsPlayer(heroName))
                continue;
            yield return (hand.HandId, hand.GetPlayer(heroName));
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
