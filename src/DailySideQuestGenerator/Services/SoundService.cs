using Microsoft.JSInterop;

namespace DailySideQuestGenerator.Services;

/// <summary>
/// Service for playing sound effects in the application
/// </summary>
public interface ISoundService
{
    Task PlayXpGainAsync();
    Task PlayLevelUpAsync();
    Task PlayQuestCompleteAsync();
}

public class SoundService(IJSRuntime jsRuntime) : ISoundService
{
    public async Task PlayXpGainAsync()
    {
        await PlaySoundAsync("xp-gain");
    }

    public async Task PlayLevelUpAsync()
    {
        await PlaySoundAsync("level-up");
    }

    public async Task PlayQuestCompleteAsync()
    {
        await PlaySoundAsync("quest-complete");
    }

    private async Task PlaySoundAsync(string soundName)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("playSound", soundName);
        }
        catch
        {
            // Silently fail if sound cannot be played
        }
    }
}

