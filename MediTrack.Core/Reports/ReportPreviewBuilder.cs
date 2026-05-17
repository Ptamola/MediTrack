using System.Text;
using MediTrack.Core.DTOs;
using MediTrack.Core.Helpers;

namespace MediTrack.Core.Reports;

public static class ReportPreviewBuilder
{
    public static string Build(GeneratedReportResult data)
    {
        var sb = new StringBuilder();
        var pacienteNombre = data.PacienteUsuario?.NombreCompleto ?? "Paciente no disponible";
        var doctorNombre = data.DoctorAsignado?.NombreCompleto ?? "Sin doctor asignado";
        var historialDoctores = data.DoctoresAnteriores.Count > 0
            ? string.Join(", ", data.DoctoresAnteriores.Select(d => d.NombreCompleto))
            : "No hay doctores previos registrados.";

        sb.AppendLine(data.Titulo);
        sb.AppendLine(new string('=', data.Titulo.Length));
        sb.AppendLine("Informe clínico generado en MediTrack a partir de los datos locales almacenados en MySQL.");
        sb.AppendLine();
        sb.AppendLine($"Paciente: {pacienteNombre}");
        sb.AppendLine($"Email: {data.PacienteUsuario?.Email}");
        sb.AppendLine($"Teléfono: {data.PacientePerfil?.Telefono}");
        sb.AppendLine($"Doctor asignado: {doctorNombre}");
        sb.AppendLine($"Doctores anteriores: {historialDoctores}");
        sb.AppendLine($"Periodo: {data.Reporte.FechaInicioPeriodo:dd/MM/yyyy} - {data.Reporte.FechaFinPeriodo:dd/MM/yyyy}");
        sb.AppendLine($"Fecha de generación: {data.Reporte.FechaGeneracion:dd/MM/yyyy HH:mm}");
        sb.AppendLine();
        var diseaseCatalog = data.CatalogoEnfermedades.ToDictionary(d => d.Id, d => d.Nombre);
        var activeDiseases = data.EnfermedadesPaciente.Where(d => d.Activa).ToList();
        var resolvedDiseases = data.EnfermedadesPaciente.Where(d => !d.Activa).ToList();

        sb.AppendLine("Enfermedades activas");
        sb.AppendLine("-------------------");
        foreach (var enfermedad in activeDiseases.DefaultIfEmpty())
        {
            sb.AppendLine(enfermedad == null
                ? "Sin enfermedades activas."
                : $"- {GetDiseaseName(diseaseCatalog, enfermedad.EnfermedadId)} desde {enfermedad.FechaDiagnostico:dd/MM/yyyy}");
        }

        sb.AppendLine();
        sb.AppendLine("Enfermedades superadas");
        sb.AppendLine("----------------------");
        foreach (var enfermedad in resolvedDiseases.DefaultIfEmpty())
        {
            sb.AppendLine(enfermedad == null
                ? "Sin enfermedades superadas registradas."
                : $"- {GetDiseaseName(diseaseCatalog, enfermedad.EnfermedadId)}: {enfermedad.FechaDiagnostico:dd/MM/yyyy} - {enfermedad.FechaFin:dd/MM/yyyy}, superada.");
        }

        sb.AppendLine();
        sb.AppendLine("Medicación actual");
        sb.AppendLine("-----------------");
        foreach (var medicamento in data.Medicamentos.DefaultIfEmpty())
        {
            sb.AppendLine(medicamento == null
                ? "Sin medicación activa."
                : $"- {medicamento.Nombre} | {medicamento.Dosis} | {medicamento.Frecuencia} | {medicamento.Horario}");
        }

        sb.AppendLine();
        sb.AppendLine("Mediciones registradas");
        sb.AppendLine("----------------------");
        foreach (var medicion in data.Mediciones.OrderByDescending(m => m.FechaHora).Take(15).DefaultIfEmpty())
        {
            sb.AppendLine(medicion == null
                ? "Sin mediciones en el periodo seleccionado."
                : $"- {medicion.FechaHora:dd/MM/yyyy HH:mm} | {MeasurementHelper.GetDisplayName(medicion.TipoMedicion)} | {medicion.Valor} {medicion.Unidad}");
        }

        sb.AppendLine();
        sb.AppendLine("Notas médicas relevantes");
        sb.AppendLine("-------------");
        foreach (var nota in data.Notas.OrderByDescending(n => n.FechaHora).Take(10).DefaultIfEmpty())
        {
            sb.AppendLine(nota == null
                ? "Sin notas médicas relevantes."
                : $"- {nota.FechaHora:dd/MM/yyyy HH:mm} | {nota.Titulo}: {nota.Contenido}");
        }

        sb.AppendLine();
        sb.AppendLine("Resumen");
        sb.AppendLine("-------");
        sb.AppendLine(data.Reporte.Resumen);

        return sb.ToString();
    }

    private static string GetDiseaseName(Dictionary<Guid, string> catalog, Guid diseaseId) =>
        catalog.TryGetValue(diseaseId, out var name) ? name : "Enfermedad no disponible";
}
