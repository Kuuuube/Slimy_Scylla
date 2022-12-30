using System;
using System.Numerics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Tablet;
using OpenTabletDriver.Plugin.Output;

namespace slimy_scylla;

[PluginName("Slimy Scylla Position Smoothing Moving Average")]
public sealed class slimy_scylla_position_smoothing_moving_average : IPositionedPipelineElement<IDeviceReport>
{
    private bool emit = true;
    private List<Vector2> last_positions = new List<Vector2>();

    private Vector2 moving_average_catch_up(Vector2 report) {
        last_positions.Add(report);
        while (last_positions.Count > amount) {
            last_positions.RemoveAt(0);
        }

        if (last_positions.Count == amount) {
            Vector2 total = new Vector2();
            foreach (Vector2 position in last_positions) {
                total.X += position.X;
                total.Y += position.Y;
            }

            report.X = total.X / (float)amount;
            report.Y = total.Y / (float)amount;
            return report;
        }

        emit = false;
        return report;
    }

    private Vector2 moving_average(Vector2 report) {
        return report;
    }

    public event Action<IDeviceReport> Emit;
    public void Consume(IDeviceReport device_report)
    {
        if (device_report is ITabletReport report) {
            if (!apply_to_hover && report.Pressure <= pressure_deadzone) {
                last_positions = new List<Vector2>();
                Emit?.Invoke(device_report);
                return;
            }

            if (catch_up) {
                report.Position = moving_average_catch_up(report.Position);
            } else {
                report.Position = moving_average(report.Position);
            }
            device_report = report;
        }

        if (emit) {
            Emit?.Invoke(device_report);
        } else {
            emit = true;
        }
    }
    public PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Amount"), DefaultPropertyValue(2)]
    public int amount { set; get; }

    [BooleanProperty("Catch Up", "")]
    public bool catch_up { set; get; }

    [Property("Pressure Deadzone")]
    public int pressure_deadzone { set; get; }

    [BooleanProperty("Apply to Hover", "")]
    public bool apply_to_hover { set; get; }
}