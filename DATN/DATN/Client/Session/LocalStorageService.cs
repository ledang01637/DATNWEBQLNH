using Microsoft.JSInterop;
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
}
