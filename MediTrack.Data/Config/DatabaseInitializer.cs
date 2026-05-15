using MySqlConnector;

namespace MediTrack.Data.Config;

/// <summary>
/// Crea y actualiza la estructura minima de MySQL que necesita MediTrack.
/// Las operaciones son idempotentes para no borrar ni duplicar datos existentes.
/// </summary>
public class DatabaseInitializer
{
    private readonly DatabaseSettings _settings;
    private readonly DatabaseConnectionFactory _connectionFactory;

    public DatabaseInitializer(DatabaseSettings settings, DatabaseConnectionFactory connectionFactory)
    {
        _settings = settings;
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Garantiza que la base de datos y las tablas existan antes de construir los servicios.
    /// </summary>
    public async Task InitializeAsync()
    {
        await CreateDatabaseAsync();
        await CreateTablesAsync();
    }

    private async Task CreateDatabaseAsync()
    {
        await using var connection = _connectionFactory.CreateServerConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        var databaseName = EscapeIdentifier(_settings.DatabaseName);
        command.CommandText = $"""
            CREATE DATABASE IF NOT EXISTS `{databaseName}`
            CHARACTER SET utf8mb4
            COLLATE utf8mb4_unicode_ci;
            """;
        await command.ExecuteNonQueryAsync();
    }

    private async Task CreateTablesAsync()
    {
        await using var connection = _connectionFactory.CreateDatabaseConnection();
        await connection.OpenAsync();

        foreach (var sql in GetCreateTableStatements())
        {
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }

        await EnsurePatientProfileColumnsAsync(connection);
        await EnsurePatientDiseaseStatusColumnsAsync(connection);
    }

    /// <summary>
    /// Define las tablas principales del modelo clinico con CHAR(36) para Guid y utf8mb4 para tildes y ene.
    /// </summary>
    private static IEnumerable<string> GetCreateTableStatements()
    {
        yield return """
            CREATE TABLE IF NOT EXISTS usuarios (
                Id CHAR(36) NOT NULL,
                Nombre VARCHAR(100) NOT NULL,
                Apellidos VARCHAR(150) NOT NULL,
                Email VARCHAR(200) NOT NULL,
                NombreUsuario VARCHAR(100) NOT NULL,
                PasswordHash VARCHAR(500) NOT NULL,
                Rol INT NOT NULL,
                Activo TINYINT(1) NOT NULL DEFAULT 1,
                FechaCreacion DATETIME NOT NULL,
                PRIMARY KEY (Id),
                UNIQUE KEY uq_usuarios_email (Email),
                UNIQUE KEY uq_usuarios_nombre_usuario (NombreUsuario)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS pacientes (
                IdUsuario CHAR(36) NOT NULL,
                FechaNacimiento DATE NOT NULL,
                Telefono VARCHAR(30) NULL,
                Direccion VARCHAR(300) NULL,
                ObservacionesGenerales TEXT NULL,
                DniNie VARCHAR(30) NULL,
                Sexo VARCHAR(30) NULL,
                GrupoSanguineo VARCHAR(10) NULL,
                AlturaCm DECIMAL(5,2) NULL,
                PesoKg DECIMAL(5,2) NULL,
                Alergias TEXT NULL,
                AntecedentesMedicos TEXT NULL,
                ContactoEmergenciaNombre VARCHAR(150) NULL,
                ContactoEmergenciaTelefono VARCHAR(30) NULL,
                SeguroMedico VARCHAR(120) NULL,
                NumeroTarjetaSanitaria VARCHAR(80) NULL,
                FotoRuta VARCHAR(300) NULL,
                PRIMARY KEY (IdUsuario),
                CONSTRAINT fk_pacientes_usuarios
                    FOREIGN KEY (IdUsuario) REFERENCES usuarios(Id)
                    ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS doctores (
                IdUsuario CHAR(36) NOT NULL,
                NumeroColegiado VARCHAR(80) NULL,
                Especialidad VARCHAR(120) NULL,
                PRIMARY KEY (IdUsuario),
                CONSTRAINT fk_doctores_usuarios
                    FOREIGN KEY (IdUsuario) REFERENCES usuarios(Id)
                    ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS administradores (
                IdUsuario CHAR(36) NOT NULL,
                PRIMARY KEY (IdUsuario),
                CONSTRAINT fk_administradores_usuarios
                    FOREIGN KEY (IdUsuario) REFERENCES usuarios(Id)
                    ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS enfermedades_cronicas (
                Id CHAR(36) NOT NULL,
                Nombre VARCHAR(150) NOT NULL,
                Descripcion TEXT NULL,
                PRIMARY KEY (Id),
                UNIQUE KEY uq_enfermedades_nombre (Nombre)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS paciente_enfermedad (
                Id CHAR(36) NOT NULL,
                PacienteId CHAR(36) NOT NULL,
                EnfermedadId CHAR(36) NOT NULL,
                FechaDiagnostico DATE NOT NULL,
                FechaFin DATE NULL,
                Activa TINYINT(1) NOT NULL DEFAULT 1,
                Observaciones TEXT NULL,
                PRIMARY KEY (Id),
                KEY ix_paciente_enfermedad_paciente (PacienteId),
                KEY ix_paciente_enfermedad_enfermedad (EnfermedadId),
                CONSTRAINT fk_paciente_enfermedad_paciente
                    FOREIGN KEY (PacienteId) REFERENCES pacientes(IdUsuario)
                    ON DELETE CASCADE,
                CONSTRAINT fk_paciente_enfermedad_enfermedad
                    FOREIGN KEY (EnfermedadId) REFERENCES enfermedades_cronicas(Id)
                    ON DELETE RESTRICT
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS doctor_paciente (
                Id CHAR(36) NOT NULL,
                DoctorId CHAR(36) NOT NULL,
                PacienteId CHAR(36) NOT NULL,
                FechaAsignacion DATE NOT NULL,
                Activa TINYINT(1) NOT NULL DEFAULT 1,
                FechaFinAsignacion DATE NULL,
                PRIMARY KEY (Id),
                KEY ix_doctor_paciente_doctor (DoctorId),
                KEY ix_doctor_paciente_paciente (PacienteId),
                CONSTRAINT fk_doctor_paciente_doctor
                    FOREIGN KEY (DoctorId) REFERENCES doctores(IdUsuario)
                    ON DELETE CASCADE,
                CONSTRAINT fk_doctor_paciente_paciente
                    FOREIGN KEY (PacienteId) REFERENCES pacientes(IdUsuario)
                    ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS mediciones (
                Id CHAR(36) NOT NULL,
                PacienteId CHAR(36) NOT NULL,
                TipoMedicion INT NOT NULL,
                Valor DECIMAL(10,2) NOT NULL,
                Unidad VARCHAR(30) NOT NULL,
                FechaHora DATETIME NOT NULL,
                Observaciones TEXT NULL,
                PRIMARY KEY (Id),
                KEY ix_mediciones_paciente_fecha (PacienteId, FechaHora),
                CONSTRAINT fk_mediciones_paciente
                    FOREIGN KEY (PacienteId) REFERENCES pacientes(IdUsuario)
                    ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS medicaciones (
                Id CHAR(36) NOT NULL,
                PacienteId CHAR(36) NOT NULL,
                Nombre VARCHAR(150) NOT NULL,
                Dosis VARCHAR(100) NOT NULL,
                Frecuencia VARCHAR(100) NOT NULL,
                Horario VARCHAR(100) NULL,
                FechaInicio DATE NOT NULL,
                FechaFin DATE NULL,
                Observaciones TEXT NULL,
                Activo TINYINT(1) NOT NULL DEFAULT 1,
                PRIMARY KEY (Id),
                KEY ix_medicaciones_paciente (PacienteId),
                CONSTRAINT fk_medicaciones_paciente
                    FOREIGN KEY (PacienteId) REFERENCES pacientes(IdUsuario)
                    ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS notas_medicas (
                Id CHAR(36) NOT NULL,
                PacienteId CHAR(36) NOT NULL,
                DoctorId CHAR(36) NOT NULL,
                Titulo VARCHAR(180) NOT NULL,
                Contenido TEXT NOT NULL,
                FechaHora DATETIME NOT NULL,
                VisibleParaPaciente TINYINT(1) NOT NULL DEFAULT 1,
                PRIMARY KEY (Id),
                KEY ix_notas_paciente_fecha (PacienteId, FechaHora),
                KEY ix_notas_doctor (DoctorId),
                CONSTRAINT fk_notas_paciente
                    FOREIGN KEY (PacienteId) REFERENCES pacientes(IdUsuario)
                    ON DELETE CASCADE,
                CONSTRAINT fk_notas_doctor
                    FOREIGN KEY (DoctorId) REFERENCES doctores(IdUsuario)
                    ON DELETE CASCADE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;

        yield return """
            CREATE TABLE IF NOT EXISTS informes (
                Id CHAR(36) NOT NULL,
                PacienteId CHAR(36) NOT NULL,
                GeneradoPorUsuarioId CHAR(36) NOT NULL,
                FechaGeneracion DATETIME NOT NULL,
                FechaInicioPeriodo DATE NOT NULL,
                FechaFinPeriodo DATE NOT NULL,
                Resumen TEXT NOT NULL,
                RutaPdf VARCHAR(500) NULL,
                PRIMARY KEY (Id),
                KEY ix_informes_paciente_fecha (PacienteId, FechaGeneracion),
                KEY ix_informes_generado_por (GeneradoPorUsuarioId),
                CONSTRAINT fk_informes_paciente
                    FOREIGN KEY (PacienteId) REFERENCES pacientes(IdUsuario)
                    ON DELETE CASCADE,
                CONSTRAINT fk_informes_usuario
                    FOREIGN KEY (GeneradoPorUsuarioId) REFERENCES usuarios(Id)
                    ON DELETE RESTRICT
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
            """;
    }

    private static string EscapeIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return DatabaseSettings.DefaultDatabaseName;
        }

        return identifier.Replace("`", "``");
    }

    private async Task EnsurePatientProfileColumnsAsync(MySqlConnection connection)
    {
        // Las columnas se anaden de forma segura para poder actualizar bases existentes sin perder datos.
        var columns = new (string Name, string Definition)[]
        {
            ("DniNie", "VARCHAR(30) NULL"),
            ("Sexo", "VARCHAR(30) NULL"),
            ("GrupoSanguineo", "VARCHAR(10) NULL"),
            ("AlturaCm", "DECIMAL(5,2) NULL"),
            ("PesoKg", "DECIMAL(5,2) NULL"),
            ("Alergias", "TEXT NULL"),
            ("AntecedentesMedicos", "TEXT NULL"),
            ("ContactoEmergenciaNombre", "VARCHAR(150) NULL"),
            ("ContactoEmergenciaTelefono", "VARCHAR(30) NULL"),
            ("SeguroMedico", "VARCHAR(120) NULL"),
            ("NumeroTarjetaSanitaria", "VARCHAR(80) NULL"),
            ("FotoRuta", "VARCHAR(300) NULL")
        };

        foreach (var column in columns)
        {
            if (await ColumnExistsAsync(connection, "pacientes", column.Name))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.CommandText = $"ALTER TABLE pacientes ADD COLUMN `{column.Name}` {column.Definition};";
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task<bool> ColumnExistsAsync(MySqlConnection connection, string tableName, string columnName)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*)
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @SchemaName
              AND TABLE_NAME = @TableName
              AND COLUMN_NAME = @ColumnName;
            """;
        command.Parameters.AddWithValue("@SchemaName", _settings.DatabaseName);
        command.Parameters.AddWithValue("@TableName", tableName);
        command.Parameters.AddWithValue("@ColumnName", columnName);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    private async Task EnsurePatientDiseaseStatusColumnsAsync(MySqlConnection connection)
    {
        // Activa y FechaFin permiten conservar el historial sin borrar enfermedades superadas.
        var columns = new (string Name, string Definition)[]
        {
            ("FechaFin", "DATE NULL"),
            ("Activa", "TINYINT(1) NOT NULL DEFAULT 1")
        };

        foreach (var column in columns)
        {
            if (await ColumnExistsAsync(connection, "paciente_enfermedad", column.Name))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.CommandText = $"ALTER TABLE paciente_enfermedad ADD COLUMN `{column.Name}` {column.Definition};";
            await command.ExecuteNonQueryAsync();
        }
    }
}
