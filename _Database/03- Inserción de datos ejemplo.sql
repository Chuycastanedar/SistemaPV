USE [PuntoDeVentaDB];
GO

/* ============================================================
   I. INSERTAR PRODUCTOS (25 REGISTROS)
   ============================================================ */
PRINT 'Insertando productos de ejemplo...';

INSERT INTO PRODUCTO (NOMBRE_PRODUCTO, PRECIO, CANTIDAD_STOCK, DESCRIPCION)
VALUES
('Coca-Cola 600ml', 18.50, 120, 'Refresco sabor cola'),
('Pepsi 600ml', 17.90, 100, 'Refresco sabor cola'),
('Agua Ciel 1L', 14.00, 80, 'Agua purificada sin gas'),
('Galletas Oreo 117g', 22.00, 60, 'Galletas de chocolate con crema'),
('Papas Sabritas 45g', 19.50, 75, 'Papas fritas clásicas con sal'),
('Chocolate Hershey 40g', 16.00, 50, 'Chocolate con leche Hershey'),
('Pan Bimbo Blanco 680g', 45.00, 40, 'Pan de caja blanco'),
('Leche Lala 1L', 27.50, 55, 'Leche entera pasteurizada'),
('Cereal Zucaritas 300g', 48.90, 35, 'Cereal de maíz azucarado'),
('Atún Dolores 140g', 25.00, 70, 'Atún en agua'),
('Arroz Verde Valle 1kg', 38.00, 60, 'Arroz blanco pulido'),
('Frijoles Isadora 430g', 24.50, 50, 'Frijoles refritos'),
('Aceite Nutrioli 1L', 45.00, 45, 'Aceite vegetal comestible'),
('Azúcar Zulka 1kg', 33.50, 55, 'Azúcar de caña refinada'),
('Sal La Fina 1kg', 19.00, 80, 'Sal yodada refinada'),
('Café Legal 250g', 56.00, 40, 'Café molido mezcla suave'),
('Huevos San Juan 12pzas', 48.00, 30, 'Huevo blanco tamaño mediano'),
('Mantequilla Lala 90g', 22.50, 45, 'Mantequilla con sal'),
('Yogurt Yoplait 250ml', 18.00, 60, 'Yogurt natural con frutas'),
('Refresco Fanta 600ml', 17.50, 80, 'Refresco sabor naranja'),
('Refresco Sprite 600ml', 17.50, 70, 'Refresco sabor limón'),
('Chicles Trident 8pzas', 15.00, 40, 'Chicles sabor menta'),
('Jugo Del Valle 1L', 24.00, 55, 'Jugo natural de frutas'),
('Galletas Marías Gamesa 170g', 20.00, 65, 'Galletas tipo María'),
('Sopa Maruchan 64g', 17.00, 90, 'Sopa instantánea sabor pollo');
GO

/* ============================================================
   II. INSERTAR VENTAS (10 REGISTROS)
   ============================================================ */
PRINT 'Insertando ventas de ejemplo...';

-- Suponemos que existen:
-- ID_USUARIO = 1 (admin)
-- ID_METODO: 1 = Efectivo, 2 = Tarjeta
-- ID_ESTADO_VENTA: 1 = Finalizada, 2 = Cancelada

INSERT INTO VENTA (FECHA_VENTA, MONTO_TOTAL, ID_USUARIO, ID_METODO, ID_ESTADO_VENTA)
VALUES
('2025-10-01 10:45:00', 150.00, 1, 1, 1),
('2025-10-01 12:10:00', 85.50, 1, 2, 1),
('2025-10-02 09:30:00', 230.75, 1, 1, 1),
('2025-10-02 17:20:00', 99.90, 1, 2, 1),
('2025-10-03 11:15:00', 48.00, 1, 1, 1),
('2025-10-04 15:40:00', 275.60, 1, 2, 1),
('2025-10-05 13:55:00', 310.00, 1, 1, 1),
('2025-10-06 18:00:00', 180.00, 1, 1, 1), -- Cancelada
('2025-10-07 08:50:00', 220.50, 1, 1, 1),
('2025-10-07 20:10:00', 95.00, 1, 2, 1);
GO

/* ============================================================
   III. INSERTAR DETALLES DE VENTA (RELACIÓN CON VENTA Y PRODUCTO)
   ============================================================ */
PRINT 'Insertando detalles de venta...';

-- Venta 1
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES 
(1, 1, 3, 18.50), -- Coca-Cola
(1, 4, 2, 22.00), -- Oreo
(1, 5, 2, 19.50);

-- Venta 2
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES
(2, 2, 2, 17.90),
(2, 7, 1, 45.00);

-- Venta 3
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES
(3, 8, 2, 27.50),
(3, 9, 1, 48.90),
(3, 13, 2, 45.00);

-- Venta 4
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES
(4, 10, 2, 25.00),
(4, 19, 2, 18.00);

-- Venta 5
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES
(5, 16, 1, 56.00);

-- Venta 6
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES
(6, 17, 2, 48.00),
(6, 12, 3, 24.50),
(6, 18, 2, 22.50);

-- Venta 7
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES
(7, 15, 4, 19.00),
(7, 14, 3, 33.50),
(7, 20, 2, 17.50);

-- Venta 8 (Cancelada)
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES
(8, 23, 3, 24.00),
(8, 24, 2, 20.00);

-- Venta 9
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES
(9, 3, 2, 14.00),
(9, 21, 3, 17.50),
(9, 25, 2, 17.00);

-- Venta 10
INSERT INTO DETALLE_VENTA (ID_VENTA, ID_PRODUCTO, CANTIDAD, PRECIO_UNITARIO_AL_VENDER)
VALUES
(10, 6, 2, 16.00),
(10, 11, 1, 38.00);
GO

PRINT '==============================================';
PRINT 'DATOS DE PRODUCTOS, VENTAS Y DETALLES INSERTADOS EXITOSAMENTE';
PRINT '==============================================';
GO
