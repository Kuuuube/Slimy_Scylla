using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Input Degredation Jitter")]
public sealed class slimy_scylla_input_degredation_jitter : slimy_scylla_base
{
    private int seed = 0;
    private Vector2 start_point = new Vector2();

    //noise function taken from:
    //https://www.desmos.com/calculator/wb1yoxqkd4
    //https://twitter.com/MAKIO135/status/1110213004807860224
    private double fract(double x) {
        return x - Math.Floor(x);
    }

    private double rand(double x) {
        return fract(999 * Math.Sin(x));
    }

    private double rand_step(double x) {
        return rand(Math.Floor(x));
    }

    private double smooth_step(double x) {
        return Math.Pow(x, 2) * (1 - 2 * (x - 1));
    }

    private double smooth_saw(double x) {
        return smooth_step(fract(x));
    }

    private double noise(float x) {
        return rand_step(x) * smooth_saw(x) * rand_step(x) * (1 - smooth_saw(x - 1));
    }

    private float mix(double a, double b, double x) {
        return (float)((b - a) * x + a);
    }

    private Vector3 jitter(Vector2 position, uint pressure) {
        if (start_point == new Vector2()) {
            start_point = position;
        }
        if (seed == 0) {
            seed = new Random().Next(1, 10000);
        }

        Vector2 lppx = lines_per_pixel();
        Vector2 position_px = new Vector2(position.X / lppx.X, position.Y / lppx.Y);
        Vector2 start_point_px = new Vector2(start_point.X / lppx.X, start_point.Y / lppx.Y);
        float distance_px = (float)Math.Sqrt((position_px.X - start_point_px.X) * (position_px.X - start_point_px.X) + (position_px.Y - start_point_px.Y) * (position_px.Y - start_point_px.Y));
        
        float ss = distance_px + seed;
        float pressure_noise = mix(pressure_noise_amount, pressure_noise_amount * 1.5, (float)pressure / (float)get_max_pressure());
        pressure = (uint)((((float)pressure / (float)get_max_pressure()) + pressure_noise * (Math.Sin(ss / pressure_noise_period) + noise(ss / pressure_noise_period) * 2 - 1)) * (float)get_max_pressure());
        float position_noise = (float)(position_noise_amount * (Math.Sin(ss / position_noise_period) + noise(ss / position_noise_period) * 2 - 1));
        Vector2 normalized_position = position / get_max_coords();
        position = new Vector2((position_px.X + ((normalized_position.X * 2 - 1) * position_noise)) * lppx.X, (position_px.Y + ((normalized_position.Y * 2 - 1) * position_noise)) * lppx.Y);
        
        if (pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
            pressure = (uint)(pressure_deadzone_percent / 100 * get_max_pressure() + 1);
        }
        return new Vector3(position, pressure);
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                seed = 0;
                start_point = new Vector2();
                report.Pressure = 0;
                Emit?.Invoke(device_report);
                return;
            }
            Vector3 get_jitter = jitter(report.Position, report.Pressure);
            report.Position = new Vector2(get_jitter.X, get_jitter.Y);
            report.Pressure = (uint)get_jitter.Z;
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Pressure Noise Amount"), DefaultPropertyValue(0.05f)]
    public float pressure_noise_amount { set; get; }

    [Property("Pressure Noise Period"), DefaultPropertyValue(10f)]
    public float pressure_noise_period { set; get; }

    [Property("Position Noise Amount"), DefaultPropertyValue(7f)]
    public float position_noise_amount { set; get; }

    [Property("Position Noise Period"), DefaultPropertyValue(15f)]
    public float position_noise_period { set; get; }

    [Property("Pressure Deadzone"), Unit("%")]
    public float pressure_deadzone_percent { set; get; }
}