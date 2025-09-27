# Implementación de Token JWT de 30 Días

## Resumen
Se ha implementado la duración de 30 días para los tokens de autenticación JWT, mejorando la experiencia del usuario al reducir la frecuencia de inicio de sesión.

## Cambios Realizados

### 1. Backend - Configuración de JWT

#### JwtSettings.cs
- **Archivo**: `TobacoBackend/Domain/Models/JwtSettings.cs`
- **Cambio**: Agregada propiedad `ExpirationDays` con valor por defecto de 30 días
- **Código**:
```csharp
public class JwtSettings
{
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpirationDays { get; set; } = 30; // Duración del token en días, por defecto 30 días
}
```

#### TokenService.cs
- **Archivo**: `TobacoBackend/Services/TokenService.cs`
- **Cambio**: Modificada la generación del token para usar la configuración de duración
- **Código anterior**:
```csharp
expires: DateTime.UtcNow.AddHours(1),
```
- **Código nuevo**:
```csharp
expires: DateTime.UtcNow.AddDays(_jwtSettings.ExpirationDays),
```

#### appsettings.json
- **Archivo**: `TobacoBackend/appsettings.json`
- **Cambio**: Agregada configuración de duración del token
- **Código**:
```json
"JwtSettings": {
  "Secret": "EstaEsUnaClaveMuySeguraParaDesarrollo12345!",
  "Issuer": "https://localhost",
  "Audience": "https://localhost",
  "ExpirationDays": 30
}
```

### 2. Frontend - Compatibilidad

El frontend ya está preparado para manejar tokens de larga duración:

#### AuthService.dart
- **Archivo**: `TobacoFrontend/lib/Services/Auth_Service/auth_service.dart`
- **Funcionalidad existente**:
  - Almacenamiento del token y fecha de expiración en `SharedPreferences`
  - Validación automática de expiración del token
  - Manejo de renovación de tokens

#### LoginResponse.dart
- **Archivo**: `TobacoFrontend/lib/Models/LoginResponse.dart`
- **Funcionalidad existente**:
  - Modelo que incluye `expiresAt` para manejar la fecha de expiración
  - Compatible con la respuesta del backend

## Beneficios

### 1. Experiencia de Usuario
- **Reducción de fricción**: Los usuarios no necesitan iniciar sesión frecuentemente
- **Mejor productividad**: Acceso continuo a la aplicación durante 30 días
- **Ideal para uso empresarial**: Perfecto para aplicaciones de trabajo diario

### 2. Configurabilidad
- **Flexible**: La duración se puede ajustar fácilmente en `appsettings.json`
- **Mantenible**: Cambios centralizados en la configuración
- **Escalable**: Fácil de modificar para diferentes entornos

### 3. Seguridad
- **Balanceado**: 30 días es un período razonable entre seguridad y usabilidad
- **Validación automática**: El frontend valida la expiración automáticamente
- **Renovación**: Los tokens se renuevan automáticamente al validar

## Configuración por Entorno

### Desarrollo
```json
"JwtSettings": {
  "ExpirationDays": 30
}
```

### Producción (Recomendado)
```json
"JwtSettings": {
  "ExpirationDays": 7
}
```

### Testing
```json
"JwtSettings": {
  "ExpirationDays": 1
}
```

## Verificación

### Backend
1. Verificar que el token se genere con la duración correcta
2. Confirmar que la fecha de expiración se incluya en la respuesta de login
3. Validar que la configuración se lea correctamente desde `appsettings.json`

### Frontend
1. Verificar que el token se almacene correctamente
2. Confirmar que la validación de expiración funcione
3. Probar el flujo completo de autenticación

## Consideraciones de Seguridad

### Ventajas
- **Menos solicitudes de login**: Reduce la carga en el servidor
- **Mejor experiencia**: Los usuarios permanecen autenticados más tiempo
- **Configurabilidad**: Fácil ajuste según necesidades de seguridad

### Consideraciones
- **Tokens más largos**: Mayor ventana de exposición si se compromete
- **Almacenamiento local**: Los tokens se almacenan en el dispositivo
- **Validación del lado del cliente**: Dependencia de la validación local

### Recomendaciones
- **Monitoreo**: Implementar logging de accesos sospechosos
- **Renovación automática**: Considerar implementar refresh tokens
- **Revocación**: Implementar lista de tokens revocados si es necesario

## Próximos Pasos

1. **Monitoreo**: Implementar logging de autenticación
2. **Refresh Tokens**: Considerar implementar renovación automática
3. **Revocación**: Implementar capacidad de revocar tokens específicos
4. **Auditoría**: Agregar logs de seguridad para tokens

## Conclusión

La implementación de tokens de 30 días mejora significativamente la experiencia del usuario mientras mantiene un nivel de seguridad apropiado. La configuración es flexible y permite ajustes según las necesidades específicas del entorno de despliegue.
