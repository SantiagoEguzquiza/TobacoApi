-- ===============================================
-- Migración manual: Agregar coordenadas GPS a Clientes
-- ===============================================

-- Verificar si las columnas ya existen
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Latitud')
BEGIN
    ALTER TABLE Clientes 
    ADD Latitud FLOAT NULL;
    
    PRINT '✅ Columna Latitud agregada a Clientes';
END
ELSE
BEGIN
    PRINT '⚠️ Columna Latitud ya existe';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Clientes' AND COLUMN_NAME = 'Longitud')
BEGIN
    ALTER TABLE Clientes 
    ADD Longitud FLOAT NULL;
    
    PRINT '✅ Columna Longitud agregada a Clientes';
END
ELSE
BEGIN
    PRINT '⚠️ Columna Longitud ya existe';
END

PRINT '';
PRINT '✅ Migración completada exitosamente';
PRINT 'Ahora puedes ejecutar el script DatosPruebaEntregas.sql';

