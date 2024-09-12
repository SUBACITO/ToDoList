CREATE TABLE Tasks (
    TaskID INT PRIMARY KEY IDENTITY(1,1),
    TaskName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    IsCompleted BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    DueDate DATETIME,
    UserName VARCHAR(30) NOT NULL
);

CREATE TABLE tbl_Account (
    UserName VARCHAR(30) NOT NULL PRIMARY KEY,
    UserPass VARBINARY(100) NOT NULL,
    FullName NVARCHAR(50) NULL,
    UserRight TINYINT NULL,
    AllowInsert BIT NULL,
    AllowUpdate BIT NULL,
    AllowDelete BIT NULL,
    UserStatus BIT NULL,
    UpdateTime DATETIME NULL,
    LastLogin DATETIME NULL
);


create PROCEDURE AddTask
    @TaskName NVARCHAR(255),
    @Description NVARCHAR(MAX),
    @DueDate DATETIME,
    @UserName VARCHAR(30)
AS
BEGIN
    -- Check user permissions
    IF NOT EXISTS (SELECT * FROM tbl_Account WHERE UserName = @UserName AND AllowInsert = 1)
    BEGIN
        RAISERROR(N'Người dùng không có quyền thêm dữ liệu. Vui lòng kiểm tra lại', 16, 1);
        RETURN;
    END

    -- Insert the task
    INSERT INTO Tasks (TaskName, Description, DueDate, UserName)
    VALUES (@TaskName, @Description, @DueDate, @UserName);

	select	errMsg = N'Thêm công việc thành công'
END;
GO

create PROCEDURE RemoveTask
    @TaskId NVARCHAR(255),
    @UserName VARCHAR(30)
AS
BEGIN
    -- Check user permissions
    IF NOT EXISTS (SELECT * FROM tbl_Account WHERE UserName = @UserName AND AllowDelete = 1)
    BEGIN
        RAISERROR(N'Người dùng không có quyền xóa dữ liệu. Vui lòng kiểm tra lại', 16, 1);
        RETURN;
    END

	IF NOT EXISTS (SELECT * FROM Tasks WHERE TaskID = @TaskId)
	BEGIN
        RAISERROR(N'Công việc không tồn tại', 16, 1);
        RETURN;
    END
	
    -- Insert the task
    Delete Tasks 
	where TaskID = @TaskId

	select	errMsg = N'Xóa công việc thành công'
END;

CREATE PROCEDURE sp_TaskSelectAll
    @UserName VARCHAR(30)
AS
BEGIN
    -- Check user permissions
    IF NOT EXISTS (SELECT * FROM tbl_Account WHERE UserName = @UserName AND AllowInsert = 1)
    BEGIN
        RAISERROR(N'Người dùng không có quyền xem dữ liệu. Vui lòng kiểm tra lại', 16, 1);
        RETURN;
    END

    -- Select all tasks for the user
    SELECT * FROM Tasks WHERE UserName = @UserName;
END;
GO

CREATE PROCEDURE spAdmin_UserLogin
    @UserName VARCHAR(30),
    @PassWord VARCHAR(50)
AS
BEGIN
    BEGIN TRY
        -- Validate user credentials
        IF NOT EXISTS (
            SELECT * 
            FROM tbl_Account 
            WHERE UserName = @UserName 
              AND PWDCOMPARE(@PassWord, UserPass) = 1 
              AND UserStatus = 1
        )
        BEGIN
            RAISERROR(N'Tên đăng nhập hoặc mật khẩu không chính xác.', 16, 1);
            RETURN;
        END

        -- Update last login time
        UPDATE tbl_Account
        SET LastLogin = GETDATE()
        WHERE UserName = @UserName;

        -- Return user details
        SELECT UserName, FullName, AllowInsert, AllowUpdate, AllowDelete, 
               UserRight, UserStatus, LastLogin, UpdateTime
        FROM tbl_Account
        WHERE UserName = @UserName;
    END TRY
    BEGIN CATCH
        DECLARE @err NVARCHAR(1000) = ERROR_MESSAGE();
        RAISERROR(@err, 16, 1);
    END CATCH
END;
GO

insert into tbl_Account(UserName,UserPass,AllowInsert,AllowUpdate,AllowDelete,UserStatus)
values ('admin',PWDENCRYPT('admin'),1,1,1,1)