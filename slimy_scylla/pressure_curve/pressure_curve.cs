using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Pressure Curve")]
public sealed class slimy_scylla_pressure_curve : slimy_scylla_base
{
    private uint pressure_curve(uint pressure) {
        double pressure_double = (double)pressure / (double)get_max_pressure();
        int invert_int = invert ? 1 : 0;

        if (softness <= 0) {
            pressure_double = ((invert_int * 2 - 1) * -1) * (Math.Pow(pressure_double * gain, 1 - softness)) + invert_int;
        }
        if (softness > 0) {
            pressure_double = ((invert_int * 2 - 1) * -1) * (Math.Pow(pressure_double * gain, 1 / (1 + softness))) + invert_int;
        }

        if (minimum_value_percent > maximum_value_percent) {
            pressure_double = maximum_value_percent / 100;
        } else {
            pressure_double = Math.Clamp(pressure_double, minimum_value_percent / 100, maximum_value_percent / 100);
        }

        return (uint)(pressure_double * (double)get_max_pressure());
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

            report.Pressure = pressure_curve(report.Pressure);
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Gain"), DefaultPropertyValue(1f)]
    public float gain { set; get; }

    [Property("Softness"), DefaultPropertyValue(0f)]
    public float softness { set; get; }

    [Property("Minimum Value"), DefaultPropertyValue(0f), Unit("%")]
    public float minimum_value_percent { set; get; }

    [Property("Maximum Value"), DefaultPropertyValue(100f), Unit("%")]
    public float maximum_value_percent { set; get; }

    [BooleanProperty("Invert", "")]
    public bool invert { set; get; }

    [Property("Pressure Deadzone"), Unit("%")]
    public float pressure_deadzone_percent { set; get; }
}