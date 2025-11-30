namespace DailySideQuestGenerator.Services.Interfaces;

public interface IStorageService
{
    Task SetItemAsync<T>(string key, T item);
    Task<T?> GetItemAsync<T>(string key);
    Task RemoveItemAsync(string key);
    Task<bool> ContainKeyAsync(string key);
    Task ClearAsync();
}