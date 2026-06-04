CREATE TABLE Contacts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Phone TEXT NOT NULL
);

INSERT INTO Contacts (Name, Phone) VALUES
    ('Иванов Иван', '+7 (999) 123-45-67'),
    ('Петрова Мария', '+7 (999) 765-43-21'),
    ('Сидоров Алексей', '+7 (999) 555-88-99');
