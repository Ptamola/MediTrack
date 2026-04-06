using MediTrack.Core.Enums;
using MediTrack.Core.Helpers;
using MediTrack.Core.Models;

namespace MediTrack.WinForms.Helpers;

public class MeasurementsChartPanel : Panel
{
    private readonly Dictionary<MeasurementType, Color> _colors = new()
    {
        [MeasurementType.Glucosa] = Color.FromArgb(220, 38, 38),
        [MeasurementType.Peso] = Color.FromArgb(37, 99, 235),
        [MeasurementType.PresionSistolica] = Color.FromArgb(14, 165, 233),
        [MeasurementType.PresionDiastolica] = Color.FromArgb(22, 163, 74)
    };

    public List<Measurement> Measurements { get; set; } = [];

    public MeasurementsChartPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        Dock = DockStyle.Fill;
        Padding = new Padding(24);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(Color.White);

        var rect = new Rectangle(50, 20, Width - 90, Height - 70);
        if (rect.Width <= 0 || rect.Height <= 0)
        {
            return;
        }

        using var axisPen = new Pen(Color.Gray, 1.2f);
        g.DrawRectangle(axisPen, rect);
        g.DrawString("Evolución clínica", AppTheme.SubtitleFont, Brushes.Black, new PointF(rect.Left, 0));

        var grouped = Measurements
            .Where(m => _colors.ContainsKey(m.TipoMedicion))
            .GroupBy(m => m.TipoMedicion)
            .ToList();

        if (grouped.Count == 0)
        {
            g.DrawString("No hay datos suficientes para mostrar la evolución.", AppTheme.BodyFont, Brushes.Gray, rect.Left + 10, rect.Top + 10);
            return;
        }

        var all = grouped.SelectMany(gp => gp).OrderBy(m => m.FechaHora).ToList();
        var minDate = all.Min(m => m.FechaHora);
        var maxDate = all.Max(m => m.FechaHora);
        var minValue = all.Min(m => m.Valor);
        var maxValue = all.Max(m => m.Valor);

        if (maxValue == minValue)
        {
            maxValue += 1;
        }

        if (maxDate == minDate)
        {
            maxDate = maxDate.AddDays(1);
        }

        foreach (var group in grouped)
        {
            using var pen = new Pen(_colors[group.Key], 2.5f);
            var points = group.OrderBy(x => x.FechaHora).Select(m =>
            {
                var xRatio = (float)((m.FechaHora - minDate).TotalMinutes / (maxDate - minDate).TotalMinutes);
                var yRatio = (float)((double)(m.Valor - minValue) / (double)(maxValue - minValue));
                var x = rect.Left + (xRatio * rect.Width);
                var y = rect.Bottom - (yRatio * rect.Height);
                return new PointF(x, y);
            }).ToArray();

            if (points.Length > 1)
            {
                g.DrawLines(pen, points);
            }

            foreach (var point in points)
            {
                g.FillEllipse(new SolidBrush(_colors[group.Key]), point.X - 4, point.Y - 4, 8, 8);
            }
        }

        var legendY = rect.Bottom + 12;
        var legendX = rect.Left;
        foreach (var group in grouped)
        {
            using var brush = new SolidBrush(_colors[group.Key]);
            g.FillRectangle(brush, legendX, legendY + 4, 14, 14);
            g.DrawString(MeasurementHelper.GetDisplayName(group.Key), AppTheme.BodyFont, Brushes.Black, legendX + 20, legendY);
            legendX += 160;
        }
    }
}
