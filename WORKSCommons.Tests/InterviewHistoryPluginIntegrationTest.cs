using JobBank.Management.Plugins;
using JobBank.Services.Abstraction;
using Microsoft.SemanticKernel;
using Moq;

namespace WORKSCommons.Tests;

[TestClass]
public class InterviewHistoryPluginIntegrationTest
{
    private Mock<IInterviewService> _mockInterviewService;
    private Kernel _kernel;

    [TestInitialize]
    public void Setup()
    {
        _mockInterviewService = new Mock<IInterviewService>();
        var builder = Kernel.CreateBuilder();
        _kernel = builder.Build();
    }

    private void RegisterPlugin()
    {
        var plugin = new InterviewHistoryPlugin(_mockInterviewService.Object);
        _kernel.Plugins.AddFromObject(plugin, "InterviewHistory");
    }

    [TestMethod]
    public void PluginIntegration_Kernel_Successfully_Registration()
    {
        // Arrange
        _mockInterviewService
            .Setup(s => s.GetGapsForApplicantAsync("user123"))
            .ReturnsAsync(new List<string> { "C# Basics", "OOP Principles" });

        RegisterPlugin();

        // Act
        var function = _kernel.Plugins["InterviewHistory"]["GetPastFailures"];

        // Assert
        Assert.IsNotNull(function);
    }

    [TestMethod]
    public async Task PluginIntegration_WithKernel_ExecutesSuccessfully()
    {
        // Arrange
        _mockInterviewService
            .Setup(s => s.GetGapsForApplicantAsync("user123"))
            .ReturnsAsync(new List<string> { "C# Basics", "OOP Principles" });

        RegisterPlugin();

        // Act
        var result = await _kernel.InvokeAsync(
            "InterviewHistory",
            "GetPastFailures",
            new KernelArguments { { "userId", "user123" } });

        // Assert
        _mockInterviewService.Verify(
            s => s.GetGapsForApplicantAsync("user123"),
            Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual("C# Basics, OOP Principles", result.ToString());
    }

    [TestMethod]
    public async Task PluginIntegration_Returns_EmptyString_WithKernel()
    {
        // Arrange
        _mockInterviewService
            .Setup(s => s.GetGapsForApplicantAsync("user123"))
            .ReturnsAsync(new List<string>());

        RegisterPlugin();

        // Act
        var result = await _kernel.InvokeAsync(
            "InterviewHistory",
            "GetPastFailures",
            new KernelArguments { { "userId", "user123" } });

        // Assert
        _mockInterviewService.Verify(
            s => s.GetGapsForApplicantAsync("user123"),
            Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual(string.Empty, result.ToString());
    }

    [TestMethod]
    public async Task PluginIntegration_Returns_EmptyString_ForNull_WithKernel()
    {
        // Arrange
        _mockInterviewService
            .Setup(s => s.GetGapsForApplicantAsync("user123"))
            .ReturnsAsync((List<string>)null);

        RegisterPlugin();

        // Act
        var result = await _kernel.InvokeAsync(
            "InterviewHistory",
            "GetPastFailures",
            new KernelArguments { { "userId", "user123" } });

        // Assert
        _mockInterviewService.Verify(
            s => s.GetGapsForApplicantAsync("user123"),
            Times.Once);

        Assert.IsNotNull(result);
        Assert.AreEqual(string.Empty, result.ToString());
    }
}