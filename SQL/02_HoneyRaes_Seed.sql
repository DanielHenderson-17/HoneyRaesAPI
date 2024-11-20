\c HoneyRaes

INSERT INTO Customer (Name, Address) VALUES 
    ('John Doe', '123 Main St'),
    ('Jane Smith', '456 Elm St');

INSERT INTO Employee (Name, Specialty) VALUES 
    ('Alice Johnson', 'Plumbing'),
    ('Bob Brown', 'Electrical');

INSERT INTO ServiceTicket (CustomerId, EmployeeId, Description, Emergency) VALUES 
    (1, 1, 'Fix leaking faucet', FALSE),
    (2, NULL, 'Install new outlet', TRUE);
