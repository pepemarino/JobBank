using JobBank.Management.Plugins;
using JobBank.Services.Abstraction;
using Microsoft.SemanticKernel;
using Moq;

namespace WORKSCommons.Tests;

[TestClass]
public class InterviewHistoryPluginIntegrationTest
{
    [TestMethod]
    public async Task PluginIntegration_WithKernel_ExecutesSuccessfully()
    {
        // Arrange
        var mockInterviewService = new Mock<IInterviewService>();
        mockInterviewService
            .Setup(s => s.GetGapsForApplicantAsync("user123"))
            .ReturnsAsync(new List<string> { "C# Basics", "OOP Principles" });

        var plugin = new InterviewHistoryPlugin(mockInterviewService.Object);

        var builder = Kernel.CreateBuilder();
        var kernel = builder.Build();

        kernel.Plugins.AddFromObject(plugin, "InterviewHistory");

        // Act
        var result = await kernel.InvokeAsync(
            "InterviewHistory",
            "GetPastFailures",
            new KernelArguments { { "userId", "user123" } });

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("C# Basics, OOP Principles", result.ToString());
    }
}