using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartTicket.Application.DTOs.Auth;
using System.Net;
using System.Net.Http.Json;

namespace SmartTicket.IntegrationTests;

public class AuthApiTests : IClassFixture<SmartTicketApiFactory>
{
    private const string RefreshCookieName = "refresh_token";
    private readonly SmartTicketApiFactory _factory;

    public AuthApiTests(SmartTicketApiFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_setta_cookie_refresh_e_ritorna_access_token()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });

        var reg = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "t1@test.it",
            Password = "P@ssw0rd123!"
        });
        reg.StatusCode.Should().Be(HttpStatusCode.OK);
        CookieHelper.ExtractCookieValue(reg, RefreshCookieName).Should().NotBeNull();

        var res = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "t1@test.it",
            Password = "P@ssw0rd123!"
        });

        res.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await res.Content.ReadFromJsonAsync<AuthResponseDto>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();

        CookieHelper.ExtractCookieValue(res, RefreshCookieName)
            .Should().NotBeNullOrWhiteSpace();
    }
    [Fact]
    public async Task Refresh_ruota_refresh_cookie_e_ritorna_nuovo_access_token()
    {
        var httpHandler = new HttpClientHandler { UseCookies = false };
        var client = _factory.CreateDefaultClient(new PassThroughHandler(httpHandler));
        client.BaseAddress = new Uri("https://localhost");

        var reg = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "t2@test.it",
            Password = "P@ssw0rd123!"
        });

        var rt1 = CookieHelper.ExtractCookieValue(reg, RefreshCookieName);
        rt1.Should().NotBeNullOrWhiteSpace();

        var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        req.Headers.Add("Cookie", $"{RefreshCookieName}={rt1}");

        var refreshRes = await client.SendAsync(req);
        refreshRes.StatusCode.Should().Be(HttpStatusCode.OK);
    }


    [Fact]
    public async Task Single_session_login_secondo_invalida_refresh_vecchio()
    {
        var httpHandler = new HttpClientHandler { UseCookies = false };
        var client = _factory.CreateDefaultClient(new PassThroughHandler(httpHandler));

        await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "t3@test.it",
            Password = "P@ssw0rd123!"
        });

        var login1 = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "t3@test.it",
            Password = "P@ssw0rd123!"
        });
        var rt1 = CookieHelper.ExtractCookieValue(login1, RefreshCookieName);
        rt1.Should().NotBeNullOrWhiteSpace();

   
        var login2 = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = "t3@test.it",
            Password = "P@ssw0rd123!"
        });
        var rt2 = CookieHelper.ExtractCookieValue(login2, RefreshCookieName);
        rt2.Should().NotBeNullOrWhiteSpace();
        rt2.Should().NotBe(rt1);

  
        var reqOld = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        reqOld.Headers.Add("Cookie", $"{RefreshCookieName}={rt1}");

        var refreshOld = await client.SendAsync(reqOld);
        refreshOld.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_revoca_refresh_e_refresh_successivo_fallisce()
    {
        var httpHandler = new HttpClientHandler { UseCookies = false };
        var client = _factory.CreateDefaultClient(new PassThroughHandler(httpHandler));

        var reg = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Email = "t4@test.it",
            Password = "P@ssw0rd123!"
        });
        var rt = CookieHelper.ExtractCookieValue(reg, RefreshCookieName);
        rt.Should().NotBeNullOrWhiteSpace();

        var logoutReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
        logoutReq.Headers.Add("Cookie", $"{RefreshCookieName}={rt}");

        var logoutRes = await client.SendAsync(logoutReq);
        logoutRes.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var refreshReq = new HttpRequestMessage(HttpMethod.Post, "/api/auth/refresh");
        refreshReq.Headers.Add("Cookie", $"{RefreshCookieName}={rt}");

        var refreshRes = await client.SendAsync(refreshReq);
        refreshRes.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
