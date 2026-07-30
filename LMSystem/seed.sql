USE LMS;
GO

-- Login table
CREATE TABLE logintab (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50),
    Password NVARCHAR(50)
);

INSERT INTO logintab (Username, Password) VALUES ('admin', '12345');
INSERT INTO logintab (Username, Password) VALUES ('mycodingproject', 'myc546');
INSERT INTO logintab (Username, Password) VALUES ('my', 'myc');

-- Librarians table
CREATE TABLE Librarians (
    LibrarianId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),
    Age INT,
    Phone NVARCHAR(20)
);

INSERT INTO Librarians (Name, Age, Phone) VALUES
('Sarah Connor', 34, '555-0201'),
('John Doe', 28, '555-0202'),
('Michael Scott', 45, '555-0203'),
('Ellen Ripley', 39, '555-0204'),
('James Bond', 40, '555-0205');

-- Students table
CREATE TABLE Students (
    StudentId INT PRIMARY KEY IDENTITY(1,1),
    Student_Name NVARCHAR(100),
    Email NVARCHAR(100),
    Phone_Number NVARCHAR(20)
);

INSERT INTO Students (Student_Name, Email, Phone_Number) VALUES
('Alice Johnson', 'alice.j@email.com', '555-0101'),
('Bob Smith', 'bob.smith@email.com', '555-0102'),
('Charlie Brown', 'charlie.b@email.com', '555-0103'),
('Diana Prince', 'diana.p@email.com', '555-0104'),
('Evan Wright', 'evan.w@email.com', '555-0105');
