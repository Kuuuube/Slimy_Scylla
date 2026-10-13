using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Position Smoothing Pulled String")]
public sealed class slimy_scylla_position_smoothing_pulled_string : slimy_scylla_base
{
    private Vector2 last_position = new();
    private uint last_pressure = 0;
    private bool first_report = true;
    private int tail_reports = 0;

    private Vector2 pulled_string(Vector2 position, Vector2 string_length_lines, Vector2 delta) {
        // Don't use Math.Sign since it returns 0 for 0, we need to return 1 for 0
        var delta_sign = new Vector2(delta.X < 0 ? -1 : 1, delta.Y < 0 ? -1 : 1);

        var m_slope = (position.Y - (position.Y + delta.Y)) / (position.X - (position.X + delta.X));
        var a_radius = string_length_lines.X;
        var b_radius = string_length_lines.Y;

        var a_radius2 = a_radius * a_radius;
        var b_radius2 = b_radius * b_radius;
        var m_slope2 = m_slope * m_slope;

        // Calculate the length of the string at the exact angle of the cursor's movement.
        // This will equal the intersection of the slope of the cursor from its last position `position` to its current position `position + delta` and the ellipse of the string.
        //
        // slope (in this case, b = 0): y = mx + b, ellipse formula: y = √(b ^ 2 * (1 - x ^ 2 / a ^ 2))
        // slope: y = m_slope * x, ellipse: y = √(b_radius ^ 2 * (1 - x ^ 2 / a_radius ^ 2))
        // intersection (set sides equal to eachother): m_slope * x = √(b_radius ^ 2 * (1 - x ^ 2 / a_radius ^ 2))
        // intersection (rearranged to x): x = √(b_radius ^ 2 / (m_slope ^ 2 + b_radius ^ 2 / a_radius ^ 2))

        var string_length_x = Math.Sqrt(b_radius2 / (m_slope2 + b_radius2 / a_radius2));
        var string_length_y = m_slope * string_length_x;

        // String length deltas must be absoluted then adjusted to the delta's sign since we're only solving for a semicircle intersection instead of a full circle (which makes the math more simple)
        position = new Vector2(position.X - (float)Math.Abs(string_length_x) * Math.Sign(delta.X), position.Y - (float)Math.Abs(string_length_y) * Math.Sign(delta.Y));

        last_position = position;
        return position;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport? device_report)
    {
        if (device_report == null) return;

        if (device_report is ITabletReport report) {
            if (!always_apply_to_hover && report.Pressure == 0) {
                if (tail_reports <= 0) {
                    last_position = new Vector2();
                    first_report = true;
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

            if (report.Pressure > 0 && !always_apply_to_hover) {
                //must be reset before a new line can be drawn
                if (tail_reports < remove_tail_position_reports) {
                    last_position = new Vector2();
                    first_report = true;
                }
                tail_reports = remove_tail_position_reports;
            }

            if (first_report) {
                if (!never_intercept_pressure_on_off) {
                    report.Pressure = 0;
                    last_pressure = 0;
                }
                last_position = report.Position;
                first_report = false;
                Emit?.Invoke(device_report);
                return;
            }

            Vector2 lppx = new(lines_per_pixel().X, lines_per_pixel().Y);
            Vector2 delta = new(report.Position.X - last_position.X, report.Position.Y - last_position.Y);

            //ignore reports that move less than string length px
            if ((Math.Pow(delta.X - 0, 2) / Math.Pow(lppx.X * string_length, 2)) + (Math.Pow(delta.Y - 0, 2) / Math.Pow(lppx.Y * string_length, 2)) < 1) {
                report.Position = last_position;
                if (never_intercept_pressure_on_off && ((last_pressure > 0 && report.Pressure == 0) || (last_pressure == 0 && report.Pressure > 0))) {
                    //let the pressure through
                } else {
                    if (report.Pressure != 0) {
                        report.Pressure = last_pressure;
                    }
                }
                device_report = report;
                Emit?.Invoke(device_report);
                return;
            }
            report.Position = pulled_string(report.Position, new Vector2(lppx.X * string_length, lppx.Y * string_length), delta);
            last_pressure = report.Pressure;
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("String Length"), DefaultPropertyValue(35), Unit("px"), ToolTip
        ("String Length: Min: 0, Max: 200, Default: 35\n" +
        "The radius of the circular deadzone in pixels.")]
    public int string_length { set; get; }

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

    [BooleanProperty("Never Intercept Pressure on/off", ""),DefaultPropertyValue(true), ToolTip
        ("Never Intercept Pressure on/off: Min: False, Max: True, Default: True\n" +
        "When true, pressure on/off will send even if the position has moved less than one pixel. When false, pressure will wait to send until movement is detected.")]
    public bool never_intercept_pressure_on_off { set; get; }
}