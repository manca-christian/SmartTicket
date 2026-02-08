namespace SmartTicket.API.Integrations;

public class FakeDownstreamClient
{
    private readonly HttpClient _http;

    public FakeDownstreamClient(HttpClient http) => _http = http;

    public async Task<string> GetStatusAsync(CancellationToken ct = default)
    {
        var res = await _http.GetAsync("/status", ct);
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadAsStringAsync(ct);
    }
}
