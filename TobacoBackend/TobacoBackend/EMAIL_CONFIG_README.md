# Configuración de correo (recuperar contraseña)

Para que se envíe el correo de recuperación de contraseña por Gmail:

## 1. Añadir sección Email en appsettings

En **appsettings.Development.json** (desarrollo) o **appsettings.json** (producción), añade la sección `"Email"` dentro de la raíz, por ejemplo:

```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587,
  "EnableSsl": true,
  "UserName": "tu-gmail@gmail.com",
  "SmtpPassword": "tu-contraseña-de-aplicacion-16-chars",
  "FromAddress": "tu-gmail@gmail.com",
  "FromName": "Provider",
  "BaseUrlForEmails": "http://localhost:5006"
}
```

- **UserName** y **SmtpPassword**: obligatorios. En Gmail **no** uses tu contraseña normal; usa una **contraseña de aplicación**:
  - Cuenta de Google → Seguridad → Verificación en 2 pasos (activada) → Contraseñas de aplicaciones → Generar.
- **BaseUrlForEmails**: URL pública de tu API para que el enlace del correo funcione. En local: `http://TU_IP:5006` o `http://localhost:5006`. En producción: `https://tu-api.railway.app`.

## 2. El usuario debe tener email en el sistema

En **Gestión de Usuarios** (Admin), el usuario que va a recuperar contraseña debe tener un **email** guardado. Si no tiene, el correo no se envía (por seguridad la app no indica si el usuario existe).

## 3. Revisar logs del backend

Si no llega el correo, en la consola donde ejecutas `dotnet run` verás mensajes como:
- `Email no configurado` → falta UserName/SmtpPassword en appsettings.
- `El usuario X no tiene email registrado` → añade el email al usuario.
- `Error al enviar correo` → revisa SmtpPassword (contraseña de aplicación de Gmail) y el firewall.
