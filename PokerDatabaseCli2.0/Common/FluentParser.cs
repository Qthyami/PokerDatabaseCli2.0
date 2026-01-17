using System.Globalization;

namespace PokerDatabaseCli2._0.Common;

public class FluentParser {
    public string String { get; }
    public int _position { get; private set; }
    public int Position => _position;
    public char NextChar => String[_position];
    public bool HasNext => _position < String.Length - 1;
    public char NextNextChar => CharactersLeft > 1 ? String[_position + 1] : '\0';
    public char PreviousChar => String[_position - 1];
    public int Length => String.Length;
    public bool HasCurrent => String.Length > _position;
    public int CharactersLeft => Length - _position;

    public FluentParser(string @string) => String = @string;

    public override string ToString() {
        var contextLen = Math.Min(40, CharactersLeft);
        var snippet = contextLen > 0 ? String.Substring(Position, contextLen) : string.Empty;
        return $"pos={Position}, next=\"{snippet}\"";
    }

    public FluentParser
    SkipOne() {
        _position++;
        return this;
    }

    public FluentParser
    Skip(int count) {
        _position += count;
        return this;
    }

    public FluentParser
    SkipSpaces() {
        while (HasCurrent && char.IsWhiteSpace(NextChar))
            SkipOne();
        return this;
    }

    public FluentParser
    RollbackUntil(string @string) {
        var index = String.LastIndexOf(@string, Position, StringComparison.Ordinal);
        if (index == -1)
            _position = 0;
        _position = index + @string.Length;
        return this;
    }

    public bool
    Next(char @char) => NextChar == @char;

    public bool
    Next(string @string, int offset = 0) {

        if (@string.Length > CharactersLeft)
            return false;

        if (String[_position + offset] != @string[0])
            return false;

        for (int i = offset + 1; i < @string.Length; i++)
            if (@string[i] != String[_position + i])
                return false;
        return true;
    }

    public bool
    NextCaseInSensitive(string @string, int offset = 0) {
        if (@string.Length > CharactersLeft)
            return false;

        var result = this.Clone().Read(@string.Length);
        return result.Equals(@string, comparisonType: StringComparison.OrdinalIgnoreCase);
    }

    public char
    NextCharAt(int offset) => String[_position + offset];


    public string
    Read(int count) {
        count = Math.Min(count, CharactersLeft);
        var result = String.Substring(_position, count);
        _position += count;
        return result;
    }

    public string
    ReadUntil(char @char) {
        if (!HasCurrent || NextChar == @char)
            return string.Empty;
        for (int i = _position; i < Length; i++)
            if (String[i] == @char) {
                var result = String.Substring(_position, i - _position);
                _position = i;
                return result;
            }
        return string.Empty;
    }

    public string
    ReadUntilLast(char @char) {
        var index = String.LastIndexOf(@char);
        if (index == -1 || index == _position || index < _position)
            return string.Empty;
        var result = String.Substring(Position, index - Position);
        _position = index;
        return result;
    }

    public bool
    TryReadUntil(char @char, int maxLength, out string result) {
        if (NextChar == @char) {
            result = string.Empty;
            return true;
        }

        if (!HasNext) {
            result = null;
            return false;
        }

        var index = String.IndexOf(@char, _position, Math.Min(maxLength, CharactersLeft));
        if (index == -1) {
            result = null;
            return false;
        }

        result = String.Substring(_position, index - _position);
        _position = index;
        return true;
    }

    public bool
    TryReadUntil(string @string, out string result) {
        var initialPosition = _position;
        var index = String.IndexOf(@string, _position, StringComparison.Ordinal);
        if (index == -1) {
            result = null;
            return false;
        }

        result = String.Substring(_position, index - _position);
        _position = index;
        return true;
    }

    public bool
    TrySkipUntil(string @string) {
        var index = String.IndexOf(@string, _position, StringComparison.Ordinal);
        if (index == -1)
            return false;
        _position = index;
        return true;
    }

    public bool

    TryReadWord(out string result, int offset = 0) {
        if (!NextCharAt(offset).IsWordCharacter()) {
            result = string.Empty;
            return false;
        }
        for (int i = _position + offset; i < Length; i++) {
            if (!String[i].IsWordCharacter()) {
                result = String.Substring(_position + offset, i - _position - offset);
                _position = i;
                return true;
            }
        }

        result = string.Empty;
        return false;
    }
    // Reads a word from terminal, where  word can be entire string, and TryReadWord func does not work
    public bool TryReadWordFromTerminal(out string result, int offset = 0) {
        if (!NextCharAt(offset).IsWordCharacter()) {
            result = string.Empty;
            return false;
        }
        int start = _position + offset;
        int i = start;
        while (i < Length && String[i].IsWordCharacter())
            i++;
        result = String.Substring(start, i - start);
        _position = i;
        return true;
    }

    public FluentParser
    SkipUntilNextLine() {
        SkipAfter('\n');
        if (Next('\r'))
            SkipOne();
        return this;
    }

    public FluentParser
    SkipAfter(char @char) {
        if (HasCurrent) {
            var index = String.IndexOf(@char, _position);
            _position = index == -1 ? String.Length : index + 1;
        }
        return this;
    }

    public string
    ReadToEnd() => String.Substring(Position);

    public int
    ReadInt() {
        int result = 0;
        var isNegative = HasCurrent && NextChar == '-';
        if (isNegative)
            SkipOne();
        while (HasCurrent && NextChar.IsDigit()) {
            result = result * 10 + NextChar.ToDigit();
            SkipOne();
        }
        return isNegative ? -result : result;
    }

    public long
    ReadLong() {
        long result = 0;
        var isNegative = HasCurrent && NextChar == '-';
        if (isNegative)
            SkipOne();
        while (HasCurrent && NextChar.IsDigit()) {
            result = result * 10 + NextChar.ToDigit();
            SkipOne();
        }
        return isNegative ? -result : result;
    }

    public double
    ReadDouble(CultureInfo cultureInfo = null) {
        if (!NextChar.IsDigit())
            throw new InvalidOperationException($"Reader position must be placed on a digit: {this}");
        double result = ReadInt();

        void
        AddIntPart(int number, int digits) => result = result * Math.Pow(10, digits) + number;

        (int number, int digits)
         ReadIntLocal() {
            var initialPosition = Position;
            return (ReadInt(), Position - initialPosition);
        }

        while (HasCurrent) {
            if (Next('.') || Next(',')) {
                if (!String[Position + 1].IsDigit())
                    return result;
                SkipOne();
                var (number, digits) = ReadIntLocal();
                if (digits == 3 && (cultureInfo == null || !Equals(cultureInfo, CultureInfo.InvariantCulture)))
                    AddIntPart(number, 3);
                else
                    return result + number / Math.Pow(10, digits);
            }
            else
                return result;
        }

        return result;
    }

    public int
    ReadDigit() {
        var result = NextChar.ToDigit();
        SkipOne();
        return result;
    }

    public FluentParser
    Clone() => new FluentParser(String).Skip(_position);

    public FluentParser
    VerifyNext(string @string) {
        if (!Next(@string))
            throw new InvalidOperationException($"Expecting next=\"{@string}\" but was {this}");
        return this;
    }
}

internal static class
FluentParserHelperInternal {
    public static bool
    IsWordCharacter(this char @char) => @char.IsDigit() || @char.IsLetter() || @char == '_' || @char == '-' || @char == '.';

    public static bool
    IsDigit(this char @char) => '0' <= @char && @char <= '9';

    public static bool
    IsCapitalLetter(this char @char) => 'A' <= @char && @char <= 'Z';

    public static bool
    IsSmallLetter(this char @char) => 'a' <= @char && @char <= 'z';

    public static bool
    IsLetter(this char @char) => @char.IsSmallLetter() || @char.IsCapitalLetter();

    public static int
    ToDigit(this char @char) {
#if DEBUG
        if (@char - '0' > 9)
            throw new ArgumentOutOfRangeException(nameof(@char));
#endif
        return @char - '0';
    }
}

