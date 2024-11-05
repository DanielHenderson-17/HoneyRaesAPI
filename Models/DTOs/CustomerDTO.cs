namespace HoneyRaesAPI.Models.DTOs;

public class CustomerDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<ServiceTicketDTO> ServiceTickets { get; set; } = new List<ServiceTicketDTO>();
}
