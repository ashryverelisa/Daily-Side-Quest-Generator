using Blazored.LocalStorage;
using DailySideQuestGenerator.Services.Interfaces;

namespace DailySideQuestGenerator.Services;

public class StorageService(ILocalStorageService storageService) : IStorageService
{
    public async Task SetItemAsync<T>(string key, T item)
    {
        await storageService.SetItemAsync(key, item);
    }
    
    public async Task<T?> GetItemAsync<T>(string key)
    {
        return await storageService.GetItemAsync<T>(key);
    }
    
    public async Task RemoveItemAsync(string key)
    {
        await storageService.RemoveItemAsync(key);
    }
    
    public async Task<bool> ContainKeyAsync(string key) => await storageService.ContainKeyAsync(key);

    public async Task ClearAsync() => await storageService.ClearAsync();
}