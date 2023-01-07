using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Tilt Smoothing Moving Average")]
public sealed class slimy_scylla_tilt_smoothing_moving_average : slimy_scylla_base
{
    private List<Vector2> last_tilts = new List<Vector2>();

    private Vector2 moving_average(Vector2 tilt) {
        last_tilts.Add(tilt);
        while (last_tilts.Count > amount) {
            last_tilts.RemoveAt(0);
        }
        while (last_tilts.Count < amount) {
            last_tilts.Add(tilt);
        }

        Vector2 total = new Vector2();
        foreach (Vector2 last_tilt in last_tilts) {
            total.X += last_tilt.X;
            total.Y += last_tilt.Y;
        }

        tilt.X = total.X / (float)last_tilts.Count;
        tilt.Y = total.Y / (float)last_tilts.Count;

        return tilt;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITiltReport report) {
            report.Tilt = moving_average(report.Tilt);
            device_report = report;
        }
        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Amount"), DefaultPropertyValue(10), ToolTip
        ("Amount: Min: 2, Max: 200, Default: 10\n" +
        "The number of reports to average. The higher the value, the higher the smoothing.")]
    public int amount { set; get; }
}