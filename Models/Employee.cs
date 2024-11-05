namespace HoneyRaesAPI.Models;

public class Employee
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Specialty { get; set; }
    public List<ServiceTicket> ServiceTickets { get; set; } = new List<ServiceTicket>();
}
