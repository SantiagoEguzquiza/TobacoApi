USE [TobacoDB]
GO

-- Primero, crear un tenant especial con ID = 0 para recursos compartidos (si no existe)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Tenants] WHERE [Id] = 0)
BEGIN
    -- Verificar si la tabla tiene columna de identidad y si podemos insertar ID = 0
    -- Si la columna Id es IDENTITY, necesitamos desactivarla temporalmente o usar SET IDENTITY_INSERT
    SET IDENTITY_INSERT [dbo].[Tenants] ON;
    
    INSERT INTO [dbo].[Tenants] 
    (
        [Id],
        [Nombre],
        [Email],
        [IsActive]
    )
    VALUES 
    (
        0,
        'Sistema Compartido',
        'sistema@compartido.local',
        1
    );
    
    SET IDENTITY_INSERT [dbo].[Tenants] OFF;
END
ELSE
BEGIN
    -- Si ya existe, asegurarse de que esté activo
    UPDATE [dbo].[Tenants]
    SET [IsActive] = 1,
        [Nombre] = 'Sistema Compartido'
    WHERE [Id] = 0;
END
GO

-- Actualizar Consumidor Final para que sea compartido entre todos los tenants (TenantId = 0)
UPDATE [dbo].[Clientes]
SET 
    [Nombre] = 'Consumidor Final',
    [Direccion] = 'Sin dirección especificada',
    [Telefono] = '0',
    [Deuda] = '0',
    [DescuentoGlobal] = 0.00,
    [Latitud] = NULL,
    [Longitud] = NULL,
    [Visible] = 1,
    [TenantId] = 0
WHERE [Nombre] = 'Consumidor Final'
GO

-- Si no existe, crear el Consumidor Final con TenantId = 0
IF NOT EXISTS (SELECT 1 FROM [dbo].[Clientes] WHERE [Nombre] = 'Consumidor Final' AND [TenantId] = 0)
BEGIN
    INSERT INTO [dbo].[Clientes] 
    (
        [Nombre],
        [Direccion],
        [Telefono],
        [Deuda],
        [DescuentoGlobal],
        [Latitud],
        [Longitud],
        [Visible],
        [TenantId]
    )
    VALUES 
    (
        'Consumidor Final',
        'Sin dirección especificada',
        '0',
        '0',
        0.00,
        NULL,
        NULL,
        1,
        0
    )
END
GO

-- Verificar que se actualizó correctamente
SELECT 
    [Id],
    [Nombre],
    [TenantId],
    [Direccion],
    [Telefono],
    [Deuda],
    [Visible]
FROM [dbo].[Clientes]
WHERE [Nombre] = 'Consumidor Final'
GO

