# 📦 Guía Completa: Sistema de Entregas

## 🎯 Resumen de Cambios

Se ha implementado un **sistema completo de entregas** con mapa interactivo en Flutter. Los cambios incluyen:

### Backend (.NET)
✅ Nuevo controlador `EntregasController.cs` con endpoints:
- `GET /api/Entregas/mis-entregas` - Obtiene entregas del día
- `PUT /api/Entregas/{id}/estado` - Actualiza estado de entrega
- `PUT /api/Entregas/{id}/notas` - Agrega notas

✅ Campos nuevos en `Cliente`:
- `Latitud` (FLOAT, nullable)
- `Longitud` (FLOAT, nullable)

### Frontend (Flutter)
✅ Mapa interactivo con Google Maps
✅ Marcadores de ubicación actual y clientes
✅ Rutas optimizadas entre puntos
✅ Estados de entrega (Pendiente, Parcial, Entregada)
✅ Modo claro/oscuro automático
✅ Funciona offline con SQLite

---

## 🚀 Pasos para Probar

### 1️⃣ Actualizar la Base de Datos

Abre **SQL Server Management Studio** y ejecuta:

```bash
# Ruta del script
C:\Users\rodri\OneDrive\Escritorio\Tobaco 2\TobacoApi\TobacoBackend\TobacoBackend\Scripts\AddCoordenadasClientes.sql
```

Este script agrega las columnas `Latitud` y `Longitud` a la tabla `Clientes`.

---

### 2️⃣ Insertar Datos de Prueba

Ejecuta el segundo script:

```bash
# Ruta del script
C:\Users\rodri\OneDrive\Escritorio\Tobaco 2\TobacoApi\TobacoBackend\TobacoBackend\Scripts\DatosPruebaEntregas.sql
```

Este script:
- Crea 5 clientes con direcciones en Asunción
- Crea 5 ventas del día de hoy
- Las asigna a tu usuario actual
- Define diferentes estados de entrega

---

### 3️⃣ Reiniciar el Backend

En la terminal del backend:

```bash
cd "C:\Users\rodri\OneDrive\Escritorio\Tobaco 2\TobacoApi\TobacoBackend\TobacoBackend"
dotnet clean
dotnet build
dotnet run
```

---

### 4️⃣ Probar en la App Flutter

1. **Reinicia la app Flutter** (presiona `R` en la terminal)
2. **Inicia sesión** con tu usuario
3. **Abre "Mapa de Entregas"** desde el menú
4. Deberías ver:
   - 🟢 Tu ubicación actual (marcador verde)
   - 📦 Marcadores de clientes pendientes (azul)
   - ✅ Marcadores de clientes entregados (verde)
   - ⚠️ Marcadores parciales (naranja)
   - 🛣️ Ruta verde hacia el siguiente cliente

---

## 🗺️ Funcionalidades del Mapa

### Panel Superior (Información)
- 🟡 **Pendientes**: Cantidad de entregas por hacer
- 🟢 **Completadas**: Entregas finalizadas
- 🟢 **Distancia**: Kilómetros recorridos

### Botones Flotantes
- 📍 **Mi Ubicación**: Centra el mapa en ti
- 🔍 **Ver Todas**: Ajusta el zoom para ver todas las entregas
- 📊 **Estadísticas**: Muestra resumen detallado

### Interacción
- **Toca un marcador** para ver detalles del cliente
- **Botón "Siguiente"** navega a la próxima entrega
- **Marcar como entregado** actualiza el estado (online y offline)

---

## 🎨 Colores del Mapa

### Modo Claro ☀️
- Fondo de mapa: Claro
- Botones: Fondo blanco + Íconos verdes
- Pendientes: Amarillo

### Modo Oscuro 🌑
- Fondo de mapa: Oscuro (estilo Uber)
- Botones: Fondo negro + Íconos verdes
- Pendientes: Amarillo

---

## 🔧 Troubleshooting

### "No aparecen entregas en el mapa"

**Causa:** No hay ventas del día de hoy asignadas a tu usuario.

**Solución:**
1. Verifica que ejecutaste `DatosPruebaEntregas.sql`
2. Verifica tu ID de usuario en la base de datos:
   ```sql
   SELECT * FROM Users WHERE Username = 'tuUsuario';
   ```
3. Verifica que las ventas se crearon hoy:
   ```sql
   SELECT * FROM Ventas 
   WHERE UsuarioId = TU_ID 
   AND Fecha = CAST(GETDATE() AS DATE);
   ```

---

### "Error al obtener entregas del día"

**Causa:** El backend no se reinició después de agregar el nuevo controller.

**Solución:**
```bash
cd "C:\Users\rodri\OneDrive\Escritorio\Tobaco 2\TobacoApi\TobacoBackend\TobacoBackend"
dotnet clean
dotnet build
dotnet run
```

---

### "Clientes sin coordenadas"

**Causa:** Los clientes no tienen latitud/longitud.

**Solución:**
```sql
-- Ver clientes sin coordenadas
SELECT * FROM Clientes WHERE Latitud IS NULL;

-- Actualizar manualmente (coordenadas de Asunción)
UPDATE Clientes 
SET Latitud = -25.2637, Longitud = -57.5759 
WHERE Id = TU_CLIENTE_ID;
```

---

## 📍 Coordenadas de Ejemplo (Asunción, Paraguay)

Puedes usar estas coordenadas para probar:

| Zona | Latitud | Longitud |
|------|---------|----------|
| Centro | -25.2637 | -57.5759 |
| Villa Morra | -25.2800 | -57.6000 |
| Carmelitas | -25.2500 | -57.5500 |
| San Vicente | -25.2700 | -57.5800 |
| Recoleta | -25.2600 | -57.5600 |

---

## 🎯 Estados de Entrega

| Código | Nombre | Color Marcador | Significado |
|--------|--------|----------------|-------------|
| 0 | NO_ENTREGADA | 🔵 Azul | Pendiente de entrega |
| 1 | PARCIAL | 🟠 Naranja | Entrega parcial realizada |
| 2 | ENTREGADA | 🟢 Verde | Completamente entregada |

---

## 🔄 Sincronización Offline

El sistema funciona **completamente offline**:

✅ **Con internet:**
- Carga entregas desde el servidor
- Guarda en cache local (SQLite)
- Actualiza estados en tiempo real

✅ **Sin internet:**
- Usa cache local
- Marca entregas para sincronizar
- Sincroniza automáticamente cuando vuelve la conexión

---

## 📱 Testing en Dispositivo Real

Para probar con GPS real en tu teléfono:

1. Conecta tu teléfono al PC
2. Habilita "Depuración USB"
3. En la terminal:
   ```bash
   flutter devices
   flutter run -d TU_DISPOSITIVO
   ```
4. Sal a la calle y prueba entregar pedidos reales

---

## 🎉 ¡Todo Listo!

Ahora tienes un sistema completo de entregas con:
- 🗺️ Mapa interactivo
- 📍 Geolocalización en tiempo real
- 🛣️ Rutas optimizadas
- 📦 Gestión de estados
- 💾 Funcionalidad offline
- 🎨 Tema claro/oscuro

**¡A entregar pedidos!** 🚚💨

