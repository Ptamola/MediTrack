namespace MediTrack.Core.Models;

public class MedicationReminder
{
    public Guid MedicamentoId { get; set; }
    public string MedicamentoNombre { get; set; } = string.Empty;
    public DateTime ProximaToma { get; set; }
    public string TextoRecordatorio { get; set; } = string.Empty;
}
