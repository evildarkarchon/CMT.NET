using System;
using System.Net.Http;
using System.Threading.Tasks;
using CMT.NET.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using xdelta3.net;

namespace CMT.NET.Tests.Services;

public class DowngraderServiceTests : IDisposable
{
    private readonly Mock<IGameDetectionService> _gameDetectionService;
    private readonly Mock<IFileOperationService> _fileOperationService;
    private readonly Mock<ILogger<DowngraderService>> _logger;
    private readonly HttpClient _httpClient;
    private readonly DowngraderService _service;

    public DowngraderServiceTests()
    {
        _gameDetectionService = new Mock<IGameDetectionService>();
        _fileOperationService = new Mock<IFileOperationService>();
        _logger = new Mock<ILogger<DowngraderService>>();
        _httpClient = new HttpClient();

        _service = new DowngraderService(
            _gameDetectionService.Object,
            _fileOperationService.Object,
            _logger.Object,
            _httpClient);
    }

    [Fact]
    public async Task ApplyPatchAsync_ValidInputs_AppliesPatchSuccessfully()
    {
        // Arrange
        var originalData = "Hello, World! This is some original data."u8.ToArray();
        var modifiedData = "Hello, World! This is some modified data."u8.ToArray();

        // Create a delta patch using xdelta3.net
        var patchData = Xdelta3Lib.Encode(originalData, modifiedData).ToArray();

        // Act
        var result = await InvokeApplyPatchAsync(originalData, patchData);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(modifiedData.Length, result.Length);
        Assert.Equal(modifiedData, result);
    }

    [Fact]
    public async Task ApplyPatchAsync_NullOriginalData_ThrowsArgumentException()
    {
        // Arrange
        var patchData = new byte[] { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            InvokeApplyPatchAsync(null!, patchData));
    }

    [Fact]
    public async Task ApplyPatchAsync_EmptyOriginalData_ThrowsArgumentException()
    {
        // Arrange
        var originalData = Array.Empty<byte>();
        var patchData = new byte[] { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            InvokeApplyPatchAsync(originalData, patchData));
    }

    [Fact]
    public async Task ApplyPatchAsync_NullPatchData_ThrowsArgumentException()
    {
        // Arrange
        var originalData = new byte[] { 1, 2, 3 };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            InvokeApplyPatchAsync(originalData, null!));
    }

    [Fact]
    public async Task ApplyPatchAsync_EmptyPatchData_ThrowsArgumentException()
    {
        // Arrange
        var originalData = new byte[] { 1, 2, 3 };
        var patchData = Array.Empty<byte>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            InvokeApplyPatchAsync(originalData, patchData));
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    // Helper method to invoke the private ApplyPatchAsync method using reflection
    private async Task<byte[]> InvokeApplyPatchAsync(byte[] originalData, byte[] patchData)
    {
        var method = typeof(DowngraderService).GetMethod("ApplyPatchAsync",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (method == null)
            throw new InvalidOperationException("ApplyPatchAsync method not found");

        var task = (Task<byte[]>)method.Invoke(_service, new object[] { originalData, patchData })!;
        return await task;
    }
}
