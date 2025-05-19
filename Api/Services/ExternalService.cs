public class ExternalService
{
    private readonly HttpClient _http;
    public ExternalService(HttpClient http) => _http = http;



    public async Task<string> GetDataAsync()
    {
        var response = await _http.GetAsync("https://api.example.com/data");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}