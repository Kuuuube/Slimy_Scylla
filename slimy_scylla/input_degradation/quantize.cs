using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Input Degradation Quantize")]
public sealed class slimy_scylla_input_degradation_quantize : slimy_scylla_base
{
    private Vector2 quantize(Vector2 position) {
        Vector2 grid_size = new Vector2(scale * x_grid * lines_per_pixel().X, scale * y_grid * lines_per_pixel().Y);
        position = new Vector2((float)Math.Floor(position.X / grid_size.X) * grid_size.X, (float)Math.Floor(position.Y / grid_size.Y) * grid_size.Y);
        return position;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (!apply_to_hover && report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                report.Pressure = 0;
                Emit?.Invoke(device_report);
                return;
            }

            report.Position = quantize(report.Position);
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("X Grid"), DefaultPropertyValue(10), ToolTip
        ("X Grid: Min: 0, Max: 100, Default: 10\n" +
        "The smallest step in pixels the position can move in the X axis.")]
    public int x_grid { set; get; }

    [Property("Y Grid"), DefaultPropertyValue(20), ToolTip
        ("Y Grid: Min: 0, Max: 100, Default: 20\n" +
        "The smallest step in pixels the position can move in the Y axis.")]
    public int y_grid { set; get; }

    [Property("Scale"), DefaultPropertyValue(1f), ToolTip
        ("Scale: Min: 0.00, Max: 2.00, Default: 1.00\n" +
        "Multiplier for the size of the grid in both axes.")]
    public float scale { set; get; }

    [Property("Pressure Deadzone"), Unit("%"), ToolTip
        ("Pressure Deadzone: Min: 0%, Max: 100%, Default: 0%\n" +
        "Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.")]
    public float pressure_deadzone_percent { set; get; }

    [BooleanProperty("Apply to Hover", ""), ToolTip
        ("Apply to Hover: Min: False, Max: True, Default: False\n" +
        "When true, the filter is applied while hovering. When false, filter is turned off while hovering.")]
    public bool apply_to_hover { set; get; }
}