using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Tilt Smoothing Exponential Moving Average")]
public sealed class slimy_scylla_tilt_smoothing_exponential_moving_average : slimy_scylla_base
{
    private Vector2 last_tilt = new Vector2();

    private Vector2 exponential_moving_average(Vector2 tilt) {
        //scale the smoothing amount by x^(0.02/x) to make the difference between different settings feels better
        //the perceptible difference in smoothing between settings when using an unscaled linear smoothing amount is not linear
        //when scaling exponentially, users should feel the difference between, for example, 0.2 and 0.4 or 0.6 and 0.8 much better
        float scaled_amount = (float)Math.Pow(amount, 0.02 / amount);
        if (last_tilt != new Vector2()) {
            tilt = new Vector2(tilt.X * (1 - scaled_amount) + last_tilt.X * scaled_amount, tilt.Y * (1 - scaled_amount) + last_tilt.Y * scaled_amount);
        }

        last_tilt = tilt;
        return tilt;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITiltReport report) {
            report.Tilt = exponential_moving_average(report.Tilt);
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Amount"), DefaultPropertyValue(0.1f), ToolTip
        ("Amount: Min: 0.00, Max: 1.00, Default: 0.10\n" +
        "Changes the amount of smoothing applied. The higher the value within the min and max, the higher the smoothing.")]
    public float amount { set; get; }
}