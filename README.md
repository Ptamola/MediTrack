# MediTrack

MediTrack es una aplicación de escritorio para Windows desarrollada en C# con Windows Forms. Está pensada como una demo académico-profesional para la gestión de pacientes con enfermedades crónicas.

La aplicación usa MySQL como base de datos obligatoria. No usa tecnologías web ni funciona en modo sin base de datos.

## Tecnologías

- C#.
- Windows Forms.
- .NET 8.
- MySQL.
- MySqlConnector.
- QuestPDF para exportación de informes.

## Roles principales

- Administrador: gestiona usuarios, asignaciones doctor-paciente e informes.
- Doctor: consulta pacientes asignados, enfermedades, medicación, mediciones, notas médicas e informes.
- Paciente: consulta su perfil, enfermedades, medicación pautada, mediciones, notas visibles e informes.

## Base de datos y semilla demo

MediTrack usa una base de datos local llamada `meditrak_db`.

Al arrancar, la aplicación puede crear la base de datos y las tablas necesarias mediante `DatabaseInitializer`. Después, `DataSeeder` comprueba si la tabla `usuarios` está vacía. Si no hay usuarios, carga automáticamente una semilla demo con usuarios, pacientes, doctor, enfermedades, medicación, mediciones, notas e informes.

La semilla es idempotente: si ya existen usuarios, no duplica datos ni borra información existente.

## Documentación

- Guía completa de instalación: [README_INSTALACION.md](/C:/Users/Admin/Desktop/MediTrack/MediTrack/README_INSTALACION.md).
- Guía rápida de demo y usuarios: [README_DEMO.md](/C:/Users/Admin/Desktop/MediTrack/MediTrack/README_DEMO.md).
- Scripts SQL auxiliares: [Database](/C:/Users/Admin/Desktop/MediTrack/MediTrack/Database).

## Ejecución rápida para desarrollo

Configura las variables de entorno de MySQL antes de ejecutar la app:

```powershell
$env:MEDITRACK_DB_INIT="true"
$env:MEDITRACK_DB_SERVER="localhost"
$env:MEDITRACK_DB_PORT="3306"
$env:MEDITRACK_DB_USER="meditrak_app"
$env:MEDITRACK_DB_PASSWORD="MeditrakDemo123!"
$env:MEDITRACK_DB_NAME="meditrak_db"

dotnet run --project .\MediTrack.WinForms\MediTrack.WinForms.csproj
```

Para una instalación completa en otro ordenador, sigue [README_INSTALACION.md](/C:/Users/Admin/Desktop/MediTrack/MediTrack/README_INSTALACION.md).

## Usuarios demo

| Rol | Usuario | Contraseña |
| --- | --- | --- |
| Administrador | `admin` | `Admin123!` |
| Doctor | `doctor` | `Doctor123!` |
| Paciente 1 | `paciente1` | `Paciente123!` |
| Paciente 2 | `paciente2` | `Paciente123!` |

## Estado actual

- Login por roles.
- MySQL como almacenamiento principal.
- Semilla demo automática.
- Perfil ampliado de paciente con foto.
- Enfermedades libres, activas y superadas.
- Medicación, mediciones, notas médicas e informes.
- Exportación a PDF.
- Scripts y documentación de instalación.
