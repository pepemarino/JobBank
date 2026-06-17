using JobBank.Components.Pages.UserSettingPages.ViewModels;
using JobBank.Management;
using JobBank.ModelsDTO;
using JobBank.Services.Abstraction;
using Microsoft.Extensions.Logging;
using Moq;

namespace WORKSCommons.Tests;

[TestClass]
public class SettingsViewModelTest
{
    private Mock<IUserSettingService> _mockUserSettingService;
    private Mock<IIdentityService> _mockIdentityService;
    private Mock<ILogger<ISkillsService>> _mockLogger;
    private SettingsViewModel _viewModel;

    private const string TestUserId = "test-user-123";
    private const string TestErrorMessage = "Test error message";

    [TestInitialize]
    public void Setup()
    {
        _mockUserSettingService = new Mock<IUserSettingService>();
        _mockIdentityService = new Mock<IIdentityService>();
        _mockLogger = new Mock<ILogger<ISkillsService>>();

        SetupDefaultMocks();

        _viewModel = new SettingsViewModel(
            _mockUserSettingService.Object,
            _mockIdentityService.Object,
            _mockLogger.Object);
    }

    private void SetupDefaultMocks()
    {
        _mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Verifiable();

        _mockIdentityService.Setup(x => x.GetUserIdAsync())
            .ReturnsAsync(TestUserId);

        _mockUserSettingService.Setup(x => x.GetUserSettingAsync(It.IsAny<string>()))
            .ReturnsAsync((UserSettingsDTO?)null);
    }

    #region InitializeAsync Tests

    [TestMethod]
    public async Task InitializeAsync_WithValidUser_LoadsSettingsSuccessfully()
    {
        // Arrange
        var expectedSettings = new UserSettingsDTO
        {
            Id = 1,
            UserId = TestUserId,
            UseTutorMode = true
        };

        _mockUserSettingService.Setup(x => x.GetUserSettingAsync(TestUserId))
            .ReturnsAsync(expectedSettings);

        var uiUpdateInvoked = false;
        _viewModel.OnRequestUIUpdate += () => uiUpdateInvoked = true;

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.AreEqual(TestUserId, _viewModel.UserId);
        Assert.IsNotNull(_viewModel.UserSettingsTracker);
        Assert.AreEqual(true, _viewModel.UserSettingsTracker.Current.UseTutorMode);
        Assert.IsNull(_viewModel.ErrorMessage);
        Assert.IsTrue(uiUpdateInvoked);
        _mockUserSettingService.Verify(x => x.GetUserSettingAsync(TestUserId), Times.Once);
    }

    [TestMethod]
    public async Task InitializeAsync_WithNoExistingSettings_CreatesNewSettings()
    {
        // Arrange
        _mockUserSettingService.Setup(x => x.GetUserSettingAsync(TestUserId))
            .ReturnsAsync((UserSettingsDTO?)null);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.AreEqual(TestUserId, _viewModel.UserId);
        Assert.IsNotNull(_viewModel.UserSettingsTracker);
        Assert.AreEqual(TestUserId, _viewModel.UserSettingsTracker.Current.UserId);
        Assert.AreEqual(false, _viewModel.UserSettingsTracker.Current.UseTutorMode);
        Assert.IsNull(_viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task InitializeAsync_WithNullUserId_SetsErrorMessage()
    {
        // Arrange
        _mockIdentityService.Setup(x => x.GetUserIdAsync())
            .ReturnsAsync((string?)null);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.IsNull(_viewModel.UserId);
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.AreEqual("Unable to determine current user.", _viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task InitializeAsync_WithEmptyUserId_SetsErrorMessage()
    {
        // Arrange
        _mockIdentityService.Setup(x => x.GetUserIdAsync())
            .ReturnsAsync(string.Empty);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.AreEqual(string.Empty, _viewModel.UserId);
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.AreEqual("Unable to determine current user.", _viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task InitializeAsync_WhenIdentityServiceThrows_CatchesExceptionAndSetError()
    {
        // Arrange
        var testException = new InvalidOperationException("Identity service failed");
        _mockIdentityService.Setup(x => x.GetUserIdAsync())
            .ThrowsAsync(testException);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.IsTrue(_viewModel.ErrorMessage.Contains("Error loading settings"));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task InitializeAsync_WhenUserSettingServiceThrows_CatchesExceptionAndSetError()
    {
        // Arrange
        var testException = new InvalidOperationException("Database error");
        _mockUserSettingService.Setup(x => x.GetUserSettingAsync(It.IsAny<string>()))
            .ThrowsAsync(testException);

        // Act
        await _viewModel.InitializeAsync();

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.IsTrue(_viewModel.ErrorMessage.Contains("Error loading settings"));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    #endregion

    #region SaveCommand Tests

    [TestMethod]
    public async Task SaveCommand_Execute_WithValidSettings_SavesSuccessfully()
    {
        // Arrange
        await _viewModel.InitializeAsync();
        _viewModel.UserSettingsTracker.Current.UseTutorMode = true;

        var uiUpdateInvoked = false;
        _viewModel.OnRequestUIUpdate += () => uiUpdateInvoked = true;

        _mockUserSettingService.Setup(x => x.SaveUserSettingAsync(It.IsAny<UserSettingsDTO>()))
            .Returns(Task.CompletedTask);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
        Assert.IsTrue(uiUpdateInvoked);
        _mockUserSettingService.Verify(
            x => x.SaveUserSettingAsync(It.Is<UserSettingsDTO>(s => s.UseTutorMode == true)),
            Times.Once);
    }

    [TestMethod]
    public async Task SaveCommand_Execute_WithNullUserId_SetsErrorMessage()
    {
        // Arrange
        _viewModel.UserId = null;
        _viewModel.UserSettingsTracker = new(new UserSettingsDTO { UseTutorMode = true });

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.AreEqual("User ID is not available. Please log in again.", _viewModel.ErrorMessage);
        _mockUserSettingService.Verify(x => x.SaveUserSettingAsync(It.IsAny<UserSettingsDTO>()), Times.Never);
    }

    [TestMethod]
    public async Task SaveCommand_Execute_WhenServiceThrows_CatchesExceptionAndSetError()
    {
        // Arrange
        await _viewModel.InitializeAsync();
        var testException = new InvalidOperationException("Save failed");
        _mockUserSettingService.Setup(x => x.SaveUserSettingAsync(It.IsAny<UserSettingsDTO>()))
            .ThrowsAsync(testException);

        // Act
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNotNull(_viewModel.ErrorMessage);
        Assert.IsTrue(_viewModel.ErrorMessage.Contains("Error saving settings"));
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                testException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SaveCommand_CanExecute_WhenNoChanges_ReturnsFalse()
    {
        // Arrange
        await _viewModel.InitializeAsync();

        // Act
        var canExecute = _viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.IsFalse(canExecute);
    }

    [TestMethod]
    public async Task SaveCommand_CanExecute_WhenChangesExist_ReturnsTrue()
    {
        // Arrange
        await _viewModel.InitializeAsync();
        _viewModel.UserSettingsTracker.Current.UseTutorMode = true;

        // Act
        var canExecute = _viewModel.SaveCommand.CanExecute(null);

        // Assert
        Assert.IsTrue(canExecute);
    }

    #endregion

    #region ErrorMessage Tests

    [TestMethod]
    public void ErrorMessage_WhenSet_InvokesOnRequestUIUpdate()
    {
        // Arrange
        var uiUpdateInvoked = false;
        _viewModel.OnRequestUIUpdate += () => uiUpdateInvoked = true;

        // Act
        _viewModel.ErrorMessage = TestErrorMessage;

        // Assert
        Assert.IsTrue(uiUpdateInvoked);
        Assert.AreEqual(TestErrorMessage, _viewModel.ErrorMessage);
    }

    [TestMethod]
    public void ErrorMessage_WhenSet_RaisesPropertyChanged()
    {
        // Arrange
        var propertyChangedRaised = false;
        string? changedPropertyName = null;

        _viewModel.PropertyChanged += (s, e) =>
        {
            propertyChangedRaised = true;
            changedPropertyName = e.PropertyName;
        };

        // Act
        _viewModel.ErrorMessage = TestErrorMessage;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
        Assert.AreEqual(nameof(_viewModel.ErrorMessage), changedPropertyName);
    }

    [TestMethod]
    public void ClearError_SetsErrorMessageToNull()
    {
        // Arrange
        _viewModel.ErrorMessage = TestErrorMessage;

        // Act
        _viewModel.ClearError();

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_InitializesProperties()
    {
        // Assert
        Assert.IsNotNull(_viewModel.SaveCommand);
        Assert.AreEqual("User Settings", _viewModel.Title);
        Assert.IsNull(_viewModel.UserId);
        Assert.IsNull(_viewModel.ErrorMessage);
        Assert.IsNotNull(_viewModel.UserSettingsTracker);
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public async Task FullWorkflow_LoadSettingsModifyAndSave()
    {
        // Arrange
        var initialSettings = new UserSettingsDTO
        {
            Id = 1,
            UserId = TestUserId,
            UseTutorMode = false
        };

        _mockUserSettingService.Setup(x => x.GetUserSettingAsync(TestUserId))
            .ReturnsAsync(initialSettings);

        _mockUserSettingService.Setup(x => x.SaveUserSettingAsync(It.IsAny<UserSettingsDTO>()))
            .Returns(Task.CompletedTask);

        var uiUpdateCount = 0;
        _viewModel.OnRequestUIUpdate += () => uiUpdateCount++;

        // Act - Initialize
        await _viewModel.InitializeAsync();
        Assert.IsFalse(_viewModel.SaveCommand.CanExecute(null), "Save should be disabled initially");

        // Act - Modify
        _viewModel.UserSettingsTracker.Current.UseTutorMode = true;
        Assert.IsTrue(_viewModel.SaveCommand.CanExecute(null), "Save should be enabled after modification");

        // Act - Save
        await _viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.IsNull(_viewModel.ErrorMessage);
        Assert.IsFalse(_viewModel.SaveCommand.CanExecute(null), "Save should be disabled after successful save");
        Assert.IsTrue(uiUpdateCount >= 2, "UI should update during initialization and save");

        _mockUserSettingService.Verify(
            x => x.SaveUserSettingAsync(It.Is<UserSettingsDTO>(s => s.UseTutorMode == true)),
            Times.Once);
    }

    #endregion
}
