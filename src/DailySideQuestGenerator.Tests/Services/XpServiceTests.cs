using DailySideQuestGenerator.Models;
using DailySideQuestGenerator.Services;
using DailySideQuestGenerator.Services.Interfaces;
using NSubstitute;

namespace DailySideQuestGenerator.Tests.Services;

public class XpServiceTests
{
    private readonly IUserProgressService _userProgressService;
    private readonly XpService _xpService;

    public XpServiceTests()
    {
        _userProgressService = Substitute.For<IUserProgressService>();
        _userProgressService.Progress.Returns(new UserProgress());
        _xpService = new XpService(_userProgressService);
    }

    #region Level Calculation Tests

    [Theory]
    [InlineData(0, 1)]
    [InlineData(99, 1)]
    [InlineData(100, 2)]
    [InlineData(399, 2)]
    [InlineData(400, 3)]
    [InlineData(899, 3)]
    [InlineData(900, 4)]
    [InlineData(1600, 5)]
    [InlineData(8100, 10)]
    [InlineData(36100, 20)]
    public void CalculateLevel_ReturnsCorrectLevel(int totalXp, int expectedLevel)
    {
        // Act
        var result = _xpService.CalculateLevel(totalXp);

        // Assert
        Assert.Equal(expectedLevel, result);
    }

    [Theory]
    [InlineData(-100, 1)]
    [InlineData(-1, 1)]
    public void CalculateLevel_WithNegativeXp_ReturnsLevelOne(int totalXp, int expectedLevel)
    {
        // Act
        var result = _xpService.CalculateLevel(totalXp);

        // Assert
        Assert.Equal(expectedLevel, result);
    }

    #endregion

    #region XP For Level Tests

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 100)]
    [InlineData(3, 400)]
    [InlineData(4, 900)]
    [InlineData(5, 1600)]
    [InlineData(10, 8100)]
    [InlineData(20, 36100)]
    public void GetXpForLevel_ReturnsCorrectXp(int level, int expectedXp)
    {
        // Act
        var result = _xpService.GetXpForLevel(level);

        // Assert
        Assert.Equal(expectedXp, result);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(-1, 0)]
    public void GetXpForLevel_WithInvalidLevel_ReturnsZero(int level, int expectedXp)
    {
        // Act
        var result = _xpService.GetXpForLevel(level);

        // Assert
        Assert.Equal(expectedXp, result);
    }

    #endregion

    #region Streak Multiplier Tests

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 5)]
    [InlineData(2, 10)]
    [InlineData(3, 15)]
    [InlineData(4, 20)]
    [InlineData(5, 30)]
    [InlineData(6, 40)]
    [InlineData(7, 50)]
    [InlineData(10, 50)]
    [InlineData(100, 50)]
    public void GetStreakMultiplier_ReturnsCorrectBonus(int streak, int expectedBonus)
    {
        // Act
        var result = _xpService.GetStreakMultiplier(streak);

        // Assert
        Assert.Equal(expectedBonus, result);
    }

    #endregion

    #region Apply Streak Bonus Tests

    [Theory]
    [InlineData(100, 0, 100)]   // No streak = no bonus
    [InlineData(100, 1, 105)]   // 5% bonus
    [InlineData(100, 3, 115)]   // 15% bonus
    [InlineData(100, 7, 150)]   // 50% bonus (max)
    [InlineData(50, 7, 75)]     // 50% of 50 = 25, total = 75
    [InlineData(33, 3, 38)]     // 15% of 33 = 4.95, rounded up = 5, total = 38
    public void ApplyStreakBonus_ReturnsCorrectTotal(int baseXp, int streak, int expectedTotal)
    {
        // Act
        var result = _xpService.ApplyStreakBonus(baseXp, streak);

        // Assert
        Assert.Equal(expectedTotal, result);
    }

    #endregion

    #region GetLevelInfo Tests

    [Fact]
    public void GetLevelInfo_ReturnsCorrectLevelInfo()
    {
        // Arrange
        var progress = new UserProgress
        {
            TotalXp = 500,  // Should be level 3 (400-899)
            DailyStreak = 3
        };
        _userProgressService.Progress.Returns(progress);

        // Act
        var result = _xpService.GetLevelInfo();

        // Assert
        Assert.Equal(3, result.Level);
        Assert.Equal(500, result.TotalXp);
        Assert.Equal(400, result.XpForCurrentLevel);
        Assert.Equal(900, result.XpForNextLevel);
        Assert.Equal(100, result.XpProgressInLevel);  // 500 - 400
        Assert.Equal(500, result.XpNeededForNextLevel);  // 900 - 400
        Assert.Equal(20, result.ProgressPercentage);  // 100/500 * 100
        Assert.Equal(3, result.DailyStreak);
        Assert.Equal(15, result.StreakMultiplier);  // 3-day streak = 15%
    }

    [Fact]
    public void GetLevelInfo_AtLevelStart_ReturnsZeroProgress()
    {
        // Arrange
        var progress = new UserProgress { TotalXp = 400, DailyStreak = 0 };  // Exactly at level 3
        _userProgressService.Progress.Returns(progress);

        // Act
        var result = _xpService.GetLevelInfo();

        // Assert
        Assert.Equal(3, result.Level);
        Assert.Equal(0, result.XpProgressInLevel);
        Assert.Equal(0, result.ProgressPercentage);
    }

    #endregion

    #region LevelInfo Title Tests

    [Theory]
    [InlineData(1, "Novice Adventurer")]
    [InlineData(4, "Novice Adventurer")]
    [InlineData(5, "Apprentice Quester")]
    [InlineData(9, "Apprentice Quester")]
    [InlineData(10, "Journeyman Hero")]
    [InlineData(19, "Journeyman Hero")]
    [InlineData(20, "Seasoned Champion")]
    [InlineData(34, "Seasoned Champion")]
    [InlineData(35, "Veteran Warrior")]
    [InlineData(49, "Veteran Warrior")]
    [InlineData(50, "Elite Guardian")]
    [InlineData(74, "Elite Guardian")]
    [InlineData(75, "Master Legend")]
    [InlineData(99, "Master Legend")]
    [InlineData(100, "Mythic Paragon")]
    [InlineData(200, "Mythic Paragon")]
    public void LevelInfo_Title_ReturnsCorrectTitle(int level, string expectedTitle)
    {
        // Arrange
        var levelInfo = new LevelInfo { Level = level };

        // Assert
        Assert.Equal(expectedTitle, levelInfo.Title);
    }

    #endregion

    #region AwardQuestXpAsync Tests

    [Fact]
    public async Task AwardQuestXpAsync_AddsXpAndUpdatesStreak()
    {
        // Arrange
        var progress = new UserProgress { TotalXp = 0, DailyStreak = 2 };
        _userProgressService.Progress.Returns(progress);

        // Act
        _ = await _xpService.AwardQuestXpAsync(50);

        // Assert
        await _userProgressService.Received(1).AddXpAsync(55);  // 50 + 10% streak bonus
        await _userProgressService.Received(1).UpdateStreakAsync();
    }

    [Fact]
    public async Task AwardQuestXpAsync_ReturnsCorrectXpEvent()
    {
        // Arrange
        var progress = new UserProgress { TotalXp = 0, DailyStreak = 3 };
        _userProgressService.Progress.Returns(progress);

        // Act
        var result = await _xpService.AwardQuestXpAsync(100);

        // Assert
        Assert.Equal(100, result.XpGained);
        Assert.Equal(15, result.StreakBonus);  // 15% of 100
    }

    [Fact]
    public async Task AwardQuestXpAsync_DetectsLevelUp()
    {
        // Arrange - Start at level 1 with 90 XP
        var progress = new UserProgress { TotalXp = 90, DailyStreak = 0 };
        _userProgressService.Progress.Returns(progress);
        
        // Setup to return updated progress after AddXpAsync
        _userProgressService.When(x => x.AddXpAsync(Arg.Any<int>()))
            .Do(_ => {
                var updatedProgress = new UserProgress { TotalXp = 110, DailyStreak = 0 };
                _userProgressService.Progress.Returns(updatedProgress);
            });

        // Act
        var result = await _xpService.AwardQuestXpAsync(20);

        // Assert
        Assert.Equal(1, result.PreviousLevel);
        Assert.Equal(2, result.NewLevel);
        Assert.True(result.LeveledUp);
    }

    #endregion

    #region RemoveQuestXpAsync Tests

    [Fact]
    public async Task RemoveQuestXpAsync_RemovesXpAndDecrementsStreak()
    {
        // Arrange
        var progress = new UserProgress { TotalXp = 500, DailyStreak = 3 };
        _userProgressService.Progress.Returns(progress);

        // Act
        await _xpService.RemoveQuestXpAsync(50);

        // Assert
        await _userProgressService.Received(1).RemoveXpAsync(50);
        await _userProgressService.Received(1).DecrementStreakAsync();
    }

    [Fact]
    public async Task RemoveQuestXpAsync_ReturnsUpdatedLevelInfo()
    {
        // Arrange
        var progress = new UserProgress { TotalXp = 500, DailyStreak = 3 };
        _userProgressService.Progress.Returns(progress);

        // Act
        var result = await _xpService.RemoveQuestXpAsync(50);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<LevelInfo>(result);
    }

    #endregion
}

