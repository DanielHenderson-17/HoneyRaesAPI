using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;

List<Customer> customers = new List<Customer> { 
    new Customer { Id = 1, Name = "John Doe", Address = "123 Main"},
    new Customer { Id = 2, Name = "Jane Doe", Address = "123 Main"},
    new Customer { Id = 3, Name = "Jim Doe", Address = "123 Main"},
};

List<Employee> employees = new List<Employee> { 
    new Employee { Id = 1, Name = "Eve Adams", Specialty = "Plumbing" },
    new Employee { Id = 2, Name = "Frank Baker", Specialty = "Electrical" }
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket> { 
    new ServiceTicket { Id = 1, CustomerId = 1, EmployeeId = 1, Description = "Leaking faucet", Emergency = false },
    new ServiceTicket { Id = 2, CustomerId = 2, EmployeeId = 2, Description = "Power outage", Emergency = true, DateCompleted = DateTime.Now.AddDays(-1) },
    new ServiceTicket { Id = 3, CustomerId = 3, Description = "Broken window", Emergency = false },
    new ServiceTicket { Id = 4, CustomerId = 1, EmployeeId = 2, Description = "Heating issue", Emergency = true },
    new ServiceTicket { Id = 5, CustomerId = 2, EmployeeId = 1, Description = "Air conditioning maintenance", Emergency = false, DateCompleted = DateTime.Now.AddDays(-2) }
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();



//Get Endpoints
app.MapGet("/servicetickets", () =>
{
    return serviceTickets.Select(t => new ServiceTicketDTO
    {
        Id = t.Id,
        CustomerId = t.CustomerId,
        EmployeeId = t.EmployeeId,
        Description = t.Description,
        Emergency = t.Emergency,
        DateCompleted = t.DateCompleted
    });
});

app.MapGet("/employees", () =>
{
    return employees.Select(e => new EmployeeDTO
    {
        Id = e.Id,
        Name = e.Name,
        Specialty = e.Specialty
    }).ToList();
});

app.MapGet("/customers", () =>
{
    return customers.Select(c => new CustomerDTO
    {
        Id = c.Id,
        Name = c.Name,
        Address = c.Address
    }).ToList();
});

app.MapGet("/customers/{id}", (int id) =>
{
    var customer = customers.FirstOrDefault(c => c.Id == id);

    if (customer == null)
    {
        return Results.NotFound("Customer not found");
    }

    var tickets = serviceTickets.Where(st => st.CustomerId == id).ToList();

    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

app.MapGet("/employees/{id}", (int id) =>
{
    var employee = employees.FirstOrDefault(e => e.Id == id);

    if (employee == null)
    {
        return Results.NotFound("Employee not found");
    }

    var tickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();

    return Results.Ok(new EmployeeDTO
    {
        Id = employee.Id,
        Name = employee.Name,
        Specialty = employee.Specialty,
        ServiceTickets = tickets.Select(t => new ServiceTicketDTO
        {
            Id = t.Id,
            CustomerId = t.CustomerId,
            EmployeeId = t.EmployeeId,
            Description = t.Description,
            Emergency = t.Emergency,
            DateCompleted = t.DateCompleted
        }).ToList()
    });
});

app.MapGet("/servicetickets/{id}", (int id) =>
{
    var serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    var employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    var customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    return Results.Ok(new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = customer == null ? null : new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        EmployeeId = serviceTicket.EmployeeId,
        Employee = employee == null ? null : new EmployeeDTO
        {
            Id = employee.Id,
            Name = employee.Name,
            Specialty = employee.Specialty
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency,
        DateCompleted = serviceTicket.DateCompleted
    });
});


//Post Endpoints
app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{

    Customer customer = customers.FirstOrDefault(c => c.Id == serviceTicket.CustomerId);

    if (customer == null)
    {
        return Results.BadRequest();
    }

    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);

    return Results.Created($"/servicetickets/{serviceTicket.Id}", new ServiceTicketDTO
    {
        Id = serviceTicket.Id,
        CustomerId = serviceTicket.CustomerId,
        Customer = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address
        },
        Description = serviceTicket.Description,
        Emergency = serviceTicket.Emergency
    });

});

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToComplete == null)
    {
        return Results.NotFound();
    }

    ticketToComplete.DateCompleted = DateTime.Today;

    return Results.NoContent();
});



//Delete Endpoints
app.MapDelete("/servicetickets/{id}", (int id) =>
{
    var serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    serviceTickets.Remove(serviceTicket);

    return Results.NoContent();
});


//Put Endpoints
app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);

    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }

    ticketToUpdate.CustomerId = serviceTicket.CustomerId;
    ticketToUpdate.EmployeeId = serviceTicket.EmployeeId;
    ticketToUpdate.Description = serviceTicket.Description;
    ticketToUpdate.Emergency = serviceTicket.Emergency;
    ticketToUpdate.DateCompleted = serviceTicket.DateCompleted;

    return Results.NoContent();
});



app.Run();
