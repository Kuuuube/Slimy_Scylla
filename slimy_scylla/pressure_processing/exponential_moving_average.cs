using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Pressure Processing Exponential Moving Average")]
public sealed class slimy_scylla_pressure_processing_exponential_moving_average : slimy_scylla_base
{
    private uint last_pressure = 0;
    private Vector2 last_pos = new Vector2();
    private int tail_reports = 0;

    private uint exponential_moving_average(uint pressure) {
        //scale the smoothing amount by x^(0.02/x) to make the difference between different settings feels better
        //the perceptible difference in smoothing between settings when using an unscaled linear smoothing amount is not linear
        //when scaling exponentially, users should feel the difference between, for example, 0.2 and 0.4 or 0.6 and 0.8 much better
        float scaled_amount = (float)Math.Pow(amount, 0.02 / amount);
        if (last_pressure != 0) {
            pressure = (uint)((float)pressure * (1 - scaled_amount) + (float)last_pressure * scaled_amount);
        }

        last_pressure = pressure;
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
                last_pressure = 0;
                report.Pressure = 0;
                Emit?.Invoke(device_report);
                return;
            }
            
            report.Pressure = exponential_moving_average(report.Pressure);
            last_pos = report.Position;
            tail_reports = remove_tail_pressure_reports;
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Amount"), DefaultPropertyValue(0.1f), ToolTip
        ("Amount: Min: 0.00, Max: 1.00, Default: 0.10\n" +
        "Changes the amount of smoothing applied. The higher the value within the min and max, the higher the smoothing.")]
    public float amount { set; get; }

    [Property("Pressure Deadzone"), Unit("%"), ToolTip
        ("Pressure Deadzone: Min: 0%, Max: 100%, Default: 0%\n" +
        "Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.")]
    public float pressure_deadzone_percent { set; get; }

    [Property("Remove Tail Pressure Reports"), DefaultPropertyValue(0), ToolTip
        ("Remove Tail Pressure Reports: Min: 0, Max: 10, Default: 0\n" +
        "Stops drawing programs from adding their own smoothing at the end of lines which commonly creates \"shoelace line endings\" or \"line tails\".\n" +
        "Usually setting this to 1 is enough for it function properly. Only increase the value if required.")]
    public int remove_tail_pressure_reports { set; get; }
}