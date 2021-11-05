namespace ID.Test
{
    using Boxed.AspNetCore;
    using System;
    using Xunit;

    public class SnowflakeServiceTest
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
            var options = new SnowflakeOptions(DefaultEpoch)
            {
                GetUtcNow = () => nowDateTime,
            };
            var snowflakeService = new SnowflakeService(options);

            var snowflakeId = snowflakeService.CreateSnowflake();

            Assert.Equal(expectedSnowflakeId, snowflakeId);
        }

        [Fact]
        public void CreateSnowflake_TwoDifferentDataCenters_ProduceDifferentIds()
        {
            var nowDateTime = new DateTime(2000, 1, 1);
            var options1 = new SnowflakeOptions(DefaultEpoch, 1)
            {
                GetUtcNow = () => nowDateTime,
            };
            var options2 = new SnowflakeOptions(DefaultEpoch, 2)
            {
                GetUtcNow = () => nowDateTime,
            };
            var snowflakeService1 = new SnowflakeService(options1);
            var snowflakeService2 = new SnowflakeService(options2);

            var snowflakeId1 = snowflakeService1.CreateSnowflake();
            var snowflakeId2 = snowflakeService2.CreateSnowflake();

            Assert.NotEqual(snowflakeId1, snowflakeId2);
        }

        [Fact]
        public void CreateSnowflake_TwoDifferentWorkers_ProduceDifferentIds()
        {
            var nowDateTime = new DateTime(2000, 1, 1);
            var options1 = new SnowflakeOptions(DefaultEpoch, 1, 1)
            {
                GetUtcNow = () => nowDateTime,
            };
            var options2 = new SnowflakeOptions(DefaultEpoch, 1, 2)
            {
                GetUtcNow = () => nowDateTime,
            };
            var snowflakeService1 = new SnowflakeService(options1);
            var snowflakeService2 = new SnowflakeService(options2);

            var snowflakeId1 = snowflakeService1.CreateSnowflake();
            var snowflakeId2 = snowflakeService2.CreateSnowflake();

            Assert.NotEqual(snowflakeId1, snowflakeId2);
        }

        [Fact]
        public void CreateSnowflake_Default_DoesNotGenerateZeroOrNegativeIds()
        {
            var options = new SnowflakeOptions(DefaultEpoch);
            var snowflakeService = new SnowflakeService(options);

            for (var i = 0; i < 1_000_000; i++)
            {
                var snowflakeId = snowflakeService.CreateSnowflake();

                Assert.False(snowflakeId <= 0);
            }
        }

        [Fact]
        public void CreateSnowflake_Default_GeneratesSequentialIds()
        {
            var options = new SnowflakeOptions(DefaultEpoch);
            var snowflakeService = new SnowflakeService(options);

            ulong? snowflakeId = null;
            ulong? lastSnowflakeId = null;
            for (var i = 0; i < 1_000_000; i++)
            {
                lastSnowflakeId = snowflakeId;
                snowflakeId = snowflakeService.CreateSnowflake();

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
            var options = new SnowflakeOptions(DefaultEpoch);
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
            var snowflakeService = new SnowflakeService(options);

            snowflakeService.CreateSnowflake();
            Assert.Throws<InvalidOperationException>(() => snowflakeService.CreateSnowflake());
        }
    }
}