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
    private Vector2 last_pos = new Vector2();
    private int tail_reports = 0;


    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            samples++;

            if (report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                if (tail_reports > 0) {
                    report.Position = last_pos;
                    tail_reports--;
                }
                samples = 0;
                report.Pressure = 0;
                Emit?.Invoke(device_report);
                return;
            }

            if (samples == sample_number) {
                hold_pressure = report.Pressure;
            }

            if (samples > sample_number) {
                report.Pressure = hold_pressure;
            }

            last_pos = report.Position;
            tail_reports = remove_tail_pressure_reports;
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

    [Property("Remove Tail Pressure Reports"), DefaultPropertyValue(1), ToolTip
        ("Remove Tail Pressure Reports: Min: 0, Max: 10, Default: 1\n" +
        "Stops drawing programs from adding their own smoothing at the end of lines which commonly creates \"shoelace line endings\" or \"line tails\".\n" +
        "Usually setting this to 1 is enough for it function properly. Only increase the value if required.")]
    public int remove_tail_pressure_reports { set; get; }
}