using System.Buffers;
using System.IO.Pipelines;

namespace TradeWeb.Infrastructure.Processing.Csv;

public static class CsvLineReader
{
    public static async IAsyncEnumerable<ReadOnlyMemory<byte>> ReadLinesAsync(
        PipeReader reader,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct)
    {
        while (true)
        {
            var result = await reader.ReadAsync(ct);
            var buffer = result.Buffer;

            while (TryReadLine(ref buffer, out var line))
            {
                yield return line.ToArray();
            }

            if (result.IsCompleted && !buffer.IsEmpty)
            {
                yield return buffer.ToArray();
                buffer = buffer.Slice(buffer.End);
            }

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                break;
        }

        await reader.CompleteAsync();
    }

    private static bool TryReadLine(ref ReadOnlySequence<byte> buffer, out ReadOnlySequence<byte> line)
    {
        var pos = buffer.PositionOf((byte)'\n');
        if (pos is null)
        {
            line = default;
            return false;
        }

        line = buffer.Slice(0, pos.Value);
        buffer = buffer.Slice(buffer.GetPosition(1, pos.Value));
        return true;
    }
}
