using System.Text;
using System;

namespace TradeWeb.Infrastructure.Processing.Csv;

public static class CsvEscaper
{
    public static byte[] ToCsvFieldUtf8(string value)
    {
        var mustQuote = value.AsSpan().IndexOfAny([',', '"', '\r', '\n']) >= 0;
        if (!mustQuote)
            return Encoding.UTF8.GetBytes(value);

        var sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
        foreach (var ch in value)
        {
            if (ch == '"') sb.Append("\"\"");
            else sb.Append(ch);
        }
        sb.Append('"');
        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
