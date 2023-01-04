using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Dynamic Speed Smooth")]
public sealed class slimy_scylla_dynamic_speed_smooth : slimy_scylla_base
{
    private Vector2 last_position = new Vector2();
    private Vector2 last_smoothed_position = new Vector2();
    private uint last_pressure = 0;
    private float ss = 0;
    private int tail_reports = 0;

    private float mix(double a, double b, double x) {
        return (float)((b - a) * x + a);
    }

    private float smooth_step(double a, double b, double x) {
        //emulates lazy nezumi's smoothStep without cubic interpolation
        //finds the position of x between a and b as a value within 0 to 1
        return (float)Math.Clamp(((x - a) / (b - a)), 0, 1);
    }

    private Vector2 speed_smooth(Vector2 position) {
        if (last_position == new Vector2()) {
            last_position = position / lines_per_pixel();
            return position;
        }

        position = new Vector2(position.X / lines_per_pixel().X, position.Y / lines_per_pixel().Y);

        Vector2 d = new Vector2(position.X - last_position.X, position.Y - last_position.Y);
        float accel_decel;
        float s = (float)Math.Sqrt((position.X-last_position.X)*(position.X-last_position.X)+(position.Y-last_position.Y)*(position.Y-last_position.Y));
        if (s > ss) {
            accel_decel = inertia_accel;
        } else {
            accel_decel = inertia_decel;
        }
        ss = mix(s, ss, accel_decel);
        float g = 1 - smooth_amount * smooth_step(min_smooth_speed, max_smooth_speed, ss);
        position = new Vector2(last_position.X + d.X * g, last_position.Y + d.Y * g);

        last_position = new Vector2(position.X, position.Y);

        position = new Vector2(position.X * lines_per_pixel().X, position.Y * lines_per_pixel().Y);

        return position;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                ss = 0;
                if (tail_reports > 0) {
                    report.Position = last_position * lines_per_pixel();
                    tail_reports--;
                }
                if (tail_reports <= 0) {
                    last_position = new Vector2();
                }
                last_pressure = 0;
                report.Pressure = 0;
                Emit?.Invoke(device_report);
                return;
            }

            report.Position = speed_smooth(report.Position);
            tail_reports = remove_tail_pressure_reports;
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Min Smooth Speed"), DefaultPropertyValue(55)]
    public int min_smooth_speed { set; get; }

    [Property("Max Smooth Speed"), DefaultPropertyValue(0)]
    public int max_smooth_speed { set; get; }

    [Property("Smooth Amount"), DefaultPropertyValue(0.98f)]
    public float smooth_amount { set; get; }

    [Property("Inertial Accel"), DefaultPropertyValue(0.5f)]
    public float inertia_accel { set; get; }

    [Property("Inertia Decel"), DefaultPropertyValue(0.85f)]
    public float inertia_decel { set; get; }

    [Property("Pressure Deadzone"), Unit("%")]
    public float pressure_deadzone_percent { set; get; }

    [Property("Remove Tail Pressure Reports")]
    public int remove_tail_pressure_reports { set; get; }
}