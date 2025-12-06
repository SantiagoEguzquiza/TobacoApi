USE [TobacoDB]
GO

-- Script para eliminar los tenants 3 y 4 y todos sus datos relacionados
-- IMPORTANTE: Este script eliminará TODOS los datos asociados a estos tenants
-- Asegúrate de hacer un backup antes de ejecutar

BEGIN TRANSACTION;

DECLARE @TenantId3 INT = 3;
DECLARE @TenantId4 INT = 4;

PRINT 'Iniciando eliminación de tenants 3 y 4...';

-- Eliminar registros relacionados con TenantId 3 y 4
-- Orden: primero las tablas dependientes (que referencian otras), luego las principales

-- 1. Eliminar VentaPagos (depende de Ventas)
DELETE vp FROM [dbo].[VentaPagos] vp
INNER JOIN [dbo].[Ventas] v ON vp.[VentaId] = v.[Id]
WHERE v.[TenantId] IN (@TenantId3, @TenantId4);
PRINT 'VentaPagos eliminados';

-- 2. Eliminar VentasProductos (depende de Ventas)
DELETE vp FROM [dbo].[VentasProductos] vp
INNER JOIN [dbo].[Ventas] v ON vp.[VentaId] = v.[Id]
WHERE v.[TenantId] IN (@TenantId3, @TenantId4);
PRINT 'VentasProductos eliminados';

-- 3. Eliminar Ventas
DELETE FROM [dbo].[Ventas] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'Ventas eliminadas';

-- 4. Eliminar Abonos (depende de Clientes)
DELETE FROM [dbo].[Abonos] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'Abonos eliminados';

-- 5. Eliminar PreciosEspeciales (depende de Clientes y Productos)
DELETE FROM [dbo].[PreciosEspeciales] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'PreciosEspeciales eliminados';

-- 6. Eliminar ProductQuantityPrices (depende de Productos)
DELETE FROM [dbo].[ProductQuantityPrices] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'ProductQuantityPrices eliminados';

-- 7. Eliminar ProductosAFavor (depende de Clientes y Productos)
DELETE FROM [dbo].[ProductosAFavor] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'ProductosAFavor eliminados';

-- 8. Eliminar Productos
DELETE FROM [dbo].[Productos] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'Productos eliminados';

-- 9. Eliminar Categorias
DELETE FROM [dbo].[Categorias] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'Categorias eliminadas';

-- 10. Eliminar Clientes
DELETE FROM [dbo].[Clientes] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'Clientes eliminados';

-- 11. Eliminar PermisosEmpleados (depende de Users)
DELETE FROM [dbo].[PermisosEmpleados] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'PermisosEmpleados eliminados';

-- 12. Eliminar Asistencias (depende de Users)
DELETE FROM [dbo].[Asistencias] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'Asistencias eliminadas';

-- 13. Eliminar RecorridosProgramados (depende de Users)
DELETE FROM [dbo].[RecorridosProgramados] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'RecorridosProgramados eliminados';

-- 14. Eliminar Users (administradores y empleados del tenant)
DELETE FROM [dbo].[Users] WHERE [TenantId] IN (@TenantId3, @TenantId4);
PRINT 'Users eliminados';

-- 14. Finalmente, eliminar los Tenants
DELETE FROM [dbo].[Tenants] WHERE [Id] IN (@TenantId3, @TenantId4);
PRINT 'Tenants eliminados';

-- Verificar que se eliminaron
IF EXISTS (SELECT 1 FROM [dbo].[Tenants] WHERE [Id] IN (@TenantId3, @TenantId4))
BEGIN
    PRINT 'ERROR: Los tenants aún existen. Revirtiendo transacción...';
    ROLLBACK TRANSACTION;
    RETURN;
END

PRINT 'Tenants 3 y 4 eliminados exitosamente.';
COMMIT TRANSACTION;
GO

-- Verificar que no quedaron registros huérfanos
SELECT 'Categorias' AS Tabla, COUNT(*) AS Registros FROM [dbo].[Categorias] WHERE [TenantId] IN (3, 4)
UNION ALL
SELECT 'Clientes', COUNT(*) FROM [dbo].[Clientes] WHERE [TenantId] IN (3, 4)
UNION ALL
SELECT 'Users', COUNT(*) FROM [dbo].[Users] WHERE [TenantId] IN (3, 4)
UNION ALL
SELECT 'Productos', COUNT(*) FROM [dbo].[Productos] WHERE [TenantId] IN (3, 4)
UNION ALL
SELECT 'Ventas', COUNT(*) FROM [dbo].[Ventas] WHERE [TenantId] IN (3, 4);
GO

