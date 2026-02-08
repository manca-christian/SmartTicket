namespace SmartTicket.Application.DTOs.Tickets;

public class TicketQueryDto
{
    public int Page { get; set; } = 1;         
    public int PageSize { get; set; } = 20;    
    public string? Search { get; set; }        
    public string? Status { get; set; }      
    public bool? Assigned { get; set; }       
    public Guid? AssignedToUserId { get; set; }
}
