namespace ID.Test
{
    using Boxed.AspNetCore;
    using System;
    using Xunit;

    public class SnowflakeIdServiceTest
    {
        private static readonly DateTime DefaultEpoch = new DateTime(1, 1, 1);

        [Theory]
        [InlineData("0001-1-1", 1UL)]
        [InlineData("0001-1-1 00:00:00.001", 4194304UL)]
        [InlineData("0001-1-1 00:00:01", 4194304000UL)]
        [InlineData("0001-1-2 00:00:00", 362387865600000UL)]
        [InlineData("9999-1-1 00:00:00", 13610765250948235264UL)]
        public void CreateSnowflake_DefaultDatacenterAndWorderIds_GeneratesId(string now, ulong expectedSnowflakeId)
        {
            var nowDateTime = DateTime.Parse(now);
            var options = new SnowflakeIdOptions()
            {
                Epoch = DefaultEpoch,
                GetUtcNow = () => nowDateTime,
            };
            var snowflakeService = new SnowflakeIdService(options);

            var snowflakeId = snowflakeService.CreateSnowflakeId();

            Assert.Equal(expectedSnowflakeId, snowflakeId);
        }

        [Fact]
        public void CreateSnowflake_TwoDifferentDataCenters_ProduceDifferentIds()
        {
            var nowDateTime = new DateTime(2000, 1, 1);
            var options1 = new SnowflakeIdOptions()
            {
                Epoch = DefaultEpoch,
                DatacenterId = 1,
                GetUtcNow = () => nowDateTime,
            };
            var options2 = new SnowflakeIdOptions()
            {
                Epoch = DefaultEpoch,
                DatacenterId = 2,
                GetUtcNow = () => nowDateTime,
            };
            var snowflakeService1 = new SnowflakeIdService(options1);
            var snowflakeService2 = new SnowflakeIdService(options2);

            var snowflakeId1 = snowflakeService1.CreateSnowflakeId();
            var snowflakeId2 = snowflakeService2.CreateSnowflakeId();

            Assert.NotEqual(snowflakeId1, snowflakeId2);
        }

        [Fact]
        public void CreateSnowflake_TwoDifferentWorkers_ProduceDifferentIds()
        {
            var nowDateTime = new DateTime(2000, 1, 1);
            var options1 = new SnowflakeIdOptions()
            {
                Epoch = DefaultEpoch,
                DatacenterId = 1,
                WorkerId = 1,
                GetUtcNow = () => nowDateTime,
            };
            var options2 = new SnowflakeIdOptions()
            {
                Epoch = DefaultEpoch,
                DatacenterId = 1,
                WorkerId = 2,
                GetUtcNow = () => nowDateTime,
            };
            var snowflakeService1 = new SnowflakeIdService(options1);
            var snowflakeService2 = new SnowflakeIdService(options2);

            var snowflakeId1 = snowflakeService1.CreateSnowflakeId();
            var snowflakeId2 = snowflakeService2.CreateSnowflakeId();

            Assert.NotEqual(snowflakeId1, snowflakeId2);
        }

        [Fact]
        public void CreateSnowflake_Default_DoesNotGenerateZeroOrNegativeIds()
        {
            var options = new SnowflakeIdOptions()
            {
                Epoch = DefaultEpoch,
            };
            var snowflakeService = new SnowflakeIdService(options);

            for (var i = 0; i < 1_000_000; i++)
            {
                var snowflakeId = snowflakeService.CreateSnowflakeId();

                Assert.False(snowflakeId <= 0);
            }
        }

        [Fact]
        public void CreateSnowflake_Default_GeneratesSequentialIds()
        {
            var options = new SnowflakeIdOptions()
            {
                Epoch = DefaultEpoch,
            };
            var snowflakeService = new SnowflakeIdService(options);

            ulong? snowflakeId = null;
            ulong? lastSnowflakeId = null;
            for (var i = 0; i < 1_000_000; i++)
            {
                lastSnowflakeId = snowflakeId;
                snowflakeId = snowflakeService.CreateSnowflakeId();

                if (lastSnowflakeId.HasValue)
                {
                    Assert.True(snowflakeId.Value > lastSnowflakeId.Value);
                }
            }
        }

        [Fact]
        public void CreateSnowflake_ClockMovesBack_ThrowsInvalidOperationException()
        {
            var count = 0;
            var now1 = new DateTime(2000, 1, 1);
            var now2 = new DateTime(2000, 1, 1).AddMilliseconds(-1);
            var options = new SnowflakeIdOptions()
            {
                Epoch = DefaultEpoch,
            };
            options.GetUtcNow =
                () =>
                {
                    if (count == 0)
                    {
                        ++count;
                        return now1;
                    }

                    return now2;
                };
            var snowflakeService = new SnowflakeIdService(options);

            snowflakeService.CreateSnowflakeId();
            Assert.Throws<InvalidOperationException>(() => snowflakeService.CreateSnowflakeId());
        }
    }
}