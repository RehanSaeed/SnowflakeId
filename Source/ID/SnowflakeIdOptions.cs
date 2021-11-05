using System.ComponentModel.DataAnnotations;

namespace Boxed.AspNetCore
{
    public class SnowflakeIdOptions
    {
        public const int MaxFreeBits = 10;

        private DateTime epoch;
        private int datacenterIdBits = 4;
        private int workerIdBits = 6;

        public Func<DateTime> GetUtcNow { get; set; } = () => DateTime.UtcNow;

        public DateTime Epoch
        {
            get => this.epoch;
            set
            {
                if (value > DateTime.Today)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        "Epoch date must not be in the future.");
                }

                this.epoch = value;
            }
        }

        public ulong WorkerId { get; init; } = 0UL;

        public ulong DatacenterId { get; init; } = 0UL;

        public int DatacenterIdBits
        {
            get => this.datacenterIdBits;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Datacenter ID bits can't be less than zero.");
                }

                this.datacenterIdBits = value;
            }
        }

        public int WorkerIdBits
        {
            get => this.workerIdBits;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        value,
                        $"Worker ID bits can't be less than zero.");
                }

                this.workerIdBits = value;
            }
        }

        public ulong MaxDatacenterId => (ulong)(-1L ^ (-1L << this.DatacenterIdBits));

        public ulong MaxWorkerId => (ulong)(-1L ^ (-1L << this.WorkerIdBits));

        public void Validate()
        {
            if (this.DatacenterIdBits + this.WorkerIdBits != MaxFreeBits)
            {
                throw new ValidationException(
                    $"Datacenter ID '{this.DatacenterIdBits}' and Worker ID '{this.WorkerIdBits}' bits must be equal to {MaxFreeBits}.");
            }

            if (this.DatacenterId > this.MaxDatacenterId)
            {
                throw new ValidationException(
                    $"Datacenter ID '{this.DatacenterId}' can't be greater than {this.MaxDatacenterId}.");
            }

            if (this.WorkerId > this.MaxWorkerId)
            {
                throw new ValidationException(
                    $"Worker ID '{this.WorkerId}' can't be greater than {this.MaxWorkerId}.");
            }
        }
    }
}
