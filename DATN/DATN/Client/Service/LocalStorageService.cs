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

    public async Task RemoveItemAsync(string key)
    {
        await _jsRuntime.InvokeAsync<object>("localStorage.removeItem", key);
    }

    public async Task<List<Cart>> GetCartItemAsync(string key)
    {
        var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);

        if (string.IsNullOrEmpty(json))
        {
            return new List<Cart>();
        }

        return JsonSerializer.Deserialize<List<Cart>>(json);
    }

    public async Task SetCartItemAsync(string key, List<Cart> value)
    {
        var json = JsonSerializer.Serialize(value);

        await _jsRuntime.InvokeAsync<object>("localStorage.setItem", key, json);
    }


    public async Task RemoveCartItemAsync(string key)
    {
        await _jsRuntime.InvokeAsync<object>("localStorage.removeItem", key);
    }
}
