using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Input Degredation Quantize")]
public sealed class slimy_scylla_input_degredation_quantize : slimy_scylla_base
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

    [Property("X Grid"), DefaultPropertyValue(10)]
    public int x_grid { set; get; }

    [Property("Y Grid"), DefaultPropertyValue(20)]
    public int y_grid { set; get; }

    [Property("Scale"), DefaultPropertyValue(1f)]
    public float scale { set; get; }

    [Property("Pressure Deadzone"), Unit("%")]
    public float pressure_deadzone_percent { set; get; }

    [BooleanProperty("Apply to Hover", "")]
    public bool apply_to_hover { set; get; }
}