using MediTrack.Core.Models;

namespace MediTrack.Core.Helpers;

public static class ReminderHelper
{
    public static List<MedicationReminder> BuildReminders(IEnumerable<Medication> medications)
    {
        var now = DateTime.Now;
        var reminders = new List<MedicationReminder>();

        foreach (var medication in medications.Where(m => m.Activo))
        {
            var scheduleParts = medication.Horario
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var part in scheduleParts)
            {
                if (!TimeOnly.TryParse(part, out var time))
                {
                    continue;
                }

                var next = DateTime.Today.Add(time.ToTimeSpan());
                if (next < now)
                {
                    next = next.AddDays(1);
                }

                reminders.Add(new MedicationReminder
                {
                    MedicamentoId = medication.Id,
                    MedicamentoNombre = medication.Nombre,
                    ProximaToma = next,
                    TextoRecordatorio = $"{medication.Nombre} - {medication.Dosis} a las {time:HH\\:mm}"
                });
            }
        }

        return reminders
            .OrderBy(r => r.ProximaToma)
            .Take(10)
            .ToList();
    }
}
