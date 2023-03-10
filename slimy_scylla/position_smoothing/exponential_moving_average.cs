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
    private Vector2 last_raw_position = new Vector2();
    private uint last_pressure = 0;
    private int tail_reports = 0;

    private Vector2 exponential_moving_average(Vector2 position) {
        //scale the smoothing amount by x^(0.02/x) to make the difference between different settings feels better
        //the perceptible difference in smoothing between settings when using an unscaled linear smoothing amount is not linear
        //when scaling exponentially, users should feel the difference between, for example, 0.2 and 0.4 or 0.6 and 0.8 much better
        float scaled_amount = (float)Math.Pow(amount, 0.02 / amount);
        if (last_position != new Vector2()) {
            position = new Vector2(position.X * (1 - scaled_amount) + last_position.X * scaled_amount, position.Y * (1 - scaled_amount) + last_position.Y * scaled_amount);
        }

        last_position = position;
        return position;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (!always_apply_to_hover && report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                report.Pressure = 0;
                last_pressure = 0;

                if (tail_reports <= 0) {
                    last_position = new Vector2();
                    last_raw_position = new Vector2();
                    Emit?.Invoke(device_report);
                    return;
                }

                if (tail_reports > 0) {
                    tail_reports--;
                    if (!leak_smoothing_to_hover) {
                        report.Position = last_position;
                        Emit?.Invoke(device_report);
                        return;
                    }
                }
            }

            if (report.Pressure > pressure_deadzone_percent / 100 * get_max_pressure() && !always_apply_to_hover) {
                //must be reset before a new line can be drawn
                if (tail_reports < remove_tail_position_reports) {
                    last_position = new Vector2();
                    last_raw_position = new Vector2();
                }
                tail_reports = remove_tail_position_reports;
            }

            if (catch_up) {
                report.Position = exponential_moving_average(report.Position);
            } else {
                //ignore reports that move less than one pixel
                float max_lppx = Math.Max(lines_per_pixel().X, lines_per_pixel().Y);
                float max_delta;
                if (last_raw_position != new Vector2()) {
                    max_delta = Math.Max(Math.Abs(report.Position.X - last_raw_position.X), Math.Abs(report.Position.Y - last_raw_position.Y));
                } else {
                    max_delta = Math.Max(report.Position.X, report.Position.Y);
                }

                if (max_delta < max_lppx) {
                    report.Position = last_position;
                    if (never_intercept_pressure_on_off && ((last_pressure > pressure_deadzone_percent / 100 * get_max_pressure() && report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) || (last_pressure <= pressure_deadzone_percent / 100 * get_max_pressure() && report.Pressure > pressure_deadzone_percent / 100 * get_max_pressure()))) {
                        //let the pressure through
                    } else {
                        report.Pressure = last_pressure;
                    }
                    device_report = report;
                    Emit?.Invoke(device_report);
                    return;
                }
                last_raw_position = report.Position;
                report.Position = exponential_moving_average(report.Position);
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

    [Property("Remove Tail Position Reports"), DefaultPropertyValue(1), ToolTip
        ("Remove Tail Position Reports: Min: 0, Max: 10, Default: 1\n" +
        "Stops drawing programs from adding their own smoothing at the end of lines which commonly creates \"shoelace line endings\" or \"line tails\".\n" +
        "A sudden position change when transitioning to hover can cause unintended lines.\n" +
        "Usually setting this to 1 is enough for it function properly. Only increase the value if required.")]
    public int remove_tail_position_reports { set; get; }

    [BooleanProperty("Leak Smoothing to Hover", ""), ToolTip
        ("Leak Smoothing to Hover: Min: False, Max: True, Default: False\n" +
        "Applies smoothing to the amount of reports in Remove Tail Position Reports after the pen tip is released instead of holding the last postition.\n" +
        "Using any other filter (including Pressure Processing) with Remove Tail Position Reports > 0 and without Leak Smoothing to Hover enabled will override this option.")]
    public bool leak_smoothing_to_hover { set; get; }

    [BooleanProperty("Always Apply to Hover", ""), ToolTip
        ("Apply to Hover: Min: False, Max: True, Default: False\n" +
        "When true, the smoothing is applied while hovering. When false, smoothing is turned off while hovering.")]
    public bool always_apply_to_hover { set; get; }

    [BooleanProperty("Never Intercept Pressure on/off", ""), ToolTip
        ("Never Intercept Pressure on/off: Min: False, Max: True, Default: False\n" +
        "When true, while Catch Up is false, pressure on/off will send even if the position has moved less than one pixel.\n" +
        "When false, while Catch Up is false, pressure will wait to send until movement is detected.\n" +
        "This setting does not apply when Catch Up is true.")]
    public bool never_intercept_pressure_on_off { set; get; }
}