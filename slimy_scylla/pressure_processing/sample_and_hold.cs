using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Pressure Processing Sample and Hold")]
public sealed class slimy_scylla_pressure_processing_sample_and_hold : slimy_scylla_base
{
    private uint hold_pressure = 0;
    private int samples = 0;
    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            samples++;

            if (report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                report.Pressure = 0;
                samples = 0;
                Emit?.Invoke(device_report);
                return;
            }

            if (samples == sample_number) {
                hold_pressure = report.Pressure;
            }

            if (samples > sample_number) {
                report.Pressure = hold_pressure;
            }

            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Sample Number"), DefaultPropertyValue(15), ToolTip
        ("Sample Number: Min: 1, Max: 100, Default: 15\n" +
        "The number of samples before locking pressure.")]
    public int sample_number { set; get; }

    [Property("Pressure Deadzone"), Unit("%"), ToolTip
        ("Pressure Deadzone: Min: 0%, Max: 100%, Default: 0%\n" +
        "Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.")]
    public float pressure_deadzone_percent { set; get; }
}