using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Pressure Processing Fixed Value")]
public sealed class slimy_scylla_pressure_processing_fixed_value : slimy_scylla_base
{
    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (report.Pressure <= pressure_deadzone_percent * get_max_pressure()) {
                Emit?.Invoke(device_report);
                return;
            }

            report.Pressure = (uint)(value_percent * get_max_pressure());
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Value"), DefaultPropertyValue(0.22), Unit("%")]
    public float value_percent { set; get; }

    [Property("Pressure Deadzone"), Unit("%")]
    public float pressure_deadzone_percent { set; get; }
}