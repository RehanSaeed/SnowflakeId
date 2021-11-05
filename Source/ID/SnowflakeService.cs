namespace Boxed.AspNetCore
{
    public class SnowflakeService
    {
        private const int SequenceBits = 12;
        private readonly int WorkerIdShift;
        private readonly int DatacenterIdShift;
        private readonly int TimestampLeftShift;
        private readonly ulong SequenceMask;

        private ulong lastTimestamp;
        private ulong sequence;

        public SnowflakeService(SnowflakeOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            this.Options = options;

            WorkerIdShift = SequenceBits;
            DatacenterIdShift = SequenceBits + this.Options.WorkerIdBits;
            TimestampLeftShift = SequenceBits + this.Options.WorkerIdBits + this.Options.DatacenterIdBits;
            SequenceMask = -1L ^ (-1L << SequenceBits);
        }

        public SnowflakeOptions Options { get; }

        public ulong CreateSnowflake()
        {
            var timestamp = this.GetTimestampInMilliseconds();

            if (timestamp < this.lastTimestamp)
            {
                throw new InvalidOperationException(
                    $"Clock moved backwards. Refusing to generate ID for {this.lastTimestamp - timestamp} milliseconds.");
            }

            if (this.lastTimestamp == timestamp)
            {
                this.sequence = (this.sequence + 1UL) & SequenceMask;
                if (this.sequence == 0UL)
                {
                    timestamp = this.GetNextMillisecond(this.lastTimestamp);
                }
            }
            else
            {
                this.sequence = 0UL;
            }

            this.lastTimestamp = timestamp;
            return (timestamp << TimestampLeftShift) |
                (this.Options.DatacenterId << DatacenterIdShift) |
                (this.Options.WorkerId << WorkerIdShift) |
                this.sequence;
        }

        private ulong GetNextMillisecond(ulong lastTimestamp)
        {
            var timestamp = this.GetTimestampInMilliseconds();
            while (timestamp <= lastTimestamp)
            {
                timestamp = this.GetTimestampInMilliseconds();
            }

            return timestamp;
        }

        private ulong GetTimestampInMilliseconds() =>
            (ulong)(this.Options.GetUtcNow() - this.Options.Epoch).TotalMilliseconds;
    }
}
