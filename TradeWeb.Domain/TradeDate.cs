using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeWeb.Domain;
public readonly record struct TradeDate(int Year, int Month, int Day)
{
    public override string ToString() => $"{Year:0000}{Month:00}{Day:00}";

    public static bool TryParseYyyyMmDd(ReadOnlySpan<byte> s, out TradeDate date)
    {
        date = default;
        if (s.Length != 8) return false;

        for (int i = 0; i < 8; i++)
            if ((uint)(s[i] - (byte)'0') > 9) return false;

        int year = (s[0] - '0') * 1000 + (s[1] - '0') * 100 + (s[2] - '0') * 10 + (s[3] - '0');
        int month = (s[4] - '0') * 10 + (s[5] - '0');
        int day = (s[6] - '0') * 10 + (s[7] - '0');

        if (month < 1 || month > 12) return false;

        int dim = DateTime.DaysInMonth(year, month);
        if(day >= 1 && day <= dim) {
            date = new TradeDate(year, month, day);
            return true;
        }
        return false;
    }
}