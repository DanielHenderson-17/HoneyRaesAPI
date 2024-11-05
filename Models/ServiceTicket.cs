namespace HoneyRaesAPI.Models;

public class ServiceTicket
{
    public int Id { get; set; }
    public int? CustomerId { get; set; }
    public int? EmployeeId { get; set; }
    public required string Description { get; set; }
    public bool Emergency { get; set; }
    public DateTime? DateCompleted { get; set; }
}
