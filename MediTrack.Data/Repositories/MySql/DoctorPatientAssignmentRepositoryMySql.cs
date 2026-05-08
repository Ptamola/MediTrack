using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class DoctorPatientAssignmentRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<DoctorPatientAssignment>(connectionFactory), IDoctorPatientAssignmentRepository
{
    protected override string SelectSql => """
        SELECT Id, DoctorId, PacienteId, FechaAsignacion, Activa, FechaFinAsignacion
        FROM doctor_paciente
        ORDER BY FechaAsignacion DESC;
        """;

    protected override string InsertSql => """
        INSERT INTO doctor_paciente (Id, DoctorId, PacienteId, FechaAsignacion, Activa, FechaFinAsignacion)
        VALUES (@Id, @DoctorId, @PacienteId, @FechaAsignacion, @Activa, @FechaFinAsignacion)
        ON DUPLICATE KEY UPDATE
            DoctorId = VALUES(DoctorId),
            PacienteId = VALUES(PacienteId),
            FechaAsignacion = VALUES(FechaAsignacion),
            Activa = VALUES(Activa),
            FechaFinAsignacion = VALUES(FechaFinAsignacion);
        """;

    protected override DoctorPatientAssignment Map(MySqlDataReader reader) => new()
    {
        Id = GetGuid(reader, "Id"),
        DoctorId = GetGuid(reader, "DoctorId"),
        PacienteId = GetGuid(reader, "PacienteId"),
        FechaAsignacion = GetDateTime(reader, "FechaAsignacion"),
        Activa = GetBoolean(reader, "Activa"),
        FechaFinAsignacion = GetNullableDateTime(reader, "FechaFinAsignacion")
    };

    protected override void AddInsertParameters(MySqlCommand command, DoctorPatientAssignment item)
    {
        AddParameter(command, "@Id", item.Id.ToString());
        AddParameter(command, "@DoctorId", item.DoctorId.ToString());
        AddParameter(command, "@PacienteId", item.PacienteId.ToString());
        AddParameter(command, "@FechaAsignacion", item.FechaAsignacion.Date);
        AddParameter(command, "@Activa", item.Activa);
        AddParameter(command, "@FechaFinAsignacion", item.FechaFinAsignacion?.Date);
    }
}
