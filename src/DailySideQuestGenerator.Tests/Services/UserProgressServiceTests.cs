using Blazored.LocalStorage;
using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services;
using NSubstitute;

namespace DailySideQuestGenerator.Tests.Services;

public class UserProgressServiceTests
{
    private readonly ILocalStorageService _localStorageService;
    private readonly UserProgressService _userProgressService;

    public UserProgressServiceTests()
    {
        _localStorageService = Substitute.For<ILocalStorageService>();
        _userProgressService = new UserProgressService(_localStorageService);
    }
    
    [Fact]
    public async Task LoadAsync_WhenNoStoredProgress_CreatesDefaultProgress()
    {
        // Arrange
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns((UserProgress?)null);

        // Act
        await _userProgressService.LoadAsync();

        // Assert
        Assert.Equal(0, _userProgressService.Progress.TotalXp);
        Assert.Equal(1, _userProgressService.Progress.Level);
        Assert.Equal(0, _userProgressService.Progress.DailyStreak);
    }

    [Fact]
    public async Task LoadAsync_WhenNoStoredProgress_SavesDefaultProgress()
    {
        // Arrange
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns((UserProgress?)null);

        // Act
        await _userProgressService.LoadAsync();

        // Assert
        await _localStorageService.Received(1).SetItemAsync(
            "userProgress",
            Arg.Is<UserProgress>(p => p.TotalXp == 0 && p.Level == 1 && p.DailyStreak == 0));
    }

    [Fact]
    public async Task LoadAsync_WhenStoredProgressExists_LoadsFromStorage()
    {
        // Arrange
        var storedProgress = new UserProgress
        {
            TotalXp = 100,
            Level = 4,
            DailyStreak = 5,
            LastQuestCompleted = new DateTime(2025, 12, 10)
        };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);

        // Act
        await _userProgressService.LoadAsync();

        // Assert
        Assert.Equal(100, _userProgressService.Progress.TotalXp);
        Assert.Equal(4, _userProgressService.Progress.Level);
        Assert.Equal(5, _userProgressService.Progress.DailyStreak);
        Assert.Equal(new DateTime(2025, 12, 10), _userProgressService.Progress.LastQuestCompleted);
    }

    [Fact]
    public async Task LoadAsync_WhenStoredProgressExists_DoesNotSave()
    {
        // Arrange
        var storedProgress = new UserProgress { TotalXp = 100 };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);

        // Act
        await _userProgressService.LoadAsync();

        // Assert
        await _localStorageService.DidNotReceive().SetItemAsync(
            Arg.Any<string>(),
            Arg.Any<UserProgress>());
    }
    
    [Fact]
    public async Task SaveAsync_PersistsCurrentProgress()
    {
        // Arrange
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns((UserProgress?)null);
        await _userProgressService.LoadAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _userProgressService.SaveAsync();

        // Assert
        await _localStorageService.Received(1).SetItemAsync("userProgress", _userProgressService.Progress);
    }
    
    [Fact]
    public async Task AddXpAsync_AddsXpToTotal()
    {
        // Arrange
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns((UserProgress?)null);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.AddXpAsync(25);

        // Assert
        Assert.Equal(25, _userProgressService.Progress.TotalXp);
    }

    [Fact]
    public async Task AddXpAsync_UpdatesLevel()
    {
        // Arrange
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns((UserProgress?)null);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.AddXpAsync(40); // Should be level 3: floor(sqrt(40/10)) + 1 = floor(2) + 1 = 3

        // Assert
        Assert.Equal(3, _userProgressService.Progress.Level);
    }

    [Fact]
    public async Task AddXpAsync_SavesProgress()
    {
        // Arrange
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns((UserProgress?)null);
        await _userProgressService.LoadAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _userProgressService.AddXpAsync(10);

        // Assert
        await _localStorageService.Received(1).SetItemAsync("userProgress", _userProgressService.Progress);
    }

    [Theory]
    [InlineData(0, 1)]   // 0 XP = level 1
    [InlineData(9, 1)]   // 9 XP = level 1 (floor(sqrt(0.9)) + 1 = 1)
    [InlineData(10, 2)]  // 10 XP = level 2 (floor(sqrt(1)) + 1 = 2)
    [InlineData(40, 3)]  // 40 XP = level 3 (floor(sqrt(4)) + 1 = 3)
    [InlineData(90, 4)]  // 90 XP = level 4 (floor(sqrt(9)) + 1 = 4)
    [InlineData(160, 5)] // 160 XP = level 5 (floor(sqrt(16)) + 1 = 5)
    public async Task AddXpAsync_CalculatesCorrectLevel(int xp, int expectedLevel)
    {
        // Arrange
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns((UserProgress?)null);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.AddXpAsync(xp);

        // Assert
        Assert.Equal(expectedLevel, _userProgressService.Progress.Level);
    }
    
    [Fact]
    public async Task RemoveXpAsync_RemovesXpFromTotal()
    {
        // Arrange
        var storedProgress = new UserProgress { TotalXp = 50, Level = 3 };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.RemoveXpAsync(20);

        // Assert
        Assert.Equal(30, _userProgressService.Progress.TotalXp);
    }

    [Fact]
    public async Task RemoveXpAsync_DoesNotGoBelowZero()
    {
        // Arrange
        var storedProgress = new UserProgress { TotalXp = 10, Level = 2 };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.RemoveXpAsync(50);

        // Assert
        Assert.Equal(0, _userProgressService.Progress.TotalXp);
    }

    [Fact]
    public async Task RemoveXpAsync_UpdatesLevel()
    {
        // Arrange
        var storedProgress = new UserProgress { TotalXp = 50, Level = 3 };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.RemoveXpAsync(45); // 5 XP remaining = level 1

        // Assert
        Assert.Equal(1, _userProgressService.Progress.Level);
    }

    [Fact]
    public async Task RemoveXpAsync_SavesProgress()
    {
        // Arrange
        var storedProgress = new UserProgress { TotalXp = 50, Level = 3 };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _userProgressService.RemoveXpAsync(10);

        // Assert
        await _localStorageService.Received(1).SetItemAsync("userProgress", _userProgressService.Progress);
    }
    
    [Fact]
    public async Task UpdateStreakAsync_WhenFirstQuestToday_IncrementsStreakFromYesterday()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var storedProgress = new UserProgress
        {
            DailyStreak = 3,
            LastQuestCompleted = yesterday
        };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.UpdateStreakAsync();

        // Assert
        Assert.Equal(4, _userProgressService.Progress.DailyStreak);
    }

    [Fact]
    public async Task UpdateStreakAsync_WhenFirstQuestToday_UpdatesLastQuestCompleted()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var storedProgress = new UserProgress
        {
            DailyStreak = 3,
            LastQuestCompleted = yesterday
        };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.UpdateStreakAsync();

        // Assert
        Assert.Equal(DateTime.UtcNow.Date, _userProgressService.Progress.LastQuestCompleted.Date);
    }

    [Fact]
    public async Task UpdateStreakAsync_WhenAlreadyCompletedToday_DoesNotChangeStreak()
    {
        // Arrange
        var today = DateTime.UtcNow.Date;
        var storedProgress = new UserProgress
        {
            DailyStreak = 5,
            LastQuestCompleted = today.AddHours(1) // Earlier today
        };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _userProgressService.UpdateStreakAsync();

        // Assert
        Assert.Equal(5, _userProgressService.Progress.DailyStreak);
        await _localStorageService.DidNotReceive().SetItemAsync(
            Arg.Any<string>(),
            Arg.Any<UserProgress>());
    }

    [Fact]
    public async Task UpdateStreakAsync_WhenStreakBroken_ResetsStreakToOne()
    {
        // Arrange
        var twoDaysAgo = DateTime.UtcNow.Date.AddDays(-2);
        var storedProgress = new UserProgress
        {
            DailyStreak = 10,
            LastQuestCompleted = twoDaysAgo
        };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.UpdateStreakAsync();

        // Assert
        Assert.Equal(1, _userProgressService.Progress.DailyStreak);
    }

    [Fact]
    public async Task UpdateStreakAsync_WhenNeverCompleted_StartsStreakAtOne()
    {
        // Arrange
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns((UserProgress?)null);
        await _userProgressService.LoadAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _userProgressService.UpdateStreakAsync();

        // Assert
        Assert.Equal(1, _userProgressService.Progress.DailyStreak);
    }

    [Fact]
    public async Task UpdateStreakAsync_SavesProgress()
    {
        // Arrange
        var yesterday = DateTime.UtcNow.Date.AddDays(-1);
        var storedProgress = new UserProgress
        {
            DailyStreak = 3,
            LastQuestCompleted = yesterday
        };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _userProgressService.UpdateStreakAsync();

        // Assert
        await _localStorageService.Received(1).SetItemAsync("userProgress", _userProgressService.Progress);
    }

    [Fact]
    public async Task DecrementStreakAsync_DecrementsStreak()
    {
        // Arrange
        var storedProgress = new UserProgress { DailyStreak = 5 };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.DecrementStreakAsync();

        // Assert
        Assert.Equal(4, _userProgressService.Progress.DailyStreak);
    }

    [Fact]
    public async Task DecrementStreakAsync_DoesNotGoBelowZero()
    {
        // Arrange
        var storedProgress = new UserProgress { DailyStreak = 0 };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();

        // Act
        await _userProgressService.DecrementStreakAsync();

        // Assert
        Assert.Equal(0, _userProgressService.Progress.DailyStreak);
    }

    [Fact]
    public async Task DecrementStreakAsync_SavesProgress()
    {
        // Arrange
        var storedProgress = new UserProgress { DailyStreak = 3 };
        _localStorageService.GetItemAsync<UserProgress>("userProgress")
            .Returns(storedProgress);
        await _userProgressService.LoadAsync();
        _localStorageService.ClearReceivedCalls();

        // Act
        await _userProgressService.DecrementStreakAsync();

        // Assert
        await _localStorageService.Received(1).SetItemAsync("userProgress", _userProgressService.Progress);
    }
    
    [Fact]
    public void Progress_BeforeLoad_ReturnsDefaultProgress()
    {
        // Assert
        Assert.NotNull(_userProgressService.Progress);
        Assert.Equal(0, _userProgressService.Progress.TotalXp);
        Assert.Equal(1, _userProgressService.Progress.Level);
        Assert.Equal(0, _userProgressService.Progress.DailyStreak);
    }
}