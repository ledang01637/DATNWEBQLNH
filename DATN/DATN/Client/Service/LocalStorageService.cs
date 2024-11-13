using DATN.Client.Pages.AdminManager;
using DATN.Shared;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

public class LocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetItemAsync(string key)
    {
        return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
    }

    public async Task SetItemAsync(string key, string value)
    {
        await _jsRuntime.InvokeAsync<object>("localStorage.setItem", key, value);
    }

    public async Task<T> GetAsync<T>(string key)
    {
        var json = await GetItemAsync(key);
        return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task SetAsync<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        await SetItemAsync(key, json);
    }

    public async Task<List<T>> GetListAsync<T>(string key)
    {
        var json = await GetItemAsync(key);
        return string.IsNullOrEmpty(json) ? new List<T>() : JsonSerializer.Deserialize<List<T>>(json);
    }

    public async Task SetListAsync<T>(string key, List<T> value)
    {
        var json = JsonSerializer.Serialize(value);
        await SetItemAsync(key, json);
    }


    public async Task<Dictionary<K, V>> GetDictionaryAsync<K, V>(string key)
    {
        var json = await GetItemAsync(key);
        return string.IsNullOrEmpty(json) ? new Dictionary<K, V>() : JsonSerializer.Deserialize<Dictionary<K, V>>(json);
    }

    public async Task SetDictionaryAsync<K, V>(string key, Dictionary<K, V> value)
    {
        var json = JsonSerializer.Serialize(value);
        await SetItemAsync(key, json);
    }
    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeAsync<object>("localStorage.removeItem", key);
    }
}
