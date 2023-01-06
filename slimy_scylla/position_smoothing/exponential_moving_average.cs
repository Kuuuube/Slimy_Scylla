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

    private Vector2 exponential_moving_average(Vector2 position) {
        if (last_position != new Vector2()) {
            position = new Vector2(position.X * (1 - amount) + last_position.X * amount, position.Y * (1 - amount) + last_position.Y * amount);
        }

        last_position = position;
        return position;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (!apply_to_hover && report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                report.Pressure = 0;
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
                if (last_position != new Vector2()) {
                    max_delta = Math.Max(Math.Abs(report.Position.X - last_position.X), Math.Abs(report.Position.Y - last_position.Y));
                } else {
                    max_delta = Math.Max(report.Position.X, report.Position.Y);
                }

                if (max_delta < max_lppx) {
                    report.Position = last_smoothed_position;
                    if (never_intercept_pressure_on_off && ((last_pressure > pressure_deadzone_percent / 100 * get_max_pressure() && report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) || (last_pressure <= pressure_deadzone_percent / 100 * get_max_pressure() && report.Pressure > pressure_deadzone_percent / 100 * get_max_pressure()))) {
                        //let the pressure through
                    } else {
                        report.Pressure = last_pressure;
                    }
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

    [Property("Amount"), DefaultPropertyValue(0.1f), ToolTip
        ("Amount: Min: 0.00, Max: 1.00, Default: 0.10\n" +
        "Changes the amount of smoothing applied. The higher the value within the min and max, the higher the smoothing.")]
    public float amount { set; get; }

    [BooleanProperty("Catch Up", ""), DefaultPropertyValue(true), ToolTip
        ("Catch Up: Min: False, Max: True, Default: True\n" +
        "When true, the current position is always updated. When false, the current position is held until it moves at least one pixel.")]
    public bool catch_up { set; get; }

    [Property("Pressure Deadzone"), Unit("%"), ToolTip
        ("Pressure Deadzone: Min: 0%, Max: 100%, Default: 0%\n" +
        "Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.")]
    public float pressure_deadzone_percent { set; get; }

    [BooleanProperty("Apply to Hover", ""), ToolTip
        ("Apply to Hover: Min: False, Max: True, Default: False\n" +
        "When true, the smoothing is applied while hovering. When false, smoothing is turned off while hovering.")]
    public bool apply_to_hover { set; get; }

    [BooleanProperty("Never Intercept Pressure on/off", ""), ToolTip
        ("Never Intercept Pressure on/off: Min: False, Max: True, Default: False\n" +
        "When true, while Catch Up is true, pressure on/off will send even if the position has moved less than one pixel.\n" +
        "When false, while Catch Up is true, pressure will wait to send until movement is detected.\n" +
        "This setting does not apply when Catch Up is false.")]
    public bool never_intercept_pressure_on_off { set; get; }
}