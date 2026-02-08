using Microsoft.AspNetCore.Authorization;

namespace SmartTicket.API.Security;

public sealed class TicketReadRequirement : IAuthorizationRequirement;
public sealed class TicketWriteRequirement : IAuthorizationRequirement;
public sealed class TicketAssignRequirement : IAuthorizationRequirement;
