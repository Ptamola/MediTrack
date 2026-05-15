using MediTrack.Core.Enums;

namespace MediTrack.Core.Models;

/// <summary>
/// Registro puntual de una medicion clinica usada en historiales, graficas e informes.
/// </summary>
public class Measurement
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PacienteId { get; set; }
    public MeasurementType TipoMedicion { get; set; }
    public decimal Valor { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; } = DateTime.Now;
    public string Observaciones { get; set; } = string.Empty;
}
