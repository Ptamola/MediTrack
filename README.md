# MediTrack

MediTrack es una aplicacion de escritorio en C# con Windows Forms para gestionar pacientes con enfermedades cronicas. Esta version usa archivos JSON locales como persistencia y esta organizada en 3 capas para poder migrar a MySQL mas adelante sin rehacer la app.

## Solucion

- [MediTrack.slnx](/C:/Users/Admin/Desktop/MediTrack/MediTrack/MediTrack.slnx)
- Proyecto de inicio recomendado:
  [MediTrack.WinForms.csproj](/C:/Users/Admin/Desktop/MediTrack/MediTrack/MediTrack.WinForms/MediTrack.WinForms.csproj)

## Estructura

- `MediTrack.WinForms`: interfaz, formularios, navegacion, tema visual y bootstrap.
- `MediTrack.Core`: modelos, DTOs, helpers, servicios, validaciones e informes.
- `MediTrack.Data`: repositorios JSON, rutas de almacenamiento y datos semilla.

## Funcionalidad incluida

- Login y registro publico de pacientes.
- Roles de paciente, doctor y administrador.
- Dashboards separados por rol.
- Gestion de perfil del paciente.
- Gestion de enfermedades cronicas.
- Gestion de medicacion con recordatorios.
- Registro e historial de mediciones.
- Notas medicas visibles para paciente y doctor.
- Gestion de usuarios y asignaciones doctor-paciente.
- Informes con vista previa dentro de la app.
- Exportacion a PDF.
- Datos semilla automaticos al primer arranque.

## Credenciales de prueba

- Administrador
  - Usuario: `admin`
  - Contrasena: `Admin123`
- Doctor
  - Usuario: `doctor.carlos`
  - Contrasena: `Doctor123`
- Doctora
  - Usuario: `doctora.elena`
  - Contrasena: `Doctor123`
- Paciente
  - Usuario: `ana.gomez`
  - Contrasena: `Paciente123`
- Paciente
  - Usuario: `miguel.perez`
  - Contrasena: `Paciente123`

## Requisitos

- Windows
- Visual Studio 2022 o superior
- SDK de .NET 8

## Paquetes NuGet

- `QuestPDF`

## Como abrirlo en Visual Studio

1. Abre Visual Studio.
2. Pulsa `Abrir un proyecto o una solucion`.
3. Selecciona [MediTrack.slnx](/C:/Users/Admin/Desktop/MediTrack/MediTrack/MediTrack.slnx).
4. En el Explorador de soluciones, haz clic derecho sobre [MediTrack.WinForms.csproj](/C:/Users/Admin/Desktop/MediTrack/MediTrack/MediTrack.WinForms/MediTrack.WinForms.csproj).
5. Elige `Establecer como proyecto de inicio`.
6. Pulsa `F5` o el boton verde `Iniciar`.

## Como ejecutarlo por terminal

Desde la carpeta del repositorio:

```powershell
dotnet restore .\MediTrack.slnx --configfile .\NuGet.Config
dotnet run --project .\MediTrack.WinForms\MediTrack.WinForms.csproj
```

## Donde guarda los datos

Al ejecutarse, la app crea automaticamente una carpeta `Data` junto al ejecutable de WinForms. Ahi se guardan:

- `usuarios.json`
- `pacientes.json`
- `doctores.json`
- `administradores.json`
- `enfermedades.json`
- `paciente_enfermedades.json`
- `medicamentos.json`
- `mediciones.json`
- `notas_medicas.json`
- `informes.json`
- `asignaciones.json`

## Flujo rapido de prueba

1. Entra como `admin`.
2. Revisa `Usuarios` y `Asignaciones`.
3. Cierra sesion y entra como `doctor.carlos`.
4. Abre `Mis pacientes` y selecciona un paciente.
5. Ve a `Notas medicas`, `Mediciones` o `Informes`.
6. Cierra sesion y entra como `ana.gomez`.
7. Revisa su dashboard, medicacion, mediciones e informes.

## Validaciones ya implementadas

- Email valido.
- Nombre de usuario obligatorio y con longitud minima.
- Contrasena con minimo 8 caracteres, mayuscula, minuscula y numero.
- Confirmacion de contrasena en registro.
- No duplicados de usuario o email.
- Validacion basica de fechas en medicacion.
- Rangos razonables para mediciones.

## Migracion futura a MySQL

La logica de negocio no depende de JSON directamente. La separacion esta preparada asi:

1. Las interfaces de repositorio viven en `MediTrack.Core/Interfaces/Repositories`.
2. Las implementaciones JSON viven en `MediTrack.Data/Repositories`.
3. El cableado actual se hace en [AppBootstrapper.cs](/C:/Users/Admin/Desktop/MediTrack/MediTrack/MediTrack.WinForms/Config/AppBootstrapper.cs).

Para migrar a MySQL despues:

1. Crea nuevas implementaciones de repositorio MySQL.
2. Sustituye en `AppBootstrapper` los repositorios JSON por los repositorios MySQL.
3. Mantiene formularios, servicios y modelos con cambios minimos.

## Estado actual

- La solucion compila correctamente.
- Los servicios estan conectados con los formularios.
- Los repositorios JSON funcionan y persisten cambios.
- Los datos semilla se generan automaticamente.
