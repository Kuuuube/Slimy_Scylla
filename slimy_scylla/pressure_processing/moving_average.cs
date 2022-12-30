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

    private uint moving_average(uint report) {
        last_pressures.Add(report);
        while (last_pressures.Count > amount) {
            last_pressures.RemoveAt(0);
        }
        while (last_pressures.Count < amount) {
            last_pressures.Add(report);
        }

        uint total = 0;
        foreach (uint pressure in last_pressures) {
            total += pressure;
        }

        report = total / (uint)last_pressures.Count;
        return report;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (report.Pressure <= pressure_deadzone_percent * get_max_pressure()) {
                report.Pressure = last_pressures.Last() / 2;
                device_report = report;
                last_pressures = new List<uint>();
                Emit?.Invoke(device_report);
                return;
            }
            
            report.Pressure = moving_average(report.Pressure);
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Amount"), DefaultPropertyValue(10)]
    public int amount { set; get; }

    [Property("Pressure Deadzone"), Unit("%")]
    public float pressure_deadzone_percent { set; get; }

    [BooleanProperty("Remove Tail", "")]
    public bool remove_tail { set; get; }
}