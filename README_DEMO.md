# MediTrack - Demo local

MediTrack es una demo académica local desarrollada en C# con Windows Forms y MySQL. Esta versión no usa servidor externo, API ni base de datos global compartida.

## Semilla automática

Al arrancar la aplicación, `DatabaseInitializer` crea la base de datos y las tablas si no existen. Después, `DataSeeder` comprueba la tabla `usuarios`.

Si `usuarios` está vacía, se cargan datos de demostración:

- Usuarios por rol.
- Perfiles de pacientes.
- Enfermedades crónicas.
- Enfermedades activas y una enfermedad superada/inactiva.
- Medicación.
- Mediciones clínicas.
- Notas médicas.
- Informes clínicos iniciales.

Si ya existen usuarios, la semilla no se ejecuta y no duplica datos.

## Usuarios demo

| Rol | Usuario | Contraseña |
| --- | --- | --- |
| Administrador | `admin` | `Admin123!` |
| Doctor | `doctor` | `Doctor123!` |
| Paciente 1 | `paciente1` | `Paciente123!` |
| Paciente 2 | `paciente2` | `Paciente123!` |

## Requisitos rápidos

Para ejecutar la demo necesitas tener MySQL Server instalado y activo en tu equipo.

La configuración recomendada usa el usuario de aplicación:

```powershell
$env:MEDITRACK_DB_INIT="true"
$env:MEDITRACK_DB_SERVER="localhost"
$env:MEDITRACK_DB_PORT="3306"
$env:MEDITRACK_DB_USER="meditrak_app"
$env:MEDITRACK_DB_PASSWORD="MeditrakDemo123!"
$env:MEDITRACK_DB_NAME="meditrak_db"

dotnet run --project .\MediTrack.WinForms\MediTrack.WinForms.csproj
```

Para una instalación completa, consulta [README_INSTALACION.md](/C:/Users/Admin/Desktop/MediTrack/MediTrack/README_INSTALACION.md).

## Comprobación rápida en MySQL Workbench

```sql
SHOW DATABASES;
USE meditrak_db;
SHOW TABLES;
SELECT NombreUsuario, Rol, Activo FROM usuarios;
```

## Evolución futura

En una versión posterior, MediTrack podría conectarse a una base de datos centralizada mediante una API segura. Esta demo mantiene todo en local para facilitar instalación, presentación y evaluación universitaria.
