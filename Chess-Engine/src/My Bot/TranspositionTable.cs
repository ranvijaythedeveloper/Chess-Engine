using System;

namespace ChessEngine.src.My_Bot
{
    public class TranspositionTable
    {
        private const int BytesInMegabyte = 1024 * 1024;
        private const int EntrySizeBytes = sizeof(int) * 3 + sizeof(ulong); // Size of each entry in bytes

        private readonly TranspositionEntry[] _table;
        private readonly int _maxEntries;

        public TranspositionTable(int sizeInMegabytes)
        {
            int totalSizeBytes = sizeInMegabytes * BytesInMegabyte;
            _maxEntries = totalSizeBytes / EntrySizeBytes;
            _table = new TranspositionEntry[_maxEntries];
        }

        public void Store(ulong hash, int depth, int score, TranspositionFlag flag)
        {
            int index = (int)(hash % (ulong)_maxEntries);
            _table[index] = new TranspositionEntry(depth, score, flag, hash);
        }

        public TranspositionEntry Retrieve(ulong hash)
        {
            int index = (int)(hash % (ulong)_maxEntries);
            return _table[index];
        }
    }

    public enum TranspositionFlag
    {
        Exact,
        LowerBound,
        UpperBound
    }

    public class TranspositionEntry
    {
        public int Depth { get; }
        public int Score { get; }
        public TranspositionFlag Flag { get; }
        public ulong Hash { get; }

        public TranspositionEntry(int depth, int score, TranspositionFlag flag, ulong hash)
        {
            Depth = depth;
            Score = score;
            Flag = flag;
            Hash = hash;
        }
    }
}
