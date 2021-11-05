namespace ID.Test
{
    using Boxed.AspNetCore;
    using Xunit;

    public class SnowflakeOptionsTest
    {
        [Fact]
        public void SnowflakeIdOptions_EpochMoreThanToday_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new SnowflakeOptions(DateTime.UtcNow.AddSeconds(1)));

        [Fact]
        public void SnowflakeIdOptions_DatacenterIdTooHigh_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new SnowflakeOptions(
                    new DateTime(1, 1, 1),
                    16));

        [Fact]
        public void SnowflakeIdOptions_WorkerIdTooHigh_ThrowsArgumentOutOfRangeException() =>
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new SnowflakeOptions(
                    new DateTime(1, 1, 1),
                    0,
                    64));

        [Theory]
        [InlineData(1, 10)]
        [InlineData(10, 1)]
        public void SnowflakeIdOptions_FreeBitsTooHigh_ThrowsArgumentException(
            int datacenterIdBits,
            int workerIdBits) =>
            Assert.Throws<ArgumentException>(
                () => new SnowflakeOptions(
                    new DateTime(1, 1, 1),
                    0,
                    0,
                    datacenterIdBits,
                    workerIdBits));

        [Fact]
        public void SnowflakeIdOptions_DatecenterIdBitsTooLow_ThrowsArgumentOutOfRangeException() =>
           Assert.Throws<ArgumentOutOfRangeException>(
               () => new SnowflakeOptions(
                   new DateTime(1, 1, 1),
                   datacenterIdBits: 0));

        [Fact]
        public void SnowflakeIdOptions_WorkerIdBitsTooLow_ThrowsArgumentOutOfRangeException() =>
           Assert.Throws<ArgumentOutOfRangeException>(
               () => new SnowflakeOptions(
                   new DateTime(1, 1, 1),
                   workerIdBits: 0));
    }
}
