-- ===============================================
-- Script para insertar datos de prueba de entregas
-- ===============================================

-- 1. Actualizar clientes existentes con coordenadas GPS
-- (Coordenadas de ejemplo en Asunción, Paraguay)

UPDATE Clientes 
SET Latitud = -25.2637, Longitud = -57.5759 
WHERE Id = 1;

UPDATE Clientes 
SET Latitud = -25.2800, Longitud = -57.6000 
WHERE Id = 2;

UPDATE Clientes 
SET Latitud = -25.2500, Longitud = -57.5500 
WHERE Id = 3;

-- 2. O crear nuevos clientes con coordenadas
INSERT INTO Clientes (Nombre, Direccion, Telefono, Deuda, DescuentoGlobal, Latitud, Longitud)
VALUES 
('Supermercado Los Álamos', 'Av. España 1234', '0981-123456', '0', 0, -25.2637, -57.5759),
('Mini Market El Ahorro', 'Calle Palma 567', '0982-234567', '0', 0, -25.2800, -57.6000),
('Almacén Don Pedro', 'Av. Artigas 890', '0983-345678', '0', 0, -25.2500, -57.5500),
('Kiosco La Esquina', 'Calle Brasil 123', '0984-456789', '0', 0, -25.2700, -57.5800),
('Despensa María', 'Av. Eusebio Ayala 456', '0985-567890', '0', 0, -25.2600, -57.5600);

-- 3. Obtener el ID de tu usuario (reemplaza 'tuUsuario' con tu nombre de usuario real)
DECLARE @UsuarioId INT;
SELECT TOP 1 @UsuarioId = Id FROM Users WHERE Username = 'admin' OR Username = 'repartidor';

-- 4. Crear ventas del día de hoy para este usuario
-- (Estas aparecerán como entregas pendientes en el mapa)

DECLARE @Fecha DATE = GETDATE();

-- Venta 1
INSERT INTO Ventas (ClienteId, Total, Fecha, MetodoPago, UsuarioId, EstadoEntrega)
SELECT TOP 1 
    Id, 
    150000, 
    @Fecha, 
    0, -- EFECTIVO
    @UsuarioId, 
    0 -- NO_ENTREGADA
FROM Clientes 
WHERE Latitud IS NOT NULL 
ORDER BY NEWID();

-- Venta 2
INSERT INTO Ventas (ClienteId, Total, Fecha, MetodoPago, UsuarioId, EstadoEntrega)
SELECT TOP 1 
    Id, 
    250000, 
    @Fecha, 
    0, 
    @UsuarioId, 
    0
FROM Clientes 
WHERE Latitud IS NOT NULL 
  AND Id NOT IN (SELECT TOP 1 ClienteId FROM Ventas WHERE Fecha = @Fecha ORDER BY Id DESC)
ORDER BY NEWID();

-- Venta 3
INSERT INTO Ventas (ClienteId, Total, Fecha, MetodoPago, UsuarioId, EstadoEntrega)
SELECT TOP 1 
    Id, 
    180000, 
    @Fecha, 
    0, 
    @UsuarioId, 
    0
FROM Clientes 
WHERE Latitud IS NOT NULL 
  AND Id NOT IN (SELECT TOP 2 ClienteId FROM Ventas WHERE Fecha = @Fecha ORDER BY Id DESC)
ORDER BY NEWID();

-- Venta 4 (esta ya está entregada para probar el estado)
INSERT INTO Ventas (ClienteId, Total, Fecha, MetodoPago, UsuarioId, EstadoEntrega)
SELECT TOP 1 
    Id, 
    120000, 
    @Fecha, 
    0, 
    @UsuarioId, 
    2 -- ENTREGADA
FROM Clientes 
WHERE Latitud IS NOT NULL 
  AND Id NOT IN (SELECT TOP 3 ClienteId FROM Ventas WHERE Fecha = @Fecha ORDER BY Id DESC)
ORDER BY NEWID();

-- Venta 5 (parcialmente entregada)
INSERT INTO Ventas (ClienteId, Total, Fecha, MetodoPago, UsuarioId, EstadoEntrega)
SELECT TOP 1 
    Id, 
    300000, 
    @Fecha, 
    1, -- CREDITO
    @UsuarioId, 
    1 -- PARCIAL
FROM Clientes 
WHERE Latitud IS NOT NULL 
  AND Id NOT IN (SELECT TOP 4 ClienteId FROM Ventas WHERE Fecha = @Fecha ORDER BY Id DESC)
ORDER BY NEWID();

-- 5. Verificar los datos insertados
SELECT 
    v.Id AS VentaId,
    c.Nombre AS Cliente,
    c.Direccion,
    c.Latitud,
    c.Longitud,
    v.Total,
    v.Fecha,
    v.EstadoEntrega,
    u.Username AS Repartidor
FROM Ventas v
INNER JOIN Clientes c ON v.ClienteId = c.Id
LEFT JOIN Users u ON v.UsuarioId = u.Id
WHERE v.Fecha = CAST(GETDATE() AS DATE)
ORDER BY v.Id DESC;

PRINT '✅ Datos de prueba insertados correctamente';
PRINT 'Ahora puedes abrir la app y ver las entregas en el mapa';

