using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Pressure Processing Moving Average")]
public sealed class slimy_scylla_pressure_processing_moving_average : slimy_scylla_base
{
    private List<uint> last_pressures = new List<uint>();
    private Vector2 last_pos = new Vector2();
    private int tail_reports = 0;

    private uint moving_average(uint pressure) {
        last_pressures.Add(pressure);
        while (last_pressures.Count > amount) {
            last_pressures.RemoveAt(0);
        }
        while (last_pressures.Count < amount) {
            last_pressures.Add(pressure);
        }

        uint total = 0;
        foreach (uint last_pressure in last_pressures) {
            total += pressure;
        }

        pressure = total / (uint)last_pressures.Count;
        return pressure;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                if (tail_reports > 0) {
                    report.Position = last_pos;
                    tail_reports--;
                }
                last_pressures = new List<uint>();
                report.Pressure = 0;
                Emit?.Invoke(device_report);
                return;
            }
            
            report.Pressure = moving_average(report.Pressure);
            last_pos = report.Position;
            tail_reports = remove_tail_pressure_reports;
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Amount"), DefaultPropertyValue(10), ToolTip
        ("Amount: Min: 2, Max: 200, Default: 10\n" +
        "The number of reports to average. The higher the value, the higher the smoothing.")]
    public int amount { set; get; }

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