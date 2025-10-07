create database Hospital;
-- 1. Users Table
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50) NOT NULL -- Admin / Doctor / Patient
);

-- 2. Departments Table
CREATE TABLE Departments (
    DepartmentId INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL
);

-- 3. Doctors Table
CREATE TABLE Doctors (
    DoctorId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Specialization NVARCHAR(100) NOT NULL,
    DepartmentId INT NULL,  -- nullable to allow deletion of department
    CONSTRAINT FK_Doctors_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Doctors_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(DepartmentId) ON DELETE SET NULL
);

-- Indexes
CREATE INDEX IX_Doctors_UserId ON Doctors(UserId);
CREATE INDEX IX_Doctors_DepartmentId ON Doctors(DepartmentId);

-- 4. Patients Table
CREATE TABLE Patients (
    PatientId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Age INT NOT NULL,
    Gender NVARCHAR(10) NOT NULL,
    Contact NVARCHAR(50),
    Address NVARCHAR(255),
    CONSTRAINT FK_Patients_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE INDEX IX_Patients_UserId ON Patients(UserId);

-- 5. DoctorPatients (Many-to-Many)
CREATE TABLE DoctorPatients (
    DoctorId INT NOT NULL,
    PatientId INT NOT NULL,
    PRIMARY KEY (DoctorId, PatientId),
    CONSTRAINT FK_DoctorPatients_Doctor FOREIGN KEY (DoctorId) REFERENCES Doctors(DoctorId) ON DELETE NO ACTION,
    CONSTRAINT FK_DoctorPatients_Patient FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE NO ACTION
);

CREATE INDEX IX_DoctorPatients_DoctorId ON DoctorPatients(DoctorId);
CREATE INDEX IX_DoctorPatients_PatientId ON DoctorPatients(PatientId);

-- 6. Appointments Table
CREATE TABLE Appointments (
    AppointmentId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    AppointmentDate DATETIME NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    CONSTRAINT FK_Appointments_Patient FOREIGN KEY (PatientId)
        REFERENCES Patients(PatientId) ON DELETE CASCADE,
    CONSTRAINT FK_Appointments_Doctor FOREIGN KEY (DoctorId)
        REFERENCES Doctors(DoctorId) ON DELETE NO ACTION
);

CREATE INDEX IX_Appointments_PatientId ON Appointments(PatientId);
CREATE INDEX IX_Appointments_DoctorId ON Appointments(DoctorId);

-- 7. MedicalRecords Table
CREATE TABLE MedicalRecords (
    RecordId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    DoctorId INT NOT NULL,
    Diagnosis NVARCHAR(255),
    Treatment NVARCHAR(255),
    RecordDate DATETIME NOT NULL,
    CONSTRAINT FK_MedicalRecords_Patient FOREIGN KEY (PatientId)
        REFERENCES Patients(PatientId) ON DELETE CASCADE,
    CONSTRAINT FK_MedicalRecords_Doctor FOREIGN KEY (DoctorId)
        REFERENCES Doctors(DoctorId) ON DELETE NO ACTION
);

-- 8. Billing Table
CREATE TABLE Billing (
    BillId INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentStatus NVARCHAR(50) NOT NULL,
    BillDate DATETIME NOT NULL,
    CONSTRAINT FK_Billing_Patient FOREIGN KEY (PatientId) REFERENCES Patients(PatientId) ON DELETE CASCADE
);

CREATE INDEX IX_Billing_PatientId ON Billing(PatientId);

-- 9. RefreshTokens Table
CREATE TABLE RefreshTokens (
    TokenId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    Token NVARCHAR(255) NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    Revoked BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);

CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);



select * from Users;


Go


-- 1. Register a Patient
CREATE PROCEDURE sp_RegisterPatient
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @Name NVARCHAR(100),
    @Age INT,
    @Gender NVARCHAR(10),
    @Contact NVARCHAR(50) = NULL,
    @Address NVARCHAR(255) = NULL,
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Users (Email, PasswordHash, Role)
        VALUES (@Email, @PasswordHash, 'Patient');

        SET @UserId = SCOPE_IDENTITY();

        INSERT INTO Patients (UserId, Name, Age, Gender, Contact, Address)
        VALUES (@UserId, @Name, @Age, @Gender, @Contact, @Address);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


-- 2. Register a Doctor
CREATE PROCEDURE sp_RegisterDoctor
    @Email NVARCHAR(100),
    @PasswordHash NVARCHAR(255),
    @Name NVARCHAR(100),
    @DepartmentId INT,
    @Specialisation NVARCHAR(100) = NULL,
    @UserId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Users (Email, PasswordHash, Role)
        VALUES (@Email, @PasswordHash, 'Doctor');

        SET @UserId = SCOPE_IDENTITY();

        INSERT INTO Doctors(UserId, Name, DepartmentId, Specialization)
        VALUES (@UserId, @Name, @DepartmentId, @Specialisation);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


-- 3. Add Medical Record
CREATE PROCEDURE sp_AddMedicalRecord
    @PatientId INT,
    @DoctorId INT,
    @Diagnosis NVARCHAR(255),
    @Treatment NVARCHAR(255),
    @RecordDate DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO MedicalRecords (PatientId, DoctorId, Diagnosis, Treatment, RecordDate)
        VALUES (@PatientId, @DoctorId, @Diagnosis, @Treatment, @RecordDate);
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO


-- 4. Get Patient Medical History
CREATE PROCEDURE sp_GetPatientHistory
    @PatientId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT mr.RecordId,
           mr.RecordDate,
           mr.Diagnosis,
           mr.Treatment,
           d.Name AS DoctorName,
           d.Specialization
    FROM MedicalRecords mr
    JOIN Doctors d ON mr.DoctorId = d.DoctorId
    WHERE mr.PatientId = @PatientId
    ORDER BY mr.RecordDate DESC;
END
GO


-- 5. Book Appointment
CREATE PROCEDURE sp_BookAppointment
    @PatientId INT,
    @DoctorId INT,
    @AppointmentDate DATETIME,
    @AppointmentId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Appointments (PatientId, DoctorId, AppointmentDate, Status)
        VALUES (@PatientId, @DoctorId, @AppointmentDate, 'Pending');

        SET @AppointmentId = SCOPE_IDENTITY();
        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


-- 6. Approve Appointment
CREATE PROCEDURE sp_ApproveAppointment
    @AppointmentId INT
AS
BEGIN
    UPDATE Appointments
    SET Status = 'Approved'
    WHERE AppointmentId = @AppointmentId;
END
GO


-- 7. Reject Appointment
CREATE PROCEDURE sp_RejectAppointment
    @AppointmentId INT
AS
BEGIN
    UPDATE Appointments
    SET Status = 'Rejected'
    WHERE AppointmentId = @AppointmentId;
END
GO




select * from users;