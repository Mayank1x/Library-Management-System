USE LMS;

CREATE TABLE IF NOT EXISTS Publications (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    Title VARCHAR(100) NOT NULL,
    Publisher VARCHAR(50) NOT NULL,
    PublishedDate DATETIME(6) NOT NULL,
    `Type` INT NOT NULL,
    IsAvailable BOOLEAN NOT NULL DEFAULT 1
);

INSERT IGNORE INTO Books (BookId, Title, Author, ISBN, PublishedDate, IsAvailable) VALUES
(5, 'Clean Architecture', 'Robert C. Martin', '978-0134494166', '2017-09-20', 1),
(6, 'Refactoring', 'Martin Fowler', '978-0134757599', '2018-11-30', 1),
(7, 'The Clean Coder', 'Robert C. Martin', '978-0137081073', '2011-05-13', 1),
(8, 'Domain-Driven Design', 'Eric Evans', '978-0321125217', '2003-08-30', 1),
(9, 'You Don''t Know JS', 'Kyle Simpson', '978-1491904244', '2015-03-01', 1),
(10, 'Introduction to Algorithms', 'Thomas H. Cormen', '978-0262033848', '2009-07-31', 1);


-- Newspapers (Type = 0)
INSERT INTO Publications (Title, Publisher, PublishedDate, `Type`, IsAvailable) VALUES
('The Daily Times', 'Global Media Group', '2026-07-22', 0, 1),
('Financial Chronicle', 'WallSt Press', '2026-07-21', 0, 1),
('Tech Weekly News', 'Silicon Valley Pubs', '2026-07-20', 0, 1),
('Metro Morning Post', 'City Press House', '2026-07-22', 0, 1),
('Saturday Sports Herald', 'Global Media Group', '2026-07-18', 0, 0);

-- Magazines (Type = 1)
INSERT INTO Publications (Title, Publisher, PublishedDate, `Type`, IsAvailable) VALUES
('National Geographic Vol 45', 'NatGeo Society', '2026-07-01', 1, 1),
('Vogue Fashion Summer', 'Condé Nast', '2026-06-15', 1, 1),
('Forbes Business 30 Under 30', 'Forbes Media', '2026-07-10', 1, 0),
('PC Gamer Ultimate', 'Future US', '2026-07-05', 1, 1),
('Scientific American', 'Springer Nature', '2026-06-28', 1, 1);

INSERT INTO Students (Student_Name, Email, Phone_Number) VALUES
('Frank Miller', 'frank.m@email.com', '555-0106'),
('Grace Lee', 'grace.lee@email.com', '555-0107'),
('Henry Adams', 'henry.a@email.com', '555-0108');

INSERT INTO Librarians (Name, Age, Phone) VALUES
('Tony Stark', 45, '555-0206'),
('Diana Ross', 38, '555-0207'),
('Peter Parker', 26, '555-0208');
