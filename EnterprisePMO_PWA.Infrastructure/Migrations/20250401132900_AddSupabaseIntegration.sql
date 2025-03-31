-- Migration Script to add Supabase integration to existing users table

-- Add SupabaseId column to Users table if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.Users') 
    AND name = 'SupabaseId'
)
BEGIN
    ALTER TABLE dbo.Users
    ADD SupabaseId NVARCHAR(255) NULL;
    
    PRINT 'Added SupabaseId column to Users table'
END
ELSE
BEGIN
    PRINT 'SupabaseId column already exists in Users table'
END

-- Create index on SupabaseId for faster lookups
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Users_SupabaseId'
    AND object_id = OBJECT_ID('dbo.Users')
)
BEGIN
    CREATE INDEX IX_Users_SupabaseId ON dbo.Users(SupabaseId);
    PRINT 'Created index on SupabaseId column'
END
ELSE
BEGIN
    PRINT 'Index on SupabaseId already exists'
END

-- Create Holding department if it doesn't exist
IF NOT EXISTS (
    SELECT 1 
    FROM dbo.Departments 
    WHERE Name = 'Holding'
)
BEGIN
    -- Create a new GUID for the department ID
    DECLARE @DepartmentId UNIQUEIDENTIFIER = NEWID();
    
    INSERT INTO dbo.Departments (Id, Name, Description, CreatedDate, IsActive)
    VALUES (@DepartmentId, 'Holding', 'Default department for new users', GETUTCDATE(), 1);
    
    PRINT 'Created Holding department'
END
ELSE
BEGIN
    PRINT 'Holding department already exists'
END

-- Create a stored procedure to sync a user from Supabase
IF OBJECT_ID('dbo.SyncUserWithSupabase', 'P') IS NOT NULL
    DROP PROCEDURE dbo.SyncUserWithSupabase;
GO

CREATE PROCEDURE dbo.SyncUserWithSupabase
    @Email NVARCHAR(255),
    @SupabaseId NVARCHAR(255),
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId UNIQUEIDENTIFIER;
    DECLARE @DepartmentId UNIQUEIDENTIFIER;
    
    -- Find user by email
    SELECT @UserId = Id
    FROM dbo.Users
    WHERE Username = @Email OR Email = @Email;
    
    -- If user exists, update SupabaseId
    IF @UserId IS NOT NULL
    BEGIN
        UPDATE dbo.Users
        SET SupabaseId = @SupabaseId,
            FirstName = ISNULL(@FirstName, FirstName),
            LastName = ISNULL(@LastName, LastName),
            LastUpdated = GETUTCDATE()
        WHERE Id = @UserId;
        
        SELECT 'User updated' AS Result, @UserId AS UserId;
        RETURN;
    END
    
    -- Get Holding department ID
    SELECT @DepartmentId = Id
    FROM dbo.Departments
    WHERE Name = 'Holding';
    
    -- If no Holding department, create one
    IF @DepartmentId IS NULL
    BEGIN
        SET @DepartmentId = NEWID();
        
        INSERT INTO dbo.Departments (Id, Name, Description, CreatedDate, IsActive)
        VALUES (@DepartmentId, 'Holding', 'Default department for new users', GETUTCDATE(), 1);
    END
    
    -- Create new user
    SET @UserId = NEWID();
    
    INSERT INTO dbo.Users (
        Id, 
        Username, 
        Email,
        FirstName,
        LastName,
        SupabaseId, 
        Role, 
        DepartmentId, 
        IsActive, 
        CreatedDate
    )
    VALUES (
        @UserId,
        @Email,
        @Email,
        @FirstName,
        @LastName,
        @SupabaseId,
        'ProjectManager', -- Default role as int (0 = ProjectManager)
        @DepartmentId,
        1, -- IsActive
        GETUTCDATE()
    );
    
    -- Log audit record of user creation
    INSERT INTO dbo.AuditLogs (
        Id,
        EntityName,
        EntityId,
        Action,
        UserId,
        Username,
        Timestamp,
        ChangeSummary,
        IpAddress
    )
    VALUES (
        NEWID(),
        'User',
        CAST(@UserId AS NVARCHAR(100)),
        'Created',
        @UserId, -- Creator is self for auto-creation
        @Email,
        GETUTCDATE(),
        'User automatically created during Supabase authentication sync',
        'System'
    );
    
    SELECT 'User created' AS Result, @UserId AS UserId;
END
GO

PRINT 'Created stored procedure: SyncUserWithSupabase'
PRINT 'Migration completed successfully'