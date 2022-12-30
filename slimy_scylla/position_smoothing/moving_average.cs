using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Position Smoothing Moving Average")]
public sealed class slimy_scylla_position_smoothing_moving_average : slimy_scylla_base
{
    private bool emit = true;
    private List<Vector2> last_positions = new List<Vector2>();
    private Vector2 last_smoothed_position = new Vector2();

    private Vector2 moving_average(Vector2 report) {
        last_positions.Add(report);
        while (last_positions.Count > amount) {
            last_positions.RemoveAt(0);
        }

        if (last_positions.Count == amount) {
            Vector2 total = new Vector2();
            foreach (Vector2 position in last_positions) {
                total.X += position.X;
                total.Y += position.Y;
            }

            report.X = total.X / (float)amount;
            report.Y = total.Y / (float)amount;
            return report;
        }

        emit = false;
        return report;
    }

    public override event Action<IDeviceReport> Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (!apply_to_hover && report.Pressure <= pressure_deadzone) {
                last_positions = new List<Vector2>();
                Emit?.Invoke(device_report);
                return;
            }

            if (catch_up) {
                report.Position = moving_average(report.Position);
            } else {
                //ignore reports that move less than one pixel
                float max_lppx = Math.Max(lines_per_pixel().X, lines_per_pixel().Y);
                float max_delta;
                if (last_positions.Count > 0) {
                    max_delta = Math.Max(Math.Abs(report.Position.X - last_positions.Last().X), Math.Abs(report.Position.Y - last_positions.Last().Y));
                } else {
                    max_delta = Math.Max(report.Position.X, report.Position.Y);
                }

                if (max_delta < max_lppx) {
                    report.Position = last_smoothed_position;
                    device_report = report;
                    Emit?.Invoke(device_report);
                    return;
                }
                report.Position = moving_average(report.Position);
                last_smoothed_position = report.Position;
            }
            device_report = report;
        }

        if (emit) {
            Emit?.Invoke(device_report);
        } else {
            emit = true;
        }
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Amount"), DefaultPropertyValue(2)]
    public int amount { set; get; }

    [BooleanProperty("Catch Up (pen only)", "")]
    public bool catch_up { set; get; }

    [Property("Pressure Deadzone")]
    public int pressure_deadzone { set; get; }

    [BooleanProperty("Apply to Hover", "")]
    public bool apply_to_hover { set; get; }
}