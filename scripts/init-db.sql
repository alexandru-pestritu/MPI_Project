IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'MPI_Database')
    BEGIN
        CREATE DATABASE MPI_Database;
        PRINT 'Database MPI_Database created.';
    END
ELSE
    BEGIN
        PRINT 'Database MPI_Database already exists.';
    END
GO

USE MPI_Database;
GO

if not exists (select * from sys.tables where name = 'Users')
begin
create table Users (
    [Id] int identity (0,1) primary key,
    [Username] nvarchar(255) NOT NULL,
    [Password] nvarchar(255) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [Role] smallint not null,
    [IsVerified] bit not null default 0
)
end 
GO

if not exists (select * from sys.tables where name = 'UserProfile')
    begin
        create table UserProfile (
            [Id] int identity (0,1) primary key,
            [UserId] int not null,
            [FirstName] nvarchar(255) not null,
            [LastName] nvarchar(255) not null,
            [Bio] nvarchar(255) not null,
            [ProfilePicture] nvarchar(255) not null,
            foreign key (UserId) references Users(Id)
        )
    end
GO

if not exists (select * from sys.tables where name = 'Courses')
    begin
        create table Courses (
            [Id] int identity (0,1) primary key,
            [TeacherId] int not null,
            [Name] nvarchar(255) not null,
            [Description] nvarchar(255) not null,
            foreign key (TeacherId) references Users(Id)
        )
    end
GO

if not exists (select * from sys.tables where name = 'CourseStudentLink')
    begin
        create table CourseStudentLink (
            [Id] int identity (0,1) primary key,
            [CourseId] int not null,
            [StudentId] int not null,
            foreign key (CourseId) references Courses(Id),
            foreign key (StudentId) references Users(Id)
        )
    end
GO

if not exists (select * from sys.tables where name = 'Grades')
    begin
        create table Grades (
            [Id] int identity (0,1) primary key,
            [CourseId] int not null,
            [StudentId] int not null,
            [Value] int not null,
            [Date] datetime not null,
            foreign key (CourseId) references Courses(Id),
            foreign key (StudentId) references Users(Id)
        )
    end
GO

if not exists (select * from sys.tables where name = 'GradeHistory')
    begin
        create table GradeHistory
        (
            [Id]      int identity (0,1) primary key,
            [GradeId] int      not null,
            [Value]   int      not null,
            [Date]    datetime not null,
            foreign key (GradeId) references Grades (Id)
        )
    end
GO
