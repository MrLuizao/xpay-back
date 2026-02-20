/*
=============================================================================
DATABASE SETUP SCRIPT - XPAY
=============================================================================
Este script consolida la creación de tablas, procedimientos almacenados
y datos iniciales (Roles) necesarios para el funcionamiento del API de Usuarios.
=============================================================================
*/

USE ANTAD_SEC;
GO

-- 1. CREACIÓN DE TABLAS
-----------------------------------------------------------------------------

-- Tabla de Roles
IF OBJECT_ID('dbo.RolXPay', 'U') IS NULL BEGIN
CREATE TABLE RolXPay (
    RolXPayId INT IDENTITY(1, 1) PRIMARY KEY,
    Nombre NVARCHAR(50) NOT NULL,
    bActivo BIT NOT NULL DEFAULT 1,
    FechaAlta DATETIME2,
    FechaEdita DATETIME2,
    FechaElimina DATETIME2
);

END

-- Tabla de Usuarios
IF OBJECT_ID('dbo.UsuarioXPay', 'U') IS NULL BEGIN
CREATE TABLE UsuarioXPay (
    UsuarioXPayId INT IDENTITY(1, 1) PRIMARY KEY,
    UserId NVARCHAR(50) NOT NULL UNIQUE,
    Nombre NVARCHAR(150) NOT NULL,
    Apellido NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Celular NVARCHAR(20) NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    RolXPayId INT NOT NULL,
    bActivo BIT NOT NULL DEFAULT 1,
    FechaAlta DATETIME2,
    FechaEdita DATETIME2,
    FechaElimina DATETIME2,
    CONSTRAINT FK_UsuarioXPay_RolXPay FOREIGN KEY (RolXPayId) REFERENCES RolXPay (RolXPayId)
);

END

-- 2. DATOS INICIALES (ROLES)
-----------------------------------------------------------------------------
IF NOT EXISTS (
    SELECT 1
    FROM RolXPay
    WHERE
        Nombre = 'User'
)
INSERT INTO
    RolXPay (Nombre, FechaAlta)
VALUES ('User', SYSDATETIME());

IF NOT EXISTS (
    SELECT 1
    FROM RolXPay
    WHERE
        Nombre = 'Guest'
)
INSERT INTO
    RolXPay (Nombre, FechaAlta)
VALUES ('Guest', SYSDATETIME());
GO

-- 3. PROCEDIMIENTOS ALMACENADOS
-----------------------------------------------------------------------------

-- 3.1 Procedimiento de Inserción
IF OBJECT_ID('dbo.UsuarioXPay_Insert', 'P') IS NOT NULL DROP
PROCEDURE dbo.UsuarioXPay_Insert;
GO
CREATE PROCEDURE dbo.UsuarioXPay_Insert
(
    @UserId NVARCHAR(50),
    @Nombre NVARCHAR(150),
    @Apellido NVARCHAR(150),
    @Email NVARCHAR(150),
    @Celular NVARCHAR(20),
    @PasswordHash NVARCHAR(255),
    @RolXPayId INT
)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO UsuarioXPay (UserId, Nombre, Apellido, Email, Celular, PasswordHash, RolXPayId, bActivo, FechaAlta)
    VALUES (@UserId, @Nombre, @Apellido, @Email, @Celular, @PasswordHash, @RolXPayId, 1, SYSDATETIME());
    SELECT 'OK' AS RESPCODE, 'Registro exitoso' AS DESCCODE;
END
GO

-- 3.2 Procedimiento de Edición (con validación de Rol 2)
IF OBJECT_ID('dbo.UsuarioXPay_Edit', 'P') IS NOT NULL DROP
PROCEDURE dbo.UsuarioXPay_Edit;
GO
CREATE PROCEDURE dbo.UsuarioXPay_Edit
(
    @UserId NVARCHAR(50),
    @Nombre NVARCHAR(150),
    @Apellido NVARCHAR(150),
    @Email NVARCHAR(150),
    @Celular NVARCHAR(20),
    @PasswordHash NVARCHAR(255) = NULL,
    @RolXPayId INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id INT;
    DECLARE @CurrentRolId INT;
    
    SELECT @Id = UsuarioXPayId, @CurrentRolId = RolXPayId FROM UsuarioXPay WHERE UserId = @UserId AND bActivo = 1;

    IF @Id IS NULL
    BEGIN
        SELECT '01' AS RESPCODE, 'Usuario Inexistente' AS DESCCODE;
        RETURN;
    END

    -- Validación: Si ya es Rol 2, bloqueamos el update
    IF @CurrentRolId = 2
    BEGIN
        SELECT '02' AS RESPCODE, 'El usuario ya se encuentra registrado' AS DESCCODE;
        RETURN;
    END

    UPDATE UsuarioXPay
    SET
        Nombre = @Nombre,
        Apellido = @Apellido,
        Email = @Email,
        Celular = @Celular,
        PasswordHash = ISNULL(@PasswordHash, PasswordHash),
        RolXPayId = ISNULL(@RolXPayId, RolXPayId),
        FechaEdita = SYSDATETIME()
    WHERE UsuarioXPayId = @Id;

    SELECT '00' AS RESPCODE, 'Actualización exitosa' AS DESCCODE;
END
GO

-- 3.3 Procedimiento de Login (específico para Rol 2)
IF OBJECT_ID('dbo.UsuarioXPay_Login', 'P') IS NOT NULL DROP
PROCEDURE dbo.UsuarioXPay_Login;
GO
CREATE PROCEDURE dbo.UsuarioXPay_Login
(
    @UserId NVARCHAR(150), -- Aumentado para soportar Email
    @PasswordHash NVARCHAR(255)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Id INT;
    DECLARE @RolId INT;

    -- Buscamos por UserId O por Email
    SELECT @Id = UsuarioXPayId, @RolId = RolXPayId 
    FROM UsuarioXPay 
    WHERE (UserId = @UserId OR Email = @UserId) 
      AND PasswordHash = @PasswordHash 
      AND bActivo = 1;

    IF @Id IS NULL
    BEGIN
        SELECT '01' AS RESPCODE, 'Usuario o contraseña incorrectos' AS DESCCODE;
        RETURN;
    END

    IF @RolId <> 2
    BEGIN
        SELECT '02' AS RESPCODE, 'Acceso denegado: Rol no permitido' AS DESCCODE, UserId, RolXPayId
        FROM UsuarioXPay WHERE UsuarioXPayId = @Id;
        RETURN;
    END

    SELECT '00' AS RESPCODE, 'Login exitoso' AS DESCCODE, UserId, Nombre, Apellido, Email, Celular, RolXPayId
    FROM UsuarioXPay WHERE UsuarioXPayId = @Id;
END

-- 3.4 Procedimiento de Eliminación (Lógica)
IF OBJECT_ID('dbo.UsuarioXPay_Delete', 'P') IS NOT NULL DROP
PROCEDURE dbo.UsuarioXPay_Delete;

CREATE PROCEDURE dbo.UsuarioXPay_Delete 
(
	@UserId NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UsuarioXPayId INT;
    SET @UsuarioXPayId = (SELECT UsuarioXPayId FROM UsuarioXPay WHERE UserId = @UserId AND bActivo = 1);

    IF @UsuarioXPayId IS NULL 
    BEGIN
        SELECT '01' AS RESPCODE, 'Usuario Inexistente' AS DESCCODE;
        RETURN;
    END

    UPDATE UsuarioXPay
    SET bActivo = 0, FechaElimina = SYSDATETIME()
    WHERE UsuarioXPayId = @UsuarioXPayId;

    SELECT '00' AS RESPCODE, 'OK' AS DESCCODE;
END
GO