using MediTrack.Core.DTOs;
using MediTrack.Core.Interfaces.Repositories;
using MediTrack.Core.Interfaces.Services;
using MediTrack.Core.Models;
using MediTrack.Core.Reports;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MediTrack.Core.Services;

public class ReportService(
    IUserRepository userRepository,
    IPatientRepository patientRepository,
    IDiseaseService diseaseService,
    IMedicationService medicationService,
    IMeasurementService measurementService,
    IMedicalNoteService medicalNoteService,
    IDoctorAssignmentService doctorAssignmentService,
    IReportRepository reportRepository) : IReportService
{
    public async Task<List<Report>> GetByPatientAsync(Guid patientId) =>
        (await reportRepository.GetAllAsync())
        .Where(r => r.PacienteId == patientId)
        .OrderByDescending(r => r.FechaGeneracion)
        .ToList();

    public async Task<GeneratedReportResult> GenerateAsync(Guid patientId, Guid generadoPorUsuarioId, DateTime fechaInicio, DateTime fechaFin, bool saveToHistory = true)
    {
        var users = await userRepository.GetAllAsync();
        var patientUser = users.FirstOrDefault(u => u.Id == patientId);
        var patientProfile = (await patientRepository.GetAllAsync()).FirstOrDefault(p => p.IdUsuario == patientId);
        var assignedDoctor = (await doctorAssignmentService.GetDoctorsForPatientAsync(patientId)).FirstOrDefault();
        var previousDoctors = await doctorAssignmentService.GetPreviousDoctorsForPatientAsync(patientId);
        var catalog = await diseaseService.GetCatalogAsync();
        var patientDiseases = await diseaseService.GetPatientDiseasesAsync(patientId);
        var diseases = patientDiseases
            .Join(catalog, pd => pd.EnfermedadId, d => d.Id, (_, d) => d)
            .DistinctBy(d => d.Id)
            .ToList();
        var medications = (await medicationService.GetByPatientAsync(patientId)).Where(m => m.Activo).ToList();
        var measurements = await measurementService.GetByPatientAsync(patientId, null, fechaInicio, fechaFin.AddDays(1).AddSeconds(-1));
        var notes = await medicalNoteService.GetByPatientAsync(patientId, false);

        var report = new Report
        {
            Id = Guid.NewGuid(),
            PacienteId = patientId,
            GeneradoPorUsuarioId = generadoPorUsuarioId,
            FechaGeneracion = DateTime.Now,
            FechaInicioPeriodo = fechaInicio,
            FechaFinPeriodo = fechaFin,
            Resumen = BuildSummary(diseases.Count, medications.Count, measurements.Count, notes.Count)
        };

        var result = new GeneratedReportResult
        {
            Reporte = report,
            Titulo = $"Informe clínico - {patientUser?.NombreCompleto}",
            PacienteUsuario = patientUser,
            PacientePerfil = patientProfile,
            DoctorAsignado = assignedDoctor,
            DoctoresAnteriores = previousDoctors,
            Enfermedades = diseases,
            Medicamentos = medications,
            Mediciones = measurements,
            Notas = notes
        };

        if (saveToHistory)
        {
            var reports = await reportRepository.GetAllAsync();
            reports.Add(report);
            await reportRepository.SaveAllAsync(reports);
        }

        result.ContenidoVistaPrevia = ReportPreviewBuilder.Build(result);
        return result;
    }

    public async Task<OperationResult> ExportPdfAsync(GeneratedReportResult reportResult, string outputFilePath)
    {
        if (string.IsNullOrWhiteSpace(outputFilePath))
        {
            return OperationResult.Fail("Debes indicar una ruta de archivo válida.");
        }

        var directory = Path.GetDirectoryName(outputFilePath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return OperationResult.Fail("No se pudo determinar la carpeta de destino del PDF.");
        }

        Directory.CreateDirectory(directory);

        Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.Size(PageSizes.A4);
                page.Header().Text(reportResult.Titulo).FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                page.Content().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Text($"Paciente: {reportResult.PacienteUsuario?.NombreCompleto}");
                    column.Item().Text($"Doctor asignado: {reportResult.DoctorAsignado?.NombreCompleto ?? "Sin asignar"}");
                    column.Item().Text(
                        $"Doctores anteriores: {(reportResult.DoctoresAnteriores.Count > 0 ? string.Join(", ", reportResult.DoctoresAnteriores.Select(d => d.NombreCompleto)) : "No hay doctores previos registrados.")}");
                    column.Item().Text($"Periodo: {reportResult.Reporte.FechaInicioPeriodo:dd/MM/yyyy} - {reportResult.Reporte.FechaFinPeriodo:dd/MM/yyyy}");
                    column.Item().Text($"Resumen: {reportResult.Reporte.Resumen}");

                    column.Item().Text("Enfermedades").Bold();
                    foreach (var disease in reportResult.Enfermedades.DefaultIfEmpty())
                    {
                        column.Item().Text(disease == null ? "Sin enfermedades registradas." : $"- {disease.Nombre}");
                    }

                    column.Item().Text("Medicamentos activos").Bold();
                    foreach (var medication in reportResult.Medicamentos.DefaultIfEmpty())
                    {
                        column.Item().Text(medication == null
                            ? "Sin medicación activa."
                            : $"- {medication.Nombre} | {medication.Dosis} | {medication.Frecuencia} | {medication.Horario}");
                    }

                    column.Item().Text("Mediciones").Bold();
                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Text("Fecha");
                            header.Cell().Text("Tipo");
                            header.Cell().Text("Valor");
                        });

                        foreach (var measurement in reportResult.Mediciones.Take(20))
                        {
                            table.Cell().Text(measurement.FechaHora.ToString("dd/MM/yyyy HH:mm"));
                            table.Cell().Text(Helpers.MeasurementHelper.GetDisplayName(measurement.TipoMedicion));
                            table.Cell().Text($"{measurement.Valor} {measurement.Unidad}");
                        }
                    });

                    column.Item().Text("Notas médicas").Bold();
                    foreach (var note in reportResult.Notas.Take(10).DefaultIfEmpty())
                    {
                        column.Item().Text(note == null
                            ? "Sin notas médicas."
                            : $"- {note.FechaHora:dd/MM/yyyy HH:mm} | {note.Titulo}: {note.Contenido}");
                    }
                });
                page.Footer().AlignCenter().Text($"Generado por MediTrack el {DateTime.Now:dd/MM/yyyy HH:mm}");
            });
        }).GeneratePdf(outputFilePath);

        var reports = await reportRepository.GetAllAsync();
        reportResult.Reporte.RutaPdf = outputFilePath;
        var existing = reports.FirstOrDefault(r => r.Id == reportResult.Reporte.Id);
        if (existing == null)
        {
            reports.Add(reportResult.Reporte);
        }
        else
        {
            existing.RutaPdf = outputFilePath;
            existing.Resumen = reportResult.Reporte.Resumen;
            existing.FechaInicioPeriodo = reportResult.Reporte.FechaInicioPeriodo;
            existing.FechaFinPeriodo = reportResult.Reporte.FechaFinPeriodo;
            existing.FechaGeneracion = reportResult.Reporte.FechaGeneracion;
        }

        await reportRepository.SaveAllAsync(reports);

        return OperationResult.Ok($"PDF exportado correctamente en: {outputFilePath}");
    }

    private static string BuildSummary(int diseasesCount, int medicationCount, int measurementCount, int notesCount)
    {
        return $"El paciente presenta {diseasesCount} enfermedad(es) activa(s), {medicationCount} medicamento(s) activo(s), {measurementCount} medición(es) registradas en el período y {notesCount} nota(s) médica(s) disponibles.";
    }
}
