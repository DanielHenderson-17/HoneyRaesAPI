namespace HoneyRaesAPI.Models.DTOs;

public class EmployeeDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public List<ServiceTicketDTO> ServiceTickets { get; set; } = new List<ServiceTicketDTO>();
}
