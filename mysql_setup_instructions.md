# Configuracion MySQL para MediTrack

MediTrack todavia usa JSON como almacenamiento principal. Esta configuracion solo activa la creacion automatica de la base de datos MySQL `meditrak_db` y sus tablas para preparar la siguiente fase.

La contrasena de MySQL no debe guardarse en el codigo ni subirse a Git. Debe pasarse mediante variables de entorno antes de ejecutar la app.

## Ejecutar MediTrack con inicializacion MySQL

Abre PowerShell en la raiz del proyecto:

```powershell
cd C:\Users\Admin\Desktop\MediTrack\MediTrack
```

Configura las variables de entorno para esa sesion:

```powershell
$env:MEDITRACK_DB_INIT="true"
$env:MEDITRACK_DB_SERVER="localhost"
$env:MEDITRACK_DB_PORT="3306"
$env:MEDITRACK_DB_USER="root"
$env:MEDITRACK_DB_PASSWORD="TU_CONTRASEÑA"
$env:MEDITRACK_DB_NAME="meditrak_db"

dotnet run --project .\MediTrack.WinForms\MediTrack.WinForms.csproj
```

Cuando `MEDITRACK_DB_INIT` vale `"true"`, MediTrack intenta conectarse a MySQL, crear la base de datos `meditrak_db` si no existe y crear las tablas necesarias si no existen.

Si no quieres inicializar MySQL, no definas `MEDITRACK_DB_INIT` o dejalo con otro valor:

```powershell
$env:MEDITRACK_DB_INIT="false"
dotnet run --project .\MediTrack.WinForms\MediTrack.WinForms.csproj
```

## Comprobar en MySQL Workbench

Ejecuta estas consultas:

```sql
SHOW DATABASES;
USE meditrak_db;
SHOW TABLES;
```

Deberias ver estas tablas:

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

## Notas importantes

- La app sigue usando los repositorios JSON en esta fase.
- La base de datos MySQL se prepara para una migracion posterior.
- La inicializacion usa `utf8mb4` y `utf8mb4_unicode_ci` para soportar tildes y `ñ`.
- Los identificadores `Guid` se guardan como `CHAR(36)`.
- Las sentencias usan `CREATE DATABASE IF NOT EXISTS` y `CREATE TABLE IF NOT EXISTS`, por lo que no borran datos existentes ni duplican tablas.
