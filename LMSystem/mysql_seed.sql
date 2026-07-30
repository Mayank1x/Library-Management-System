CREATE DATABASE IF NOT EXISTS LMS;
USE LMS;

-- Login table
CREATE TABLE IF NOT EXISTS logintab (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(50),
    Password VARCHAR(50)
);

INSERT INTO logintab (Username, Password) VALUES ('admin', '12345');
INSERT INTO logintab (Username, Password) VALUES ('mycodingproject', 'myc546');
INSERT INTO logintab (Username, Password) VALUES ('my', 'myc');

-- Librarians table
CREATE TABLE IF NOT EXISTS Librarians (
    LibrarianId INT PRIMARY KEY AUTO_INCREMENT,
    Name VARCHAR(100),
    Age INT,
    Phone VARCHAR(20)
);

INSERT INTO Librarians (Name, Age, Phone) VALUES
('Sarah Connor', 34, '555-0201'),
('John Doe', 28, '555-0202'),
('Michael Scott', 45, '555-0203'),
('Ellen Ripley', 39, '555-0204'),
('James Bond', 40, '555-0205');

-- Students table
CREATE TABLE IF NOT EXISTS Students (
    StudentId INT PRIMARY KEY AUTO_INCREMENT,
    Student_Name VARCHAR(100),
    Email VARCHAR(100),
    Phone_Number VARCHAR(20)
);

INSERT INTO Students (Student_Name, Email, Phone_Number) VALUES
('Alice Johnson', 'alice.j@email.com', '555-0101'),
('Bob Smith', 'bob.smith@email.com', '555-0102'),
('Charlie Brown', 'charlie.b@email.com', '555-0103'),
('Diana Prince', 'diana.p@email.com', '555-0104'),
('Evan Wright', 'evan.w@email.com', '555-0105');

-- Books table (For EF Core)
CREATE TABLE IF NOT EXISTS Books (
    BookId INT PRIMARY KEY AUTO_INCREMENT,
    Title LONGTEXT NOT NULL,
    Author LONGTEXT NOT NULL,
    ISBN LONGTEXT NOT NULL,
    PublishedDate DATETIME(6) NOT NULL,
    IsAvailable TINYINT(1) NOT NULL
);

INSERT INTO Books (Title, Author, ISBN, PublishedDate, IsAvailable) VALUES
('The Great Gatsby', 'F. Scott Fitzgerald', '9780743273565', '1925-04-10', 1),
('To Kill a Mockingbird', 'Harper Lee', '9780060935467', '1960-07-11', 1),
('1984', 'George Orwell', '9780451524935', '1949-06-08', 1);

-- BorrowRecords table (For EF Core)
CREATE TABLE IF NOT EXISTS BorrowRecords (
    BorrowRecordId INT PRIMARY KEY AUTO_INCREMENT,
    BookId INT NOT NULL,
    BorrowerName LONGTEXT NOT NULL,
    BorrowerEmail LONGTEXT NOT NULL,
    Phone LONGTEXT NOT NULL,
    BorrowDate DATETIME(6) NOT NULL,
    ReturnDate DATETIME(6) NULL,
    CONSTRAINT FK_BorrowRecords_Books_BookId FOREIGN KEY (BookId) REFERENCES Books(BookId) ON DELETE CASCADE
);
