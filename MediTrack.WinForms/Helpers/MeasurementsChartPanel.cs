using System.Drawing.Drawing2D;
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
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.White);
        g.DrawString("Evolución clínica", AppTheme.SubtitleFont, Brushes.Black, new PointF(24, 14));

        var grouped = Measurements
            .Where(m => _colors.ContainsKey(m.TipoMedicion))
            .GroupBy(m => m.TipoMedicion)
            .OrderBy(group => MeasurementHelper.GetDisplayName(group.Key))
            .ToList();

        if (grouped.Count == 0)
        {
            DrawCenteredMessage(g, "No hay datos suficientes para mostrar la evolución.");
            return;
        }

        var chartArea = new Rectangle(70, 52, Width - 104, Height - 112);
        if (chartArea.Width < 220 || chartArea.Height < 90)
        {
            DrawCenteredMessage(g, "Amplía esta sección para ver la evolución clínica.");
            return;
        }

        var laneGap = 16;
        var laneHeight = (chartArea.Height - (laneGap * (grouped.Count - 1))) / grouped.Count;
        if (laneHeight < 42)
        {
            DrawCenteredMessage(g, "Amplía esta sección para ver la evolución clínica.");
            return;
        }

        for (var index = 0; index < grouped.Count; index++)
        {
            var group = grouped[index].OrderBy(item => item.FechaHora).ToList();
            var lane = new Rectangle(
                chartArea.Left,
                chartArea.Top + (index * (laneHeight + laneGap)),
                chartArea.Width,
                laneHeight);

            DrawLaneBackground(g, lane);
            DrawLaneTitle(g, group[0].TipoMedicion, lane);
            DrawSeries(g, group, lane);
        }
    }

    private void DrawSeries(Graphics g, List<Measurement> group, Rectangle lane)
    {
        var minDate = group.Min(m => m.FechaHora);
        var maxDate = group.Max(m => m.FechaHora);
        var minValue = group.Min(m => m.Valor);
        var maxValue = group.Max(m => m.Valor);

        if (maxDate == minDate)
        {
            maxDate = maxDate.AddDays(1);
        }

        if (maxValue == minValue)
        {
            maxValue += 1;
            minValue -= 1;
        }

        var plot = Rectangle.Inflate(lane, -34, -22);
        if (plot.Width <= 0 || plot.Height <= 0)
        {
            return;
        }

        var type = group[0].TipoMedicion;
        using var pen = new Pen(_colors[type], 2.5f);
        using var brush = new SolidBrush(_colors[type]);
        var points = group.Select(m =>
        {
            var xRatio = (float)((m.FechaHora - minDate).TotalMinutes / (maxDate - minDate).TotalMinutes);
            var yRatio = (float)((double)(m.Valor - minValue) / (double)(maxValue - minValue));
            var x = plot.Left + (xRatio * plot.Width);
            var y = plot.Bottom - (yRatio * plot.Height);
            return new PointF(Math.Clamp(x, plot.Left, plot.Right), Math.Clamp(y, plot.Top, plot.Bottom));
        }).ToArray();

        if (points.Length > 1)
        {
            g.DrawLines(pen, points);
        }

        foreach (var point in points)
        {
            g.FillEllipse(brush, point.X - 4, point.Y - 4, 8, 8);
        }

        g.DrawString($"{maxValue:0.##}", AppTheme.SmallFont, Brushes.Gray, lane.Left + 6, plot.Top - 6);
        g.DrawString($"{minValue:0.##}", AppTheme.SmallFont, Brushes.Gray, lane.Left + 6, plot.Bottom - 14);
    }

    private static void DrawLaneBackground(Graphics g, Rectangle lane)
    {
        using var borderPen = new Pen(AppTheme.Border, 1f);
        using var gridPen = new Pen(Color.FromArgb(226, 232, 240), 1f);

        for (var i = 1; i < 4; i++)
        {
            var y = lane.Top + (lane.Height * i / 4);
            g.DrawLine(gridPen, lane.Left, y, lane.Right, y);
        }

        g.DrawRectangle(borderPen, lane);
    }

    private void DrawLaneTitle(Graphics g, MeasurementType type, Rectangle lane)
    {
        using var brush = new SolidBrush(_colors[type]);
        g.FillRectangle(brush, lane.Left + 8, lane.Top + 8, 10, 10);
        g.DrawString(MeasurementHelper.GetDisplayName(type), AppTheme.SmallFont, Brushes.Black, lane.Left + 24, lane.Top + 4);
    }

    private void DrawCenteredMessage(Graphics g, string text)
    {
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        g.DrawString(text, AppTheme.BodyFont, Brushes.Gray, ClientRectangle, format);
    }
}
