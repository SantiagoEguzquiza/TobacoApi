# TobacoApi

Backend para la aplicación móvil Tobaco. Esta API RESTful, desarrollada en .NET Core, provee los servicios necesarios para la gestión de inventario, clientes, facturación digital y cotizaciones en tiempo real, orientada a empresas proveedoras y distribuidoras de mercadería.

## Tabla de Contenidos

- [Descripción](#descripción)
- [Características](#características)
- [Tecnologías Utilizadas](#tecnologías-utilizadas)
- [Instalación y Ejecución](#instalación-y-ejecución)
- [Configuración](#configuración)
- [Uso](#uso)
- [Futuras mejoras](#futuras-mejoras)
- [Contribución](#contribución)

---

## Descripción

TobacoApi es el backend de la app Tobaco y expone una serie de endpoints para la administración de inventario, clientes, facturación electrónica y consulta de monedas. Está diseñado para integrarse fácilmente con aplicaciones móviles o web, y utiliza SQL Server (o SQLite para pruebas o desarrollo ligero) como base de datos principal.

## Características

- **Gestión de inventario:** Endpoints para consultar, agregar y actualizar stocks y productos.
- **Facturación digital:** Creación y gestión de facturas electrónicas.
- **Cotizaciones de monedas:** Consulta de tasas de cambio actualizadas.
- **Gestión de clientes:** Registro, edición y consulta de información de clientes.
- **Autenticación:** Seguridad basada en JWT para acceso seguro a la API.
- **Escalable y modular:** Arquitectura pensada para crecer y adaptarse a nuevas necesidades.

## Tecnologías Utilizadas

- **.NET Core / ASP.NET Core:** Framework principal para el desarrollo del backend.
- **C#:** Lenguaje de programación.
- **Entity Framework Core:** ORM para la gestión de la base de datos.
- **SQL Server / SQLite:** Motores de base de datos soportados.
- **JWT:** Autenticación y autorización.
- **Swagger:** Documentación interactiva de la API.

## Instalación y Ejecución

1. Clona este repositorio:

   ```bash
   git clone https://github.com/SantiagoEguzquiza/TobacoApi.git
   cd TobacoApi
   ```

2. Restaura los paquetes necesarios:

   ```bash
   dotnet restore
   ```

3. Configura la cadena de conexión a la base de datos en `appsettings.json` o mediante variables de entorno. Ejemplo:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=TU_SERVIDOR;Database=TU_BDD;User Id=TU_USUARIO;Password=TU_PASSWORD;"
   }
   ```

4. Ejecuta las migraciones para preparar la base de datos:

   ```bash
   dotnet ef database update
   ```

   **⚠️ NOTA IMPORTANTE:** Si encuentras el error "Column name 'SortOrder' is specified more than once" al ejecutar las migraciones, esto significa que la columna ya existe en tu base de datos. La migración está diseñada para manejar este caso automáticamente, pero si persiste el problema, ejecuta:

   ```bash
   # Verificar estado de migraciones
   dotnet ef migrations list
   
   # Si hay migraciones pendientes, aplicarlas una por una
   dotnet ef database update 20251002233543_AddSortOrderToCategorias
   ```

5. Inicia la API:

   ```bash
   dotnet run
   ```

   Por defecto, la API estará disponible en `https://localhost:5001`.

## Configuración

- **Base de datos:** Modifica la cadena de conexión en `appsettings.json` según tu entorno (SQL Server recomendado para producción).
- **Variables de entorno:** Puedes sobrescribir valores sensibles (como cadena de conexión, claves JWT, etc.) usando variables de entorno.
- **Swagger:** Accede a la documentación y prueba interactiva en `/swagger` al iniciar la API.

## Uso

1. Consulta la documentación Swagger para ver y probar los endpoints disponibles.
2. Integra la API con la aplicación móvil Tobaco u otras aplicaciones compatibles.
3. Utiliza las rutas protegidas autenticándote con un token JWT válido.

## Futuras mejoras

- **WebSockets para notificaciones en tiempo real.**
- **Panel de administración web.**
- **Soporte para múltiples monedas y regiones.**
- **Integración con proveedores externos para cotizaciones.**
- **Reportes y analítica avanzada.**

## Contribución

¿Quieres contribuir? Por favor, abre un issue para discutir tus propuestas antes de hacer un pull request. Se agradecen mejoras, parches de seguridad y nuevas funcionalidades.

---
