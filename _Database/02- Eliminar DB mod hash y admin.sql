/*
============================================================
SCRIPT ELIMINACIÓN DB CAMBIOS HASH Y ADMIN
============================================================
*/


USE [master];
GO

ALTER DATABASE [PuntoDeVentaDB]
SET SINGLE_USER 
WITH ROLLBACK IMMEDIATE; 
GO

PRINT 'Borrando base de datos [PuntoDeVentaDB]...';
DROP DATABASE [PuntoDeVentaDB];
GO

PRINT '¡Base de datos eliminada!.';