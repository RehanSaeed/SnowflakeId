namespace ID.Test
{
    using Boxed.AspNetCore;
    using System.ComponentModel.DataAnnotations;
    using Xunit;

    public class SnowflakeIdOptionsTest
    {
        [Fact]
        public void SnowflakeIdOptions_EpochMoreThanToday_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new SnowflakeIdOptions()
                {
                    Epoch = DateTime.UtcNow.AddSeconds(1)
                });

        [Fact]
        public void SnowflakeIdOptions_DatecenterIdBitsTooLow_ThrowsArgumentOutOfRangeException() =>
           Assert.Throws<ArgumentOutOfRangeException>(
               () => new SnowflakeIdOptions()
               {
                   Epoch = new DateTime(1, 1, 1),
                   DatacenterIdBits = 0,
               });

        [Fact]
        public void SnowflakeIdOptions_WorkerIdBitsTooLow_ThrowsArgumentOutOfRangeException() =>
           Assert.Throws<ArgumentOutOfRangeException>(
               () => new SnowflakeIdOptions()
               {
                   Epoch = new DateTime(1, 1, 1),
                   WorkerIdBits = 0,
               });

        [Fact]
        public void Validate_DatacenterIdTooHigh_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ValidationException>(
                () => new SnowflakeIdOptions()
                {
                    Epoch = new DateTime(1, 1, 1),
                    DatacenterId = 16,
                }.Validate());

        [Fact]
        public void Validate_WorkerIdTooHigh_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ValidationException>(
                () => new SnowflakeIdOptions()
                {
                    Epoch = new DateTime(1, 1, 1),
                    WorkerId = 64,
                }.Validate());

        [Theory]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        public void Validate_FreeBitsTooHigh_ThrowsArgumentException(
            int datacenterIdBits,
            int workerIdBits) =>
            Assert.Throws<ValidationException>(
                () => new SnowflakeIdOptions()
                {
                    Epoch = new DateTime(1, 1, 1),
                    DatacenterIdBits = datacenterIdBits,
                    WorkerIdBits = workerIdBits,
                }.Validate());
    }
}
