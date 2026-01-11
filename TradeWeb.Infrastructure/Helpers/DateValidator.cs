
using System.Runtime.CompilerServices;

namespace TradeWeb.Infrastructure.Helpers;
public static class DateValidator
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValidYyyyMmDd(ReadOnlySpan<byte> s)
    {
        if (s.Length != 8) return false;

        for (int i = 0; i < 8; i++)
            if ((uint)(s[i] - (byte)'0') > 9) return false;

        int year = (s[0] - '0') * 1000 + (s[1] - '0') * 100 + (s[2] - '0') * 10 + (s[3] - '0');
        int month = (s[4] - '0') * 10 + (s[5] - '0');
        int day = (s[6] - '0') * 10 + (s[7] - '0');

        if (month < 1 || month > 12) return false;

        int dim = DateTime.DaysInMonth(year, month);
        return day >= 1 && day <= dim;
    }
}
