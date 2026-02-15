using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TobacoBackend.Controllers
{
    /// <summary>
    /// Página pública para restablecer contraseña (enlace amigable desde el correo).
    /// </summary>
    [ApiController]
    [Route("recuperar-contrasena")]
    [AllowAnonymous]
    public class RecuperarContrasenaController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index([FromQuery] string? token)
        {
            var t = string.IsNullOrEmpty(token) ? "" : System.Net.WebUtility.HtmlEncode(token);
            var apiPath = "/api/User/reset-password";
            var html = $@"<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
  <title>Nueva contraseña - Provider</title>
  <link rel=""preconnect"" href=""https://fonts.googleapis.com"">
  <link rel=""preconnect"" href=""https://fonts.gstatic.com"" crossorigin>
  <link href=""https://fonts.googleapis.com/css2?family=Raleway:wght@400;600;700&display=swap"" rel=""stylesheet"">
  <style>
    * {{ box-sizing: border-box; }}
    body {{ margin: 0; font-family: 'Raleway', system-ui, sans-serif; background: linear-gradient(135deg, #0f0f0f 0%, #1a1a1a 50%, #0d2810 100%); min-height: 100vh; color: #e0e0e0; padding: 24px; display: flex; align-items: center; justify-content: center; }}
    .card {{ background: #1e1e1e; border-radius: 20px; box-shadow: 0 20px 60px rgba(0,0,0,0.4); max-width: 420px; width: 100%; padding: 40px 32px; border: 1px solid #333; }}
    .logo {{ width: 72px; height: 72px; background: linear-gradient(135deg, #4CAF50, #2E7D32); border-radius: 50%; display: flex; align-items: center; justify-content: center; margin: 0 auto 24px; box-shadow: 0 8px 24px rgba(76,175,80,0.3); }}
    .logo svg {{ width: 36px; height: 36px; fill: white; }}
    h1 {{ font-size: 1.5rem; font-weight: 700; text-align: center; margin: 0 0 8px; color: #fff; letter-spacing: 0.5px; }}
    .sub {{ font-size: 0.9rem; color: #9e9e9e; text-align: center; margin-bottom: 28px; line-height: 1.4; }}
    label {{ display: block; font-size: 0.85rem; font-weight: 600; color: #b0b0b0; margin-bottom: 6px; }}
    input[type=""password""] {{ width: 100%; padding: 14px 16px; border: 2px solid #333; border-radius: 12px; font-size: 1rem; background: #2a2a2a; color: #fff; margin-bottom: 16px; font-family: inherit; transition: border-color 0.2s; }}
    input:focus {{ outline: none; border-color: #4CAF50; }}
    input::placeholder {{ color: #666; }}
    #msg {{ min-height: 22px; font-size: 0.9rem; margin-bottom: 12px; }}
    #msg.error {{ color: #f44336; }}
    #msg.ok {{ color: #4CAF50; }}
    button {{ width: 100%; padding: 14px; background: linear-gradient(135deg, #2E7D32, #1B5E20); color: white; border: none; border-radius: 12px; font-size: 1rem; font-weight: 600; font-family: inherit; cursor: pointer; letter-spacing: 0.5px; transition: opacity 0.2s; }}
    button:hover:not(:disabled) {{ opacity: 0.95; }}
    button:disabled {{ opacity: 0.6; cursor: not-allowed; }}
    .brand {{ text-align: center; margin-top: 24px; font-size: 0.8rem; color: #666; }}
  </style>
</head>
<body>
  <div class=""card"">
    <div class=""logo""><svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24""><path d=""M18 8h-1V6c0-2.76-2.24-5-5-5S7 3.24 7 6v2H6c-1.1 0-2 .9-2 2v10c0 1.1.9 2 2 2h12c1.1 0 2-.9 2-2V10c0-1.1-.9-2-2-2zm-6 9c-1.1 0-2-.9-2-2s.9-2 2-2 2 .9 2 2-.9 2-2 2zm3.1-9H8.9V6c0-1.71 1.39-3.1 3.1-3.1 1.71 0 3.1 1.39 3.1 3.1v2z""/></svg></div>
    <h1>Nueva contraseña</h1>
    <p class=""sub"">Elige una contraseña segura (mín. 8 caracteres, una mayúscula, una minúscula y un número).</p>
    <form id=""f"">
      <input type=""hidden"" name=""token"" value=""{t}"">
      <label for=""pw"">Nueva contraseña</label>
      <input type=""password"" id=""pw"" name=""newPassword"" required minlength=""8"" placeholder=""Ej: MiClave123"" autocomplete=""new-password"">
      <label for=""pw2"">Repetir contraseña</label>
      <input type=""password"" id=""pw2"" required minlength=""8"" placeholder=""Repite la contraseña"" autocomplete=""new-password"">
      <div id=""msg""></div>
      <button type=""submit"" id=""btn"">Guardar y continuar</button>
    </form>
    <p class=""brand"">Provider · Sistema de Gestión Comercial</p>
  </div>
  <script>
    var form = document.getElementById('f');
    var msg = document.getElementById('msg');
    var btn = document.getElementById('btn');
    form.onsubmit = async function(e) {{
      e.preventDefault();
      var pw = document.getElementById('pw').value;
      var pw2 = document.getElementById('pw2').value;
      if (pw !== pw2) {{
        msg.className = 'error';
        msg.textContent = 'Las contraseñas no coinciden.';
        return;
      }}
      msg.textContent = '';
      msg.className = '';
      btn.disabled = true;
      try {{
        var r = await fetch('{apiPath}', {{
          method: 'POST',
          headers: {{ 'Content-Type': 'application/json' }},
          body: JSON.stringify({{ token: document.querySelector('[name=token]').value, newPassword: pw }})
        }});
        var j = await r.json();
        if (r.ok) {{
          msg.className = 'ok';
          msg.innerHTML = '✓ Contraseña actualizada. Cierra esta página y abre la app para iniciar sesión.';
          form.reset();
        }} else {{
          msg.className = 'error';
          msg.textContent = j.message || 'No se pudo actualizar. Intenta de nuevo.';
        }}
      }} catch (err) {{
        msg.className = 'error';
        msg.textContent = 'Error de conexión. Revisa tu internet e intenta de nuevo.';
      }}
      btn.disabled = false;
    }};
  </script>
</body>
</html>";
            return Content(html, "text/html; charset=utf-8");
        }
    }
}
