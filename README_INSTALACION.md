# Guía de instalación de MediTrack

MediTrack es una aplicación de escritorio para Windows desarrollada en **C# Windows Forms** que utiliza **MySQL local** como base de datos.

Esta guía está pensada para instalar y ejecutar MediTrack en otro ordenador desde cero, sin conocer previamente el proyecto.

## 1. Requisitos previos

Antes de abrir MediTrack, el ordenador debe tener:

1. Windows 10 o Windows 11.
2. MySQL Server instalado.
3. MySQL Workbench instalado.
4. .NET Desktop Runtime instalado si el ejecutable publicado no es autocontenido.
5. La carpeta `App` incluida en la entrega.
6. La carpeta `Database` incluida en la entrega.

Importante: MySQL es obligatorio. Esta versión de MediTrack no tiene modo sin base de datos.

## 2. Estructura esperada de la entrega

La entrega debería tener una estructura parecida a esta:

```text
MediTrack_Entrega/
├── App/
├── Database/
├── README.md
├── README_DEMO.md
└── README_INSTALACION.md
```

La carpeta `App` debe contener `MediTrack.WinForms.exe` junto con sus archivos `.dll`. No muevas solo el `.exe` fuera de esa carpeta.

## 3. Instalar MySQL

1. Descarga **MySQL Installer Community** desde la web oficial de MySQL.
2. Ejecuta el instalador.
3. Elige una instalación que incluya:
   - MySQL Server.
   - MySQL Workbench.
4. Durante la configuración de MySQL Server:
   - Usa el puerto `3306`.
   - Crea una contraseña para el usuario `root`.
   - Guarda esa contraseña, porque la necesitarás para crear la base de datos y el usuario de la app.
5. Asegúrate de que MySQL queda iniciado como servicio de Windows.

Puedes comprobarlo abriendo MySQL Workbench y conectándote con el usuario `root`.

## 4. Crear la base de datos

1. Abre **MySQL Workbench**.
2. Conéctate como `root`.
3. En MySQL Workbench, usa el menú **File > Open SQL Script...**.
4. Selecciona el archivo:

```text
Database/01_create_database.sql
```

5. Ejecútalo pulsando el icono del **rayo amarillo**.

Este script crea la base de datos `meditrak_db` si no existe.

La aplicación se encarga de crear o actualizar las tablas necesarias al arrancar mediante su inicializador interno. Por eso este script solo prepara la base de datos.

## 5. Crear el usuario de la aplicación

1. En MySQL Workbench, conectado como `root`, usa el menú **File > Open SQL Script...**.
2. Selecciona el archivo:

```text
Database/02_create_user.sql
```

3. Ejecútalo pulsando el icono del **rayo amarillo**.

Este script crea el usuario específico de MediTrack:

```text
Usuario: meditrak_app
Contraseña: MeditrakDemo123!
```

Este usuario solo tiene permisos sobre la base de datos `meditrak_db`.

No uses `root` como usuario final de la aplicación. `root` solo debe utilizarse para ejecutar los scripts iniciales de creación de base de datos y usuario.

MediTrack debe conectarse siempre con:

```text
meditrak_app
```

## 6. Configurar variables de entorno permanentes

MediTrack lee la conexión a MySQL desde variables de entorno. Así evitamos guardar contraseñas dentro del código.

Abre PowerShell y ejecuta estos comandos:

```powershell
setx MEDITRACK_DB_SERVER "localhost"
setx MEDITRACK_DB_PORT "3306"
setx MEDITRACK_DB_USER "meditrak_app"
setx MEDITRACK_DB_PASSWORD "MeditrakDemo123!"
setx MEDITRACK_DB_NAME "meditrak_db"
setx MEDITRACK_DB_INIT "true"
```

Después de ejecutar `setx`, cierra PowerShell y abre una terminal nueva, cierra sesión o reinicia Windows para que las variables se apliquen correctamente.

Las ventanas de PowerShell, Visual Studio o Explorador que ya estaban abiertas antes de ejecutar `setx` pueden no detectar las variables nuevas. Si MediTrack no conecta, cierra la app y vuelve a abrirla desde una ventana o sesión nueva.

Para comprobar las variables desde una terminal nueva, puedes ejecutar:

```powershell
echo $env:MEDITRACK_DB_SERVER
echo $env:MEDITRACK_DB_PORT
echo $env:MEDITRACK_DB_USER
echo $env:MEDITRACK_DB_NAME
echo $env:MEDITRACK_DB_INIT
```

Para comprobar que la contraseña está configurada sin mostrarla en pantalla, usa:

```powershell
if ($env:MEDITRACK_DB_PASSWORD) { "Contraseña configurada" } else { "Contraseña NO configurada" }
```

No escribas la contraseña real en capturas, repositorios o documentación pública.

## 7. Primera ejecución

La primera vez que abras MediTrack después de configurar MySQL:

1. La app leerá las variables de entorno.
2. Se conectará a `meditrak_db` usando `meditrak_app`.
3. Creará o actualizará las tablas necesarias si faltan.
4. Comprobará si la tabla `usuarios` está vacía.
5. Si `usuarios` está vacía, cargará automáticamente la semilla demo.

Este proceso puede tardar unos segundos la primera vez.

## 8. Ejecutar la aplicación

1. Entra en la carpeta:

```text
App/
```

2. Haz doble clic en:

```text
MediTrack.WinForms.exe
```

No hace falta ejecutar la app desde PowerShell.

Importante: no muevas solo `MediTrack.WinForms.exe` al escritorio ni a otra carpeta. La aplicación necesita las `.dll` y archivos que están dentro de la carpeta `App`.

Mantén siempre toda la carpeta `App` completa.

Si quieres un acceso directo, créalo desde el `.exe` original dentro de `App`.

## 9. Semilla demo automática

MediTrack carga datos de demostración automáticamente si la base de datos está vacía.

El comportamiento es:

1. La app conecta con MySQL.
2. Crea o actualiza las tablas si es necesario.
3. Comprueba si la tabla `usuarios` está vacía.
4. Si `usuarios` está vacía, inserta datos demo.
5. Si ya existen usuarios, no duplica la semilla.

Los cambios que hagas después se guardan en el MySQL local del ordenador.

Si borras la base de datos o vacías la tabla `usuarios`, la semilla puede volver a cargarse al arrancar.

## 10. Usuarios demo

Puedes iniciar sesión con estos usuarios:

| Rol | Usuario | Contraseña |
| --- | --- | --- |
| Administrador | `admin` | `Admin123!` |
| Doctor | `doctor` | `Doctor123!` |
| Paciente 1 | `paciente1` | `Paciente123!` |
| Paciente 2 | `paciente2` | `Paciente123!` |

Para una comprobación rápida, usa:

```text
doctor / Doctor123!
```

## 11. Comprobar datos en MySQL Workbench

Para comprobar que la instalación funciona:

1. Abre MySQL Workbench.
2. Conéctate como `root` o como `meditrak_app`.
3. Usa el menú **File > Open SQL Script...**.
4. Selecciona el archivo:

```text
Database/03_test_queries.sql
```

5. Ejecútalo pulsando el icono del **rayo amarillo**.

Este script sirve para comprobar que existen datos en:

- usuarios.
- pacientes.
- doctores.
- enfermedades.
- medicaciones.
- mediciones.
- notas médicas.
- informes.

También incluye consultas para revisar pacientes con su doctor asignado.

## 12. Crear un acceso directo en el escritorio

1. Entra en la carpeta `App`.
2. Haz clic derecho sobre `MediTrack.WinForms.exe`.
3. Selecciona **Enviar a > Escritorio (crear acceso directo)**.
4. Abre MediTrack usando ese acceso directo.

Si el acceso directo deja de funcionar, elimínalo y crea uno nuevo desde `App/MediTrack.WinForms.exe`.

## 13. Errores frecuentes

### Error: Access denied for user root using password NO

Significa que MediTrack no está leyendo las variables de entorno y está intentando conectarse sin contraseña.

Solución:

1. Revisa que ejecutaste los comandos `setx`.
2. Cierra sesión, reinicia Windows o abre una terminal/proceso nuevo.
3. Vuelve a abrir `MediTrack.WinForms.exe`.

### Error: Access denied for user 'meditrak_app'

Puede significar que el usuario de la app no existe, que la contraseña no coincide o que MediTrack no está leyendo las variables correctas.

Solución:

1. Abre MySQL Workbench como `root`.
2. Comprueba que se ejecutó correctamente:

```text
Database/02_create_user.sql
```

3. Si tienes dudas, ejecuta otra vez `Database/02_create_user.sql`.
4. Comprueba que las variables de entorno usan:

```text
MEDITRACK_DB_USER=meditrak_app
MEDITRACK_DB_PASSWORD=MeditrakDemo123!
```

5. Abre una terminal nueva o reinicia sesión para que MediTrack detecte los cambios.

### Error: REFERENCES command denied

Significa que el usuario `meditrak_app` no tiene permisos suficientes para crear o actualizar relaciones entre tablas.

Solución:

1. Abre MySQL Workbench como `root`.
2. Revisa que `Database/02_create_user.sql` incluye el permiso `REFERENCES`.
3. Ejecuta de nuevo `Database/02_create_user.sql`.

### Error: Unknown database 'meditrak_db'

Significa que la base de datos todavía no existe.

Solución:

1. Abre MySQL Workbench como `root`.
2. Ejecuta:

```text
Database/01_create_database.sql
```

3. Después ejecuta `Database/02_create_user.sql` si todavía no lo hiciste.
4. Vuelve a abrir MediTrack.

### Error general: No se pudo iniciar MediTrack

Si aparece un error genérico al abrir la app, revisa el archivo:

```text
startup_error.log
```

Ese archivo suele indicar el motivo real del fallo.

Comprueba especialmente:

1. Que MySQL Server está iniciado.
2. Que las variables de entorno existen y se abrieron desde una sesión nueva.
3. Que `MEDITRACK_DB_USER` es `meditrak_app`.
4. Que `Database/01_create_database.sql` y `Database/02_create_user.sql` se ejecutaron correctamente.
5. Que el usuario `meditrak_app` tiene permisos sobre `meditrak_db`.

### La app no abre

Comprueba:

1. Que MySQL Server está iniciado.
2. Que las variables de entorno están configuradas.
3. Que estás abriendo el `.exe` desde la carpeta `App`.
4. Que tienes instalado .NET Desktop Runtime si la app lo necesita.

### No aparecen datos demo

La semilla demo solo se carga si la tabla `usuarios` está vacía.

Si ya se ejecutó antes, no volverá a duplicar datos.

### El acceso directo no funciona

No muevas solo el `.exe`.

Crea un acceso directo nuevo desde:

```text
App/MediTrack.WinForms.exe
```

## 14. Publicar la app como .exe desde el proyecto

Si necesitas generar de nuevo la carpeta publicable, desde la raíz del proyecto ejecuta:

```powershell
dotnet publish .\MediTrack.WinForms\MediTrack.WinForms.csproj -c Release -r win-x64 --self-contained false
```

El ejecutable queda en una ruta similar a:

```text
MediTrack.WinForms\bin\Release\net8.0-windows\win-x64\publish\MediTrack.WinForms.exe
```

Para la entrega, puedes copiar el contenido de esa carpeta `publish` dentro de una carpeta llamada `App`.

## 15. Resumen rápido

Pasos mínimos para ejecutar MediTrack:

1. Instalar MySQL.
2. Ejecutar `Database/01_create_database.sql`.
3. Ejecutar `Database/02_create_user.sql`.
4. Configurar variables con `setx`.
5. Reiniciar sesión o abrir un proceso nuevo.
6. Abrir `App/MediTrack.WinForms.exe`.
7. Entrar con:

```text
doctor / Doctor123!
```
