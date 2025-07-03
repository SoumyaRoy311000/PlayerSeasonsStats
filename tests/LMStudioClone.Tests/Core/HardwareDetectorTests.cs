using FluentAssertions;
using LMStudioClone.Core;
using Xunit;

namespace LMStudioClone.Tests.Core
{
    public class HardwareDetectorTests
    {
        [Fact]
        public async Task DetectHardwareAsync_ShouldReturnHardwareInfo()
        {
            // Arrange
            var detector = new HardwareDetector();

            // Act
            var result = await detector.DetectHardwareAsync();

            // Assert
            result.Should().NotBeNull();
            result.CpuName.Should().NotBeNullOrEmpty();
            result.RecommendedProvider.Should().NotBeNullOrEmpty();
            result.TotalMemoryMB.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData("CPU")]
        [InlineData("CUDA")]
        [InlineData("DirectML")]
        [InlineData("OpenVINO")]
        public async Task IsProviderAvailableAsync_ShouldReturnBooleanForKnownProviders(string providerName)
        {
            // Arrange
            var detector = new HardwareDetector();

            // Act
            var result = await detector.IsProviderAvailableAsync(providerName);

            // Assert
            result.Should().BeOfType<bool>();
        }

        [Fact]
        public async Task IsProviderAvailableAsync_ShouldReturnFalseForUnknownProvider()
        {
            // Arrange
            var detector = new HardwareDetector();

            // Act
            var result = await detector.IsProviderAvailableAsync("UnknownProvider");

            // Assert
            result.Should().BeFalse();
        }
    }
}

