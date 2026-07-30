CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;

ALTER DATABASE CHARACTER SET utf8mb4;

CREATE TABLE `Books` (
    `BookId` int NOT NULL AUTO_INCREMENT,
    `Title` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Author` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `ISBN` longtext CHARACTER SET utf8mb4 NOT NULL,
    `PublishedDate` datetime(6) NOT NULL,
    `IsAvailable` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Books` PRIMARY KEY (`BookId`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `Publications` (
    `Id` int NOT NULL AUTO_INCREMENT,
    `Title` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
    `Publisher` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
    `PublishedDate` datetime(6) NOT NULL,
    `Type` int NOT NULL,
    `IsAvailable` tinyint(1) NOT NULL,
    CONSTRAINT `PK_Publications` PRIMARY KEY (`Id`)
) CHARACTER SET=utf8mb4;

CREATE TABLE `BorrowRecords` (
    `BorrowRecordId` int NOT NULL AUTO_INCREMENT,
    `BookId` int NOT NULL,
    `BorrowerName` longtext CHARACTER SET utf8mb4 NOT NULL,
    `BorrowerEmail` longtext CHARACTER SET utf8mb4 NOT NULL,
    `Phone` longtext CHARACTER SET utf8mb4 NOT NULL,
    `BorrowDate` datetime(6) NOT NULL,
    `ReturnDate` datetime(6) NULL,
    CONSTRAINT `PK_BorrowRecords` PRIMARY KEY (`BorrowRecordId`),
    CONSTRAINT `FK_BorrowRecords_Books_BookId` FOREIGN KEY (`BookId`) REFERENCES `Books` (`BookId`) ON DELETE CASCADE
) CHARACTER SET=utf8mb4;

INSERT INTO `Books` (`BookId`, `Author`, `ISBN`, `IsAvailable`, `PublishedDate`, `Title`)
VALUES (1, 'Andrew Hunt and David Thomas', '978-0201616224', TRUE, TIMESTAMP '2021-10-30 00:00:00', 'The Pragmatic Programmer'),
(2, 'Robert C. Martin', '978-0132350884', TRUE, TIMESTAMP '2023-08-01 00:00:00', 'Design Pattern using C#'),
(3, 'Pranaya Kumar Rout', '978-0451616235', TRUE, TIMESTAMP '2022-11-22 00:00:00', 'Mastering ASP.NET Core'),
(4, 'Rakesh Kumar', '978-4562350123', TRUE, TIMESTAMP '2020-08-15 00:00:00', 'SQL Server with DBA'),
(5, 'Robert C. Martin', '978-0134494166', TRUE, TIMESTAMP '2017-09-20 00:00:00', 'Clean Architecture'),
(6, 'Martin Fowler', '978-0134757599', TRUE, TIMESTAMP '2018-11-30 00:00:00', 'Refactoring'),
(7, 'Robert C. Martin', '978-0137081073', TRUE, TIMESTAMP '2011-05-13 00:00:00', 'The Clean Coder'),
(8, 'Eric Evans', '978-0321125217', TRUE, TIMESTAMP '2003-08-30 00:00:00', 'Domain-Driven Design'),
(9, 'Kyle Simpson', '978-1491904244', TRUE, TIMESTAMP '2015-03-01 00:00:00', 'You Don''t Know JS'),
(10, 'Thomas H. Cormen', '978-0262033848', TRUE, TIMESTAMP '2009-07-31 00:00:00', 'Introduction to Algorithms');

CREATE INDEX `IX_BorrowRecords_BookId` ON `BorrowRecords` (`BookId`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260727105014_ExpandCatalogAndAddPublications', '8.0.2');

COMMIT;

