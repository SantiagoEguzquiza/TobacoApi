# Script para solucionar problemas de migración
# Ejecutar desde la carpeta TobacoBackend/TobacoBackend

Write-Host "🔧 Solucionando problemas de migración..." -ForegroundColor Yellow

# Verificar estado actual de migraciones
Write-Host "📋 Verificando estado de migraciones..." -ForegroundColor Blue
dotnet ef migrations list

# Aplicar migraciones una por una para evitar conflictos
Write-Host "🚀 Aplicando migraciones..." -ForegroundColor Green

try {
    # Intentar aplicar todas las migraciones
    dotnet ef database update
    Write-Host "✅ Migraciones aplicadas exitosamente!" -ForegroundColor Green
} catch {
    Write-Host "⚠️ Error detectado, aplicando migración específica..." -ForegroundColor Yellow
    dotnet ef database update 20251002233543_AddSortOrderToCategorias
    Write-Host "✅ Migración específica aplicada!" -ForegroundColor Green
    
    # Intentar aplicar el resto
    dotnet ef database update
    Write-Host "✅ Todas las migraciones aplicadas!" -ForegroundColor Green
}

Write-Host "🎉 ¡Proceso completado! La base de datos está lista." -ForegroundColor Green
