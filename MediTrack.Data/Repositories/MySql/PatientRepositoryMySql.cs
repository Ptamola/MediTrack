using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Models;
using MediTrack.Data.Config;
using MySqlConnector;

namespace MediTrack.Data.Repositories.MySql;

public class PatientRepositoryMySql(DatabaseConnectionFactory connectionFactory)
    : MySqlRepositoryBase<Patient>(connectionFactory), IPatientRepository
{
    protected override string SelectSql => """
        SELECT IdUsuario, FechaNacimiento, Telefono, Direccion, ObservacionesGenerales,
               DniNie, Sexo, GrupoSanguineo, AlturaCm, PesoKg, Alergias,
               AntecedentesMedicos, ContactoEmergenciaNombre, ContactoEmergenciaTelefono,
               SeguroMedico, NumeroTarjetaSanitaria, FotoRuta
        FROM pacientes;
        """;

    protected override string InsertSql => """
        INSERT INTO pacientes (
            IdUsuario, FechaNacimiento, Telefono, Direccion, ObservacionesGenerales,
            DniNie, Sexo, GrupoSanguineo, AlturaCm, PesoKg, Alergias,
            AntecedentesMedicos, ContactoEmergenciaNombre, ContactoEmergenciaTelefono,
            SeguroMedico, NumeroTarjetaSanitaria, FotoRuta)
        VALUES (
            @IdUsuario, @FechaNacimiento, @Telefono, @Direccion, @ObservacionesGenerales,
            @DniNie, @Sexo, @GrupoSanguineo, @AlturaCm, @PesoKg, @Alergias,
            @AntecedentesMedicos, @ContactoEmergenciaNombre, @ContactoEmergenciaTelefono,
            @SeguroMedico, @NumeroTarjetaSanitaria, @FotoRuta)
        ON DUPLICATE KEY UPDATE
            FechaNacimiento = VALUES(FechaNacimiento),
            Telefono = VALUES(Telefono),
            Direccion = VALUES(Direccion),
            ObservacionesGenerales = VALUES(ObservacionesGenerales),
            DniNie = VALUES(DniNie),
            Sexo = VALUES(Sexo),
            GrupoSanguineo = VALUES(GrupoSanguineo),
            AlturaCm = VALUES(AlturaCm),
            PesoKg = VALUES(PesoKg),
            Alergias = VALUES(Alergias),
            AntecedentesMedicos = VALUES(AntecedentesMedicos),
            ContactoEmergenciaNombre = VALUES(ContactoEmergenciaNombre),
            ContactoEmergenciaTelefono = VALUES(ContactoEmergenciaTelefono),
            SeguroMedico = VALUES(SeguroMedico),
            NumeroTarjetaSanitaria = VALUES(NumeroTarjetaSanitaria),
            FotoRuta = VALUES(FotoRuta);
        """;

    protected override Patient Map(MySqlDataReader reader) => new()
    {
        IdUsuario = GetGuid(reader, "IdUsuario"),
        FechaNacimiento = GetDateTime(reader, "FechaNacimiento"),
        Telefono = GetString(reader, "Telefono"),
        Direccion = GetString(reader, "Direccion"),
        ObservacionesGenerales = GetString(reader, "ObservacionesGenerales"),
        DniNie = GetString(reader, "DniNie"),
        Sexo = GetString(reader, "Sexo"),
        GrupoSanguineo = GetString(reader, "GrupoSanguineo"),
        AlturaCm = GetNullableDecimal(reader, "AlturaCm"),
        PesoKg = GetNullableDecimal(reader, "PesoKg"),
        Alergias = GetString(reader, "Alergias"),
        AntecedentesMedicos = GetString(reader, "AntecedentesMedicos"),
        ContactoEmergenciaNombre = GetString(reader, "ContactoEmergenciaNombre"),
        ContactoEmergenciaTelefono = GetString(reader, "ContactoEmergenciaTelefono"),
        SeguroMedico = GetString(reader, "SeguroMedico"),
        NumeroTarjetaSanitaria = GetString(reader, "NumeroTarjetaSanitaria"),
        FotoRuta = GetString(reader, "FotoRuta")
    };

    protected override void AddInsertParameters(MySqlCommand command, Patient item)
    {
        AddParameter(command, "@IdUsuario", item.IdUsuario.ToString());
        AddParameter(command, "@FechaNacimiento", item.FechaNacimiento.Date);
        AddParameter(command, "@Telefono", item.Telefono);
        AddParameter(command, "@Direccion", item.Direccion);
        AddParameter(command, "@ObservacionesGenerales", item.ObservacionesGenerales);
        AddParameter(command, "@DniNie", item.DniNie);
        AddParameter(command, "@Sexo", item.Sexo);
        AddParameter(command, "@GrupoSanguineo", item.GrupoSanguineo);
        AddParameter(command, "@AlturaCm", item.AlturaCm > 0 ? item.AlturaCm : null);
        AddParameter(command, "@PesoKg", item.PesoKg > 0 ? item.PesoKg : null);
        AddParameter(command, "@Alergias", item.Alergias);
        AddParameter(command, "@AntecedentesMedicos", item.AntecedentesMedicos);
        AddParameter(command, "@ContactoEmergenciaNombre", item.ContactoEmergenciaNombre);
        AddParameter(command, "@ContactoEmergenciaTelefono", item.ContactoEmergenciaTelefono);
        AddParameter(command, "@SeguroMedico", item.SeguroMedico);
        AddParameter(command, "@NumeroTarjetaSanitaria", item.NumeroTarjetaSanitaria);
        AddParameter(command, "@FotoRuta", item.FotoRuta);
    }
}
