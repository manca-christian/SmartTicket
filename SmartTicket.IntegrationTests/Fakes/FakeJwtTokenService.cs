using SmartTicket.Application.Interfaces;
using SmartTicket.Domain.Entities;

namespace SmartTicket.IntegrationTests.Fakes;

public sealed class FakeJwtTokenService : IJwtTokenService
{
    public string CreateToken(User user) => $"token-for-{user.Id}";
}
