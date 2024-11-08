USE master
GO

IF NOT EXISTS (
    SELECT [name]
    FROM sys.databases
    WHERE [name] = N'SistemaMultas'
)
CREATE DATABASE SistemaMultas
GO

USE SistemaMultas
GO

----------
-- CRUD --
----------
----------------------------------------------------
------------------- USUARIOS -----------------------
----------------------------------------------------

------------------------------------
-- Validar Usuario (Autenticación)
--CREATE PROCEDURE sp_validarUsuario
--    @Email NVARCHAR(100),
--    @ContrasennaHash NVARCHAR(255)
--AS
--BEGIN
--    SELECT *
--    FROM Usuarios
--    WHERE email = @Email AND contrasennaHash = @ContrasennaHash AND estado = 1;
--END;
--GO

------------------------------------ ya no se usa
-- Obtener usuarios por rol
--CREATE PROCEDURE sp_GetUsuariosPorRol
--    @Rol_idRol UNIQUEIDENTIFIER
--AS
--BEGIN
--    SELECT u.* 
--    FROM Usuarios u
--    INNER JOIN UsuarioRoles ur ON u.IdUsuario = ur.UsuarioId
--    WHERE ur.RolId = @Rol_idRol;
--END;
--GO


----------------------------------------------------
------------------- ROLES --------------------------
----------------------------------------------------
------------------------------------ ya no se usa
-- Asignar Rol a usuario
--CREATE PROCEDURE sp_AsignarRol
--    @Usuario_idUsuario UNIQUEIDENTIFIER,
--    @Rol_idRol UNIQUEIDENTIFIER
--AS
--BEGIN
--    INSERT INTO UsuarioRoles (Usuarioid, Rolid)
--    VALUES (@Usuario_idUsuario, @Rol_idRol);
--END;
--GO

------------------------------------ ya no se usa
-- Obtener roles por usuario
--CREATE PROCEDURE sp_GetRolesPorUsuario
--    @Usuario_idUsuario UNIQUEIDENTIFIER
--AS
--BEGIN
--	SELECT r.* 
--    FROM Roles r
--    INNER JOIN UsuarioRoles ur ON r.IdRol = ur.RolId
--    WHERE ur.UsuarioId = @Usuario_idUsuario;
--END;
--GO

------------------------------------ ya no se usa
-- DELETE
--CREATE PROCEDURE sp_DeleteRolDeUsuario
--    @Usuario_idUsuario UNIQUEIDENTIFIER,
--    @Rol_idRol UNIQUEIDENTIFIER
--AS
--BEGIN
--    DELETE FROM UsuarioRoles
--    WHERE Usuarioid = @Usuario_idUsuario AND Rolid= @Rol_idRol;
--END;
--GO


----------------------------------------------------
------------------- PERMISOS -----------------------
----------------------------------------------------
------------------------------------ ya no se usa
-- Obtener permisos de x rol
--CREATE PROCEDURE sp_GetPermisosPorRol
--    @Rol_idRol UNIQUEIDENTIFIER
--AS
--BEGIN
--	SELECT p.* 
--    FROM Permisos p
--    INNER JOIN RolPermisos rp ON p.IdPermiso = rp.PermisoId
--    WHERE rp.RolId = @Rol_idRol;
--END;
--GO

------------------------------------ ya no se usa
-- Asignar Permiso a Rol
--CREATE PROCEDURE sp_AsignarPermiso
--    @Rol_idRol UNIQUEIDENTIFIER,
--    @Permiso_idPermiso UNIQUEIDENTIFIER
--AS
--BEGIN
--    BEGIN
--        INSERT INTO RolPermisos (RolId, PermisoId)
--        VALUES (@Rol_idRol, @Permiso_idPermiso);
--    END;
--END;
--GO

------------------------------------ ya no se usa
-- Eliminar permiso de rol
--CREATE PROCEDURE sp_DeletePermisoDeRol
--    @Rol_idRol UNIQUEIDENTIFIER,
--    @Permiso_idPermiso UNIQUEIDENTIFIER
--AS
--BEGIN
--    DELETE FROM RolPermisos
--    WHERE Rolid= @Rol_idRol AND Permisoid= @Permiso_idPermiso;
--END;
--GO

----------------------------------------------------
------------------- VEHICULO -----------------------
----------------------------------------------------

------------------------------------ ya no se usa
-- Obtener Vehículos por Usuario
--CREATE PROCEDURE sp_obtenerVehiculosPorUsuario
--    @Usuario_idUsuario UNIQUEIDENTIFIER
--AS
--BEGIN
--    SELECT * FROM Vehiculos
--    WHERE Usuarioid = @Usuario_idUsuario;
--END;
--GO

------------------------------------ ya no se usa
-- Actualizar Foto de Vehículo
--CREATE PROCEDURE sp_actualizarFotoVehiculo
--    @idVehiculo UNIQUEIDENTIFIER,
--    @foto_vehiculo NVARCHAR(255)
--AS
--BEGIN
--    UPDATE Vehiculos
--    SET FotoVehiculo = @foto_vehiculo
--    WHERE IdVehiculo = @idVehiculo;
--END;
--GO

----------------------------------------------------
------------------- INFRACCION ---------------------
----------------------------------------------------

------------------------------------
-- Asignar las infracciones a Multa
--CREATE PROCEDURE sp_AsignarInfraccion
--    @Multa_idMulta UNIQUEIDENTIFIER,
--    @Infraccion_idInfraccion UNIQUEIDENTIFIER
--AS
--BEGIN
--    IF NOT EXISTS (
--        SELECT 1 FROM MultaInfracciones 
--        WHERE MultaId = @Multa_idMulta AND InfraccionId = @Infraccion_idInfraccion
--    )
--    BEGIN
--        INSERT INTO MultaInfracciones (MultaId, InfraccionId)
--        VALUES (@Multa_idMulta, @Infraccion_idInfraccion);
--    END
--END;
--GO

-- Asignar Infracción a Multa
--CREATE PROCEDURE sp_AsignarInfraccion
--    @Multa_idMulta UNIQUEIDENTIFIER,
--    @Infraccion_idInfraccion UNIQUEIDENTIFIER
--AS
--BEGIN
--    INSERT INTO MultaInfracciones (MultaId, InfraccionId)
--    VALUES (@Multa_idMulta, @Infraccion_idInfraccion);
--END;
--GO


----------------------------------------------------
------------------- MULTA --------------------------
----------------------------------------------------
------------------------------------- ya no se usa
-- Obtener multas por número de placa
--CREATE PROCEDURE sp_GetMultasPorPlaca
--    @NumeroPlaca NVARCHAR(10)
--AS
--BEGIN
--    SELECT 
--        m.IdMulta,
--        m.VehiculoId, -- Devuelve el IdVehiculo en lugar de NumeroPlaca
--        m.UsuarioIdOficial, -- Devuelve el IdUsuarioOficial en lugar de Cedula
--        m.FechaHora,
--        m.Latitud,
--        m.Longitud,
--        m.Comentario,
--        m.FotoPlaca,
--        m.Estado
--    FROM Multas AS m
--    INNER JOIN Vehiculos AS v ON m.VehiculoId = v.IdVehiculo
--    WHERE v.NumeroPlaca = @NumeroPlaca;
--END;
--GO



------------------------------------
-- Obtener multas por infraccion
--CREATE PROCEDURE sp_GetMultasPorInfraccion
--    @idInfraccion UNIQUEIDENTIFIER
--AS
--BEGIN
--    SET NOCOUNT ON;

--    SELECT m.IdMulta, m.VehiculoId, m.UsuarioIdOficial, m.FechaHora, m.Latitud, m.Longitud, 
--           m.Comentario, m.FotoPlaca, m.Estado
--    FROM Multas m
--    INNER JOIN MultaInfracciones mxi ON m.IdMulta = mxi.MultaId
--    WHERE mxi.InfraccionId = @idInfraccion;
--END;
--GO

------------------------------------------
-- Obtener multas por título de infracción
--CREATE PROCEDURE sp_GetMultasPorTituloInfraccion
--    @TituloInfraccion NVARCHAR(100)
--AS
--BEGIN
--    SELECT 
--        m.IdMulta,
--        m.VehiculoId,
--        m.UsuarioIdOficial,
--        m.FechaHora,
--        m.Latitud,
--        m.Longitud,
--        m.Comentario,
--        m.FotoPlaca,
--        m.Estado
--    FROM Multas AS m
--    INNER JOIN MultaInfracciones AS mi ON m.IdMulta = mi.MultaId
--    INNER JOIN Infracciones AS i ON mi.InfraccionId = i.IdInfraccion
--    WHERE i.Titulo = @TituloInfraccion;
--END;
--GO

-- Obtener multas por Cédula o ID de Usuario
--CREATE PROCEDURE sp_GetMultasPorUsuario
--    @Cedula NVARCHAR(50) = NULL,
--    @UsuarioId UNIQUEIDENTIFIER = NULL
--AS
--BEGIN
--    SELECT 
--        m.IdMulta,
--        m.VehiculoId,
--        m.UsuarioIdOficial,
--        m.FechaHora,
--        m.Latitud,
--        m.Longitud,
--        m.Comentario,
--        m.FotoPlaca,
--        m.Estado
--    FROM Multas AS m
--    INNER JOIN Usuarios AS u ON m.UsuarioIdOficial = u.IdUsuario
--    WHERE (@Cedula IS NOT NULL AND u.Cedula = @Cedula)
--       OR (@UsuarioId IS NOT NULL AND u.IdUsuario = @UsuarioId);
--END;
--GO


----------------------------------------------------
------------------- DISPUTA ------------------------
----------------------------------------------------

------------------------------------ 
-- Obtener Disputas Activas por Usuario
--CREATE PROCEDURE sp_obtenerDisputasActivasPorUsuario
--    @Usuario_idUsuario UNIQUEIDENTIFIER
--AS
--BEGIN
--    SELECT *
--    FROM Disputas
--    WHERE Usuarioid = @Usuario_idUsuario AND estado = 1; -- 1 = En disputa
--END;
--GO
