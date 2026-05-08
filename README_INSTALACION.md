# Instalación de MediTrack en otro ordenador

MediTrack es una aplicación de escritorio en C# Windows Forms que funciona con MySQL local. MySQL es obligatorio: esta versión no tiene modo sin base de datos, no usa JSON como almacenamiento principal y no se conecta a servidores externos.

## 1. Instalar MySQL

1. Descarga MySQL Installer Community desde la web oficial de MySQL.
2. Ejecuta el instalador.
3. Instala como mínimo:
   - MySQL Server.
   - MySQL Workbench.
4. Durante la configuración de MySQL Server:
   - Usa el puerto `3306`.
   - Crea y recuerda la contraseña del usuario `root`.
   - Deja MySQL Server configurado como servicio de Windows para que pueda iniciarse automáticamente.

MySQL debe estar iniciado para que MediTrack funcione.

## 2. Crear base de datos

1. Abre MySQL Workbench.
2. Conéctate como `root`.
3. Abre y ejecuta:

```text
Database/01_create_database.sql
```

Este script crea la base de datos `meditrak_db` con `utf8mb4` para soportar tildes y ñ.

Las tablas no se crean manualmente en este script porque la app ejecuta `DatabaseInitializer` al arrancar y crea o actualiza las tablas necesarias.

## 3. Crear usuario de la aplicación

En MySQL Workbench, conectado como `root`, abre y ejecuta:

```text
Database/02_create_user.sql
```

Esto crea el usuario:

```text
Usuario: meditrak_app
Contraseña: MeditrakDemo123!
```

El usuario solo recibe permisos sobre `meditrak_db`:

```text
SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX
```

La app debe usar `meditrak_app`. No uses `root` como usuario final de MediTrack.

## 4. Configurar variables de entorno permanentes en Windows

Abre PowerShell como usuario normal y ejecuta:

```powershell
[Environment]::SetEnvironmentVariable("MEDITRACK_DB_SERVER", "localhost", "User")
[Environment]::SetEnvironmentVariable("MEDITRACK_DB_PORT", "3306", "User")
[Environment]::SetEnvironmentVariable("MEDITRACK_DB_USER", "meditrak_app", "User")
[Environment]::SetEnvironmentVariable("MEDITRACK_DB_PASSWORD", "MeditrakDemo123!", "User")
[Environment]::SetEnvironmentVariable("MEDITRACK_DB_NAME", "meditrak_db", "User")
[Environment]::SetEnvironmentVariable("MEDITRACK_DB_INIT", "true", "User")
```

Cierra y vuelve a abrir PowerShell o Visual Studio para que detecten las variables nuevas.

## 5. Ejecución temporal desde PowerShell

Si solo quieres probar la app en una sesión de PowerShell, puedes usar variables temporales:

```powershell
$env:MEDITRACK_DB_INIT="true"
$env:MEDITRACK_DB_SERVER="localhost"
$env:MEDITRACK_DB_PORT="3306"
$env:MEDITRACK_DB_USER="meditrak_app"
$env:MEDITRACK_DB_PASSWORD="MeditrakDemo123!"
$env:MEDITRACK_DB_NAME="meditrak_db"

dotnet run --project .\MediTrack.WinForms\MediTrack.WinForms.csproj
```

No guardes contraseñas reales de MySQL en el código ni en Git. Para la entrega demo se usa el usuario `meditrak_app` creado por el script.

## 6. Semilla demo automática

Al arrancar MediTrack:

1. La app crea la base de datos y tablas si faltan.
2. `DataSeeder` comprueba si la tabla `usuarios` está vacía.
3. Si está vacía, carga datos de demostración.
4. Si ya existen usuarios, no duplica datos.

Usuarios demo disponibles:

| Rol | Usuario | Contraseña |
| --- | --- | --- |
| Administrador | `admin` | `Admin123!` |
| Doctor | `doctor` | `Doctor123!` |
| Paciente 1 | `paciente1` | `Paciente123!` |
| Paciente 2 | `paciente2` | `Paciente123!` |

## 7. Comprobar la instalación

En MySQL Workbench puedes ejecutar:

```text
Database/03_test_queries.sql
```

También puedes comprobar manualmente:

```sql
SHOW DATABASES;
USE meditrak_db;
SHOW TABLES;
SELECT NombreUsuario, Rol, Activo FROM usuarios;
```

Deberías ver tablas como:

```text
usuarios
pacientes
doctores
administradores
enfermedades_cronicas
paciente_enfermedad
doctor_paciente
mediciones
medicaciones
notas_medicas
informes
```

## 8. Publicar la app como .exe

Desde la raíz del proyecto, ejecuta:

```powershell
dotnet publish .\MediTrack.WinForms\MediTrack.WinForms.csproj -c Release -r win-x64 --self-contained false
```

El ejecutable queda en una ruta similar a:

```text
MediTrack.WinForms\bin\Release\net8.0-windows\win-x64\publish\MediTrack.WinForms.exe
```

Para abrir la app en otro ordenador:

1. Copia la carpeta `publish` completa.
2. Asegúrate de que MySQL Server está instalado y activo.
3. Configura las variables de entorno.
4. Ejecuta `MediTrack.WinForms.exe`.

## 9. Crear acceso directo en el escritorio

1. Ve a la carpeta `publish`.
2. Haz clic derecho sobre `MediTrack.WinForms.exe`.
3. Elige Enviar a > Escritorio (crear acceso directo).
4. Usa ese acceso directo para abrir MediTrack como una app normal.

## 10. Nota para una versión futura

Esta entrega usa MySQL local para facilitar la demo universitaria. En una versión futura, MediTrack podría usar una base de datos centralizada o global mediante una API segura, manteniendo la aplicación de escritorio como cliente.
