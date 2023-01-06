using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Input Degradation Pressure Random")]
public sealed class slimy_scylla_input_degradation_pressure_random : slimy_scylla_base
{
    private uint pressure_random(uint pressure) {
        float random = (float)new Random().NextDouble();
        if (completely_random_pressure) {
            pressure = (uint)(random * get_max_pressure());
        } else {
            pressure = (uint)(pressure + pressure * amount * (random * 2 - 1));
        }

        if (pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
            pressure = (uint)(pressure_deadzone_percent / 100 * get_max_pressure() + 1);
        }
        return pressure;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                report.Pressure = 0;
                Emit?.Invoke(device_report);
                return;
            }

            report.Pressure = pressure_random(report.Pressure);
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Amount"), DefaultPropertyValue(0.1f), ToolTip
        ("Amount: Min: 0.00, Max: 1.00, Default: 0.10\n" +
        "The amount of pressure change added or subtracted to the current pressure normalized between 0 and 1.")]
    public float amount { set; get; }

    [BooleanProperty("Completely Random Pressure", ""), ToolTip
        ("Completely Random Pressure: Min: False, Max: True, Default: False\n" +
        "When true, ignores the current pressure and apply a completely randomized value.")]
    public bool completely_random_pressure { set; get; }

    [Property("Pressure Deadzone"), Unit("%"), ToolTip
        ("Pressure Deadzone: Min: 0%, Max: 100%, Default: 0%\n" +
        "Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.")]
    public float pressure_deadzone_percent { set; get; }
}