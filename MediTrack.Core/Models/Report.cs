namespace MediTrack.Core.Models;

public class Report
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PacienteId { get; set; }
    public Guid GeneradoPorUsuarioId { get; set; }
    public DateTime FechaGeneracion { get; set; } = DateTime.Now;
    public DateTime FechaInicioPeriodo { get; set; } = DateTime.Today.AddMonths(-1);
    public DateTime FechaFinPeriodo { get; set; } = DateTime.Today;
    public string Resumen { get; set; } = string.Empty;
    public string RutaPdf { get; set; } = string.Empty;
}
