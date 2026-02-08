using SmartTicket.Domain.Entities;

namespace SmartTicket.Application.Interfaces;

public interface IJwtTokenService
{
    string CreateToken(User user);
}
