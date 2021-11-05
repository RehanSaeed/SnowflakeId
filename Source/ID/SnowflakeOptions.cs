namespace Boxed.AspNetCore
{
    public class SnowflakeOptions
    {
        public const int MaxFreeBits = 10;

        public SnowflakeOptions(
            DateTime epoch,
            ulong datacenterId = 0UL,
            ulong workerId = 0UL,
            int datacenterIdBits = 4,
            int workerIdBits = 6)
        {
            if (epoch > DateTime.Today)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(epoch),
                    epoch,
                    "Epoch date must not be in the future.");
            }

            if (datacenterIdBits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(datacenterIdBits),
                    datacenterIdBits,
                    $"Datacenter ID bits can't be less than zero.");
            }

            if (workerIdBits <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(workerIdBits),
                    workerIdBits,
                    $"Worker ID bits can't be less than zero.");
            }

            if (datacenterIdBits + workerIdBits != MaxFreeBits)
            {
                throw new ArgumentException(
                    $"Datacenter ID and Worker ID bits be equal to {MaxFreeBits}.",
                    nameof(datacenterIdBits));
            }

            this.Epoch = epoch;
            this.DatacenterId = datacenterId;
            this.WorkerId = workerId;
            this.DatacenterIdBits = datacenterIdBits;
            this.WorkerIdBits = workerIdBits;
            this.MaxDatacenterId = (ulong)(-1L ^ (-1L << this.DatacenterIdBits));
            this.MaxWorkerId = (ulong)(-1L ^ (-1L << this.WorkerIdBits));

            if (datacenterId > this.MaxDatacenterId)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(datacenterId),
                    datacenterId,
                    $"Datacenter ID can't be greater than {this.MaxDatacenterId}.");
            }

            if (workerId > this.MaxWorkerId)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(workerId),
                    workerId,
                    $"Worker ID can't be greater than {this.MaxWorkerId}.");
            }
        }

        public Func<DateTime> GetUtcNow { get; set; } = () => DateTime.UtcNow;

        public DateTime Epoch { get; }

        public ulong WorkerId { get; }

        public ulong DatacenterId { get; }

        public int DatacenterIdBits { get; }

        public int WorkerIdBits { get; }

        public ulong MaxDatacenterId { get; }

        public ulong MaxWorkerId { get; }
    }
}
