using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class MedicalNoteRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<MedicalNote>(connectionFactory), IMedicalNoteRepository
{
    protected override string SelectSql => """
        SELECT Id, PacienteId, DoctorId, Titulo, Contenido, FechaHora, VisibleParaPaciente
        FROM notas_medicas
        ORDER BY FechaHora DESC;
        """;

    protected override string InsertSql => """
        INSERT INTO notas_medicas (Id, PacienteId, DoctorId, Titulo, Contenido, FechaHora, VisibleParaPaciente)
        VALUES (@Id, @PacienteId, @DoctorId, @Titulo, @Contenido, @FechaHora, @VisibleParaPaciente)
        ON DUPLICATE KEY UPDATE
            PacienteId = VALUES(PacienteId),
            DoctorId = VALUES(DoctorId),
            Titulo = VALUES(Titulo),
            Contenido = VALUES(Contenido),
            FechaHora = VALUES(FechaHora),
            VisibleParaPaciente = VALUES(VisibleParaPaciente);
        """;

    protected override MedicalNote Map(MySqlDataReader reader) => new()
    {
        Id = GetGuid(reader, "Id"),
        PacienteId = GetGuid(reader, "PacienteId"),
        DoctorId = GetGuid(reader, "DoctorId"),
        Titulo = GetString(reader, "Titulo"),
        Contenido = GetString(reader, "Contenido"),
        FechaHora = GetDateTime(reader, "FechaHora"),
        VisibleParaPaciente = GetBoolean(reader, "VisibleParaPaciente")
    };

    protected override void AddInsertParameters(MySqlCommand command, MedicalNote item)
    {
        AddParameter(command, "@Id", item.Id.ToString());
        AddParameter(command, "@PacienteId", item.PacienteId.ToString());
        AddParameter(command, "@DoctorId", item.DoctorId.ToString());
        AddParameter(command, "@Titulo", item.Titulo);
        AddParameter(command, "@Contenido", item.Contenido);
        AddParameter(command, "@FechaHora", item.FechaHora);
        AddParameter(command, "@VisibleParaPaciente", item.VisibleParaPaciente);
    }
}
