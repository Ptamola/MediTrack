using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class PatientDiseaseRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<PatientDisease>(connectionFactory), IPatientDiseaseRepository
{
    protected override string SelectSql => """
        SELECT Id, PacienteId, EnfermedadId, FechaDiagnostico, Observaciones
        FROM paciente_enfermedad
        ORDER BY FechaDiagnostico DESC;
        """;

    protected override string InsertSql => """
        INSERT INTO paciente_enfermedad (Id, PacienteId, EnfermedadId, FechaDiagnostico, Observaciones)
        VALUES (@Id, @PacienteId, @EnfermedadId, @FechaDiagnostico, @Observaciones)
        ON DUPLICATE KEY UPDATE
            PacienteId = VALUES(PacienteId),
            EnfermedadId = VALUES(EnfermedadId),
            FechaDiagnostico = VALUES(FechaDiagnostico),
            Observaciones = VALUES(Observaciones);
        """;

    protected override PatientDisease Map(MySqlDataReader reader) => new()
    {
        Id = GetGuid(reader, "Id"),
        PacienteId = GetGuid(reader, "PacienteId"),
        EnfermedadId = GetGuid(reader, "EnfermedadId"),
        FechaDiagnostico = GetDateTime(reader, "FechaDiagnostico"),
        Observaciones = GetString(reader, "Observaciones")
    };

    protected override void AddInsertParameters(MySqlCommand command, PatientDisease item)
    {
        AddParameter(command, "@Id", item.Id.ToString());
        AddParameter(command, "@PacienteId", item.PacienteId.ToString());
        AddParameter(command, "@EnfermedadId", item.EnfermedadId.ToString());
        AddParameter(command, "@FechaDiagnostico", item.FechaDiagnostico.Date);
        AddParameter(command, "@Observaciones", item.Observaciones);
    }
}
