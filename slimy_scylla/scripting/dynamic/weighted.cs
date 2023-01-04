using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Dynamic Weighted")]
public sealed class slimy_scylla_dynamic_weighted : slimy_scylla_base
{
    private Vector2 last_position = new Vector2();
    private Vector2 last_smoothed_position = new Vector2();
    private uint last_pressure = 0;
    private Vector2 v = new Vector2();
    private int tail_reports = 0;

    private float smooth_step(double a, double b, double x) {
        //emulates lazy nezumi's smoothStep without cubic interpolation
        //finds the position of x between a and b as a value within 0 to 1
        return (float)Math.Clamp(((x - a) / (b - a)), 0, 1);
    }

    private Vector3 weighted(Vector2 position) {
        if (last_position == new Vector2()) {
            last_position = position / lines_per_pixel();
            return new Vector3(position, 0);
        }

        position = new Vector2(position.X / lines_per_pixel().X, position.Y / lines_per_pixel().Y);
        Vector2 f = new Vector2(position.X - last_position.X, position.Y - last_position.Y);
        Vector2 a = new Vector2(f.X / mass, f.Y / mass);
        v = new Vector2((v.X + a.X) * (1 - drag), (v.Y + a.Y) * (1 - drag));
        double vs = Math.Sqrt(v.X * v.X + v.Y * v.Y);

        position = new Vector2(last_position.X + v.X, last_position.Y + v.Y);
        last_position = new Vector2(position.X, position.Y);
        float pressure = smooth_step(1, max_pressure_speed, vs);

        Vector3 position_pressure = new Vector3(position.X * lines_per_pixel().X, position.Y * lines_per_pixel().Y, pressure * get_max_pressure());

        //pressure must not go above the deadzone or the pen tip may release
        if (position_pressure.Z <= pressure_deadzone_percent / 100 * get_max_pressure()) {
            position_pressure.Z = pressure_deadzone_percent / 100 * get_max_pressure() + 1;
        }
        return position_pressure;
    }

    public override event Action<IDeviceReport>? Emit;
    public override void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (report.Pressure <= pressure_deadzone_percent / 100 * get_max_pressure()) {
                v = new Vector2();
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

            Vector3 position_pressure = weighted(report.Position);
            report.Position = new Vector2(position_pressure.X, position_pressure.Y);
            report.Pressure = (uint)position_pressure.Z;
            tail_reports = remove_tail_pressure_reports;
            device_report = report;
        }

        Emit?.Invoke(device_report);
    }
    public override PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Drag"), DefaultPropertyValue(0.2f)]
    public float drag { set; get; }

    [Property("Mass"), DefaultPropertyValue(31.25f)]
    public float mass { set; get; }

    [Property("Max Pressure Speed"), DefaultPropertyValue(20f)]
    public float max_pressure_speed { set; get; }

    [Property("Pressure Deadzone"), Unit("%")]
    public float pressure_deadzone_percent { set; get; }

    [Property("Remove Tail Pressure Reports")]
    public int remove_tail_pressure_reports { set; get; }
}