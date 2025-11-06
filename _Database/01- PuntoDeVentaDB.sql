/*
================================================================================
SCRIPT DE CREACIÓN DE BASE DE DATOS PARA PROYECTO PUNTO DE VENTA (WPF, .NET)
Basado en el ERD desarrollado anteriormente.
================================================================================
*/

-- Usar la base de datos 'master' para crear la nueva BD
USE [master];
GO

-- 1. CREACIÓN DE LA BASE DE DATOS
-- -----------------------------------------------------------------------------
-- Creamos la base de datos si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'PuntoDeVentaDB')
BEGIN
    CREATE DATABASE [PuntoDeVentaDB];
    PRINT 'Base de datos [PuntoDeVentaDB] creada.';
END
GO

-- Usar la base de datos recién creada
USE [PuntoDeVentaDB];
GO

/*
================================================================================
II. CREACIÓN DE TABLAS
================================================================================
*/

-- -----------------------------------------------------------------------------
-- Tablas de Catálogos (Las que no dependen de otras)
-- -----------------------------------------------------------------------------

-- Tabla: ROL
PRINT 'Creando tabla ROL...';
CREATE TABLE ROL (
    ID_ROL INT PRIMARY KEY IDENTITY(1,1), -- IDENTITY(1,1) hace que sea autoincremental
    NOMBRE_ROL NVARCHAR(50) NOT NULL UNIQUE -- (Administrador, Cajero)
);
GO

-- Tabla: METODO_PAGO
PRINT 'Creando tabla METODO_PAGO...';
CREATE TABLE METODO_PAGO (
    ID_METODO INT PRIMARY KEY IDENTITY(1,1),
    NOMBRE_METODO NVARCHAR(50) NOT NULL UNIQUE -- (Efectivo, Tarjeta)
);
GO

-- Tabla: ESTADO_VENTA
PRINT 'Creando tabla ESTADO_VENTA...';
CREATE TABLE ESTADO_VENTA (
    ID_ESTADO_VENTA INT PRIMARY KEY IDENTITY(1,1),
    NOMBRE_ESTADO_VENTA NVARCHAR(50) NOT NULL UNIQUE -- (Finalizada, Cancelada)
);
GO

-- Tabla: PRODUCTO
PRINT 'Creando tabla PRODUCTO...';
CREATE TABLE PRODUCTO (
    ID_PRODUCTO INT PRIMARY KEY IDENTITY(1,1),
    NOMBRE_PRODUCTO NVARCHAR(150) NOT NULL,
    PRECIO DECIMAL(10, 2) NOT NULL, -- Ej. 99999999.99
    CANTIDAD_STOCK INT NOT NULL DEFAULT 0,
    DESCRIPCION NVARCHAR(500) NULL -- Descripción puede ser opcional (NULL)
);
GO

-- -----------------------------------------------------------------------------
-- Tablas Dependientes (Las que tienen Claves Foráneas)
-- -----------------------------------------------------------------------------

-- Tabla: USUARIO (Depende de ROL)
PRINT 'Creando tabla USUARIO...';
CREATE TABLE USUARIO (
    ID_USUARIO INT PRIMARY KEY IDENTITY(1,1),
    APATERNO NVARCHAR(50) NOT NULL,
    AMATERNO NVARCHAR(50) NULL, -- Apellido materno puede ser opcional
    NOMBRE NVARCHAR(50) NOT NULL,
    NOMBRE_USUARIO NVARCHAR(50) NOT NULL UNIQUE,
    PASSWORD_HASH NVARCHAR(50) NOT NULL,
    FECHA_CREACION DATE NOT NULL DEFAULT GETDATE(), -- Se auto-asigna la fecha actual
    ID_ROL INT NOT NULL,
    
    -- Definición de la Clave Foránea
    CONSTRAINT FK_USUARIO_ROL FOREIGN KEY (ID_ROL)
    REFERENCES ROL(ID_ROL)
);
GO

-- Tabla: VENTA (Depende de USUARIO, METODO_PAGO, ESTADO_VENTA)
PRINT 'Creando tabla VENTA...';
CREATE TABLE VENTA (
    ID_VENTA INT PRIMARY KEY IDENTITY(1,1),
    FECHA_VENTA DATETIME NOT NULL DEFAULT GETDATE(), -- Fecha y hora de la venta
    MONTO_TOTAL DECIMAL(10, 2) NOT NULL,
    ID_USUARIO INT NOT NULL,
    ID_METODO INT NOT NULL,
    ID_ESTADO_VENTA INT NOT NULL,

    -- Definición de Claves Foráneas
    CONSTRAINT FK_VENTA_USUARIO FOREIGN KEY (ID_USUARIO)
    REFERENCES USUARIO(ID_USUARIO),
    
    CONSTRAINT FK_VENTA_METODO_PAGO FOREIGN KEY (ID_METODO)
    REFERENCES METODO_PAGO(ID_METODO),

    CONSTRAINT FK_VENTA_ESTADO_VENTA FOREIGN KEY (ID_ESTADO_VENTA)
    REFERENCES ESTADO_VENTA(ID_ESTADO_VENTA)
);
GO

-- Tabla: DETALLE_VENTA (Depende de VENTA y PRODUCTO)
PRINT 'Creando tabla DETALLE_VENTA...';
CREATE TABLE DETALLE_VENTA (
    ID_DETALLE_VENTA INT PRIMARY KEY IDENTITY(1,1),
    ID_VENTA INT NOT NULL,
    ID_PRODUCTO INT NOT NULL,
    CANTIDAD INT NOT NULL,
    PRECIO_UNITARIO_AL_VENDER DECIMAL(10, 2) NOT NULL, -- Guarda el precio al momento de la venta

    -- Definición de Claves Foráneas
    CONSTRAINT FK_DETALLE_VENTA_VENTA FOREIGN KEY (ID_VENTA)
    REFERENCES VENTA(ID_VENTA),

    CONSTRAINT FK_DETALLE_VENTA_PRODUCTO FOREIGN KEY (ID_PRODUCTO)
    REFERENCES PRODUCTO(ID_PRODUCTO)
);
GO

/*
================================================================================
III. INSERCIÓN DE DATOS INICIALES (CATÁLOGOS)
================================================================================
*/
PRINT 'Insertando datos iniciales en catálogos...';

-- Insertar roles
IF NOT EXISTS (SELECT 1 FROM ROL WHERE NOMBRE_ROL = 'Administrador')
    INSERT INTO ROL (NOMBRE_ROL) VALUES ('Administrador');
IF NOT EXISTS (SELECT 1 FROM ROL WHERE NOMBRE_ROL = 'Cajero')
    INSERT INTO ROL (NOMBRE_ROL) VALUES ('Cajero');

-- Insertar métodos de pago
IF NOT EXISTS (SELECT 1 FROM METODO_PAGO WHERE NOMBRE_METODO = 'Efectivo')
    INSERT INTO METODO_PAGO (NOMBRE_METODO) VALUES ('Efectivo');
IF NOT EXISTS (SELECT 1 FROM METODO_PAGO WHERE NOMBRE_METODO = 'Tarjeta de Crédito/Débito')
    INSERT INTO METODO_PAGO (NOMBRE_METODO) VALUES ('Tarjeta de Crédito/Débito');

-- Insertar estados de venta
IF NOT EXISTS (SELECT 1 FROM ESTADO_VENTA WHERE NOMBRE_ESTADO_VENTA = 'Finalizada')
    INSERT INTO ESTADO_VENTA (NOMBRE_ESTADO_VENTA) VALUES ('Finalizada');
IF NOT EXISTS (SELECT 1 FROM ESTADO_VENTA WHERE NOMBRE_ESTADO_VENTA = 'Cancelada')
    INSERT INTO ESTADO_VENTA (NOMBRE_ESTADO_VENTA) VALUES ('Cancelada');
GO


-- -----------------------------------------------------------------------------
-- Usuario administrador por defecto:
-- admin
-- admin
-- -----------------------------------------------------------------------------

PRINT 'Insertando usuario administrador por defecto...';

-- Primero nos aseguramos que el ROL 'Administrador' ya exista
IF EXISTS (SELECT 1 FROM ROL WHERE NOMBRE_ROL = 'Administrador')
BEGIN
    -- Insertamos el usuario 'admin' si no existe
    IF NOT EXISTS (SELECT 1 FROM USUARIO WHERE NOMBRE_USUARIO = 'admin')
    BEGIN
        INSERT INTO USUARIO (
            APATERNO,
            AMATERNO,
            NOMBRE,
            NOMBRE_USUARIO,
            PASSWORD_HASH,
            ID_ROL
            -- La FECHA_CREACION se pone sola por el DEFAULT GETDATE()
        )
        VALUES (
            'Admin',    -- APATERNO
            'Sistema',  -- AMATERNO (o puedes poner 'Admin' también)
            'Administrador', -- NOMBRE
            'admin',    -- NOMBRE_USUARIO
            'admin', -- PASSWORD_HASH 
            
            -- Buscamos el ID_ROL del 'Administrador' dinámicamente
            (SELECT ID_ROL FROM ROL WHERE NOMBRE_ROL = 'Administrador')
        );
        PRINT 'Usuario [admin] creado exitosamente.';
    END
    ELSE
    BEGIN
        PRINT 'El usuario [admin] ya existe.';
    END
END
ELSE
BEGIN
    PRINT 'Error: No se encontró el ROL ''Administrador'' para asignar al usuario admin.';
END
GO


PRINT '==============================================';
PRINT 'SCRIPT FINALIZADO. Base de datos y tablas creadas exitosamente.';
PRINT '==============================================';