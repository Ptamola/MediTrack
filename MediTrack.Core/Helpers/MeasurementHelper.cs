using MediTrack.Core.Constants;
using MediTrack.Core.Enums;

namespace MediTrack.Core.Helpers;

public static class MeasurementHelper
{
    public static string GetUnit(MeasurementType type) =>
        AppConstants.MeasurementUnits.TryGetValue(type, out var unit) ? unit : string.Empty;

    public static string GetDisplayName(MeasurementType type) => type switch
    {
        MeasurementType.Glucosa => "Glucosa",
        MeasurementType.PresionSistolica => "Presión sistólica",
        MeasurementType.PresionDiastolica => "Presión diastólica",
        MeasurementType.Peso => "Peso",
        MeasurementType.FrecuenciaCardiaca => "Frecuencia cardiaca",
        MeasurementType.SaturacionOxigeno => "Saturación de oxígeno",
        MeasurementType.Temperatura => "Temperatura",
        MeasurementType.SintomasPersonalizados => "Síntomas personalizados",
        _ => type.ToString()
    };
}
