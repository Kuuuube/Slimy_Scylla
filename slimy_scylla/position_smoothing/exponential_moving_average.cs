using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Position Smoothing Exponential Moving Average")]
public sealed class slimy_scylla_position_smoothing_exponential_moving_average : slimy_scylla_base
{
    private Vector2 last_position = new Vector2();
    private Vector2 last_smoothed_position = new Vector2();
    private uint last_pressure = 0;

    private Vector2 exponential_moving_average(Vector2 report) {
        if (last_position != new Vector2()) {
            report = new Vector2(report.X * (1 - amount) + last_position.X * amount, report.Y * (1 - amount) + last_position.Y * amount);
        }

        last_position = report;
        return report;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (!apply_to_hover && report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                last_position = new Vector2();
                Emit?.Invoke(device_report);
                return;
            }

            if (catch_up) {
                report.Position = exponential_moving_average(report.Position);
            } else {
                //ignore reports that move less than one pixel
                float max_lppx = Math.Max(lines_per_pixel().X, lines_per_pixel().Y);
                float max_delta;
                if (last_position != null) {
                    max_delta = Math.Max(Math.Abs(report.Position.X - last_position.X), Math.Abs(report.Position.Y - last_position.Y));
                } else {
                    max_delta = Math.Max(report.Position.X, report.Position.Y);
                }

                if (max_delta < max_lppx) {
                    report.Position = last_smoothed_position;
                    report.Pressure = last_pressure;
                    device_report = report;
                    Emit?.Invoke(device_report);
                    return;
                }
                report.Position = exponential_moving_average(report.Position);
                last_smoothed_position = report.Position;
                last_pressure = report.Pressure;
            }
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Amount"), DefaultPropertyValue(0.1f)]
    public float amount { set; get; }

    [BooleanProperty("Catch Up", ""), DefaultPropertyValue(true)]
    public bool catch_up { set; get; }

    [Property("Pressure Deadzone"), Unit("%")]
    public float pressure_deadzone_percent { set; get; }

    [BooleanProperty("Apply to Hover", "")]
    public bool apply_to_hover { set; get; }
}