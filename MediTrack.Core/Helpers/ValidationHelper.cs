using System.Net.Mail;
using System.Text.RegularExpressions;
using MediTrack.Core.Constants;
using MediTrack.Core.DTOs;
using MediTrack.Core.Enums;
using MediTrack.Core.Models;

namespace MediTrack.Core.Helpers;

public static class ValidationHelper
{
    public static OperationResult ValidateRegistration(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Apellidos) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.NombreUsuario))
        {
            return OperationResult.Fail("Todos los campos son obligatorios.");
        }

        if (request.Edad is < 1 or > 120)
        {
            return OperationResult.Fail("La edad debe estar entre 1 y 120 años.");
        }

        if (!IsValidEmail(request.Email))
        {
            return OperationResult.Fail("El correo electrónico no es válido.");
        }

        if (request.NombreUsuario.Trim().Length < AppConstants.MinimumUsernameLength)
        {
            return OperationResult.Fail($"El nombre de usuario debe tener al menos {AppConstants.MinimumUsernameLength} caracteres.");
        }

        if (request.Password != request.ConfirmarPassword)
        {
            return OperationResult.Fail("La confirmación de contraseña no coincide.");
        }

        if (!IsStrongPassword(request.Password))
        {
            return OperationResult.Fail("La contraseña debe tener mínimo 8 caracteres, una mayúscula, una minúscula y un número.");
        }

        return OperationResult.Ok();
    }

    public static bool IsValidEmail(string email)
    {
        try
        {
            _ = new MailAddress(email);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsStrongPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < AppConstants.MinimumPasswordLength)
        {
            return false;
        }

        return Regex.IsMatch(password, "[A-Z]") &&
               Regex.IsMatch(password, "[a-z]") &&
               Regex.IsMatch(password, "[0-9]");
    }

    public static OperationResult ValidateMedication(Medication medication)
    {
        if (string.IsNullOrWhiteSpace(medication.Nombre) ||
            string.IsNullOrWhiteSpace(medication.Dosis) ||
            string.IsNullOrWhiteSpace(medication.Frecuencia) ||
            string.IsNullOrWhiteSpace(medication.Horario))
        {
            return OperationResult.Fail("Nombre, dosis, frecuencia y horario son obligatorios.");
        }

        if (medication.FechaFin.HasValue && medication.FechaFin.Value.Date < medication.FechaInicio.Date)
        {
            return OperationResult.Fail("La fecha fin no puede ser anterior a la fecha de inicio.");
        }

        return OperationResult.Ok();
    }

    public static OperationResult ValidateMeasurement(Measurement measurement)
    {
        if (measurement.PacienteId == Guid.Empty)
        {
            return OperationResult.Fail("Debe seleccionarse un paciente.");
        }

        if (measurement.TipoMedicion == MeasurementType.SintomasPersonalizados)
        {
            if (string.IsNullOrWhiteSpace(measurement.Observaciones))
            {
                return OperationResult.Fail("Debe indicar los síntomas observados.");
            }

            return OperationResult.Ok();
        }

        if (measurement.Valor <= 0)
        {
            return OperationResult.Fail("El valor numérico debe ser mayor que cero.");
        }

        var isReasonable = measurement.TipoMedicion switch
        {
            MeasurementType.Glucosa => measurement.Valor is >= 30 and <= 600,
            MeasurementType.PresionSistolica => measurement.Valor is >= 50 and <= 250,
            MeasurementType.PresionDiastolica => measurement.Valor is >= 30 and <= 160,
            MeasurementType.Peso => measurement.Valor is >= 1 and <= 500,
            MeasurementType.FrecuenciaCardiaca => measurement.Valor is >= 20 and <= 250,
            MeasurementType.SaturacionOxigeno => measurement.Valor is >= 40 and <= 100,
            MeasurementType.Temperatura => measurement.Valor is >= 30 and <= 45,
            _ => true
        };

        return isReasonable
            ? OperationResult.Ok()
            : OperationResult.Fail("El valor introducido está fuera de un rango razonable para ese tipo de medición.");
    }
}
