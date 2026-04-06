using MediTrack.Core.Enums;

namespace MediTrack.Core.Constants;

public static class AppConstants
{
    public const string ApplicationName = "MediTrack";
    public const string ReportsFolderName = "Reportes";
    public const string DefaultDoctorSpecialty = "Medicina Interna";
    public const int MinimumUsernameLength = 4;
    public const int MinimumPasswordLength = 8;

    public static readonly IReadOnlyDictionary<MeasurementType, string> MeasurementUnits =
        new Dictionary<MeasurementType, string>
        {
            [MeasurementType.Glucosa] = "mg/dL",
            [MeasurementType.PresionSistolica] = "mmHg",
            [MeasurementType.PresionDiastolica] = "mmHg",
            [MeasurementType.Peso] = "kg",
            [MeasurementType.FrecuenciaCardiaca] = "lpm",
            [MeasurementType.SaturacionOxigeno] = "%",
            [MeasurementType.Temperatura] = "°C",
            [MeasurementType.SintomasPersonalizados] = "texto"
        };

    public static readonly string[] BaseDiseases =
    [
        "Diabetes tipo 2",
        "Hipertensión arterial",
        "Asma",
        "Enfermedad renal crónica"
    ];
}
