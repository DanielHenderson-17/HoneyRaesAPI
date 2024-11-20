using HoneyRaesAPI.Models;
using HoneyRaesAPI.Models.DTOs;
using Npgsql;

var connectionString = "Host=localhost;Port=5432;Username=postgres;Password=root;Database=HoneyRaes";

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

app.MapGet("/servicetickets", async () =>
{
    var serviceTickets = new List<ServiceTicket>();

    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    var query = "SELECT * FROM ServiceTicket";
    using var command = new NpgsqlCommand(query, connection);
    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        serviceTickets.Add(new ServiceTicket
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
            EmployeeId = reader.IsDBNull(reader.GetOrdinal("EmployeeId")) ? null : reader.GetInt32(reader.GetOrdinal("EmployeeId")),
            Description = reader.GetString(reader.GetOrdinal("Description")),
            Emergency = reader.GetBoolean(reader.GetOrdinal("Emergency")),
            DateCompleted = reader.IsDBNull(reader.GetOrdinal("DateCompleted")) ? null : reader.GetDateTime(reader.GetOrdinal("DateCompleted"))
        });
    }

    return serviceTickets;
});

app.MapGet("/employees", async () =>
{
    var employees = new List<Employee>();

    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    var query = "SELECT * FROM Employee";
    using var command = new NpgsqlCommand(query, connection);
    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        employees.Add(new Employee
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Specialty = reader.GetString(reader.GetOrdinal("Specialty"))
        });
    }

    return employees;
});

app.MapGet("/customers", async () =>
{
    var customers = new List<Customer>();

    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    var query = "SELECT * FROM Customer";
    using var command = new NpgsqlCommand(query, connection);
    using var reader = await command.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        customers.Add(new Customer
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Address = reader.GetString(reader.GetOrdinal("Address"))
        });
    }

    return customers;
});

app.MapGet("/customers/{id}", async (int id) =>
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    Customer customer = null;
    var customerQuery = "SELECT * FROM Customer WHERE Id = @Id";
    using (var command = new NpgsqlCommand(customerQuery, connection))
    {
        command.Parameters.AddWithValue("Id", id);
        using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            customer = new Customer
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Address = reader.GetString(reader.GetOrdinal("Address"))
            };
        }
    }

    if (customer == null) return Results.NotFound();

    var serviceTickets = new List<ServiceTicket>();
    var ticketQuery = "SELECT * FROM ServiceTicket WHERE CustomerId = @CustomerId";
    using (var command = new NpgsqlCommand(ticketQuery, connection))
    {
        command.Parameters.AddWithValue("CustomerId", id);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            serviceTickets.Add(new ServiceTicket
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                EmployeeId = reader.IsDBNull(reader.GetOrdinal("EmployeeId")) ? null : reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Emergency = reader.GetBoolean(reader.GetOrdinal("Emergency")),
                DateCompleted = reader.IsDBNull(reader.GetOrdinal("DateCompleted")) ? null : reader.GetDateTime(reader.GetOrdinal("DateCompleted"))
            });
        }
    }

    return Results.Ok(new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        ServiceTickets = serviceTickets.Select(t => new ServiceTicketDTO
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
    Employee employee = null;

    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();

    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        SELECT 
            e.Id,
            e.Name, 
            e.Specialty, 
            st.Id AS serviceTicketId, 
            st.CustomerId,
            st.Description,
            st.Emergency,
            st.DateCompleted 
        FROM Employee e
        LEFT JOIN ServiceTicket st ON st.EmployeeId = e.Id
        WHERE e.Id = @id";
    command.Parameters.AddWithValue("@id", id);

    using NpgsqlDataReader reader = command.ExecuteReader();

    while (reader.Read())
    {
        if (employee == null)
        {
            employee = new Employee
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Specialty = reader.GetString(reader.GetOrdinal("Specialty")),
                ServiceTickets = new List<ServiceTicket>()
            };
        }

        if (!reader.IsDBNull(reader.GetOrdinal("serviceTicketId")))
        {
            employee.ServiceTickets.Add(new ServiceTicket
            {
                Id = reader.GetInt32(reader.GetOrdinal("serviceTicketId")),
                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                EmployeeId = id,
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Emergency = reader.GetBoolean(reader.GetOrdinal("Emergency")),
                DateCompleted = reader.IsDBNull(reader.GetOrdinal("DateCompleted"))
                                ? null
                                : reader.GetDateTime(reader.GetOrdinal("DateCompleted"))
            });
        }
    }

    return employee == null ? Results.NotFound() : Results.Ok(employee);
});


//Post endpoints
app.MapPost("/employees", (Employee employee) =>
{
    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();
    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        INSERT INTO Employee (Name, Specialty)
        VALUES (@name, @specialty)
        RETURNING Id
    ";
    command.Parameters.AddWithValue("@name", employee.Name);
    command.Parameters.AddWithValue("@specialty", employee.Specialty);

    employee.Id = (int)command.ExecuteScalar();

    return Results.Created($"/employees/{employee.Id}", employee);
});

//Put endpoints
app.MapPut("/employees/{id}", (int id, Employee employee) =>
{
    if (id != employee.Id)
    {
        return Results.BadRequest();
    }

    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();
    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        UPDATE Employee 
        SET Name = @name,
            Specialty = @specialty
        WHERE Id = @id
    ";
    command.Parameters.AddWithValue("@name", employee.Name);
    command.Parameters.AddWithValue("@specialty", employee.Specialty);
    command.Parameters.AddWithValue("@id", id);

    command.ExecuteNonQuery();

    return Results.NoContent();
});



//Delete endpoints
app.MapDelete("/employees/{id}", (int id) =>
{
    using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
    connection.Open();
    using NpgsqlCommand command = connection.CreateCommand();
    command.CommandText = @"
        DELETE FROM Employee WHERE Id = @id
    ";
    command.Parameters.AddWithValue("@id", id);
    command.ExecuteNonQuery();
    return Results.NoContent();
});


app.Run();
