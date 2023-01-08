using System.Numerics;
using OpenTabletDriver;
using OpenTabletDriver.Plugin;
using OpenTabletDriver.Plugin.DependencyInjection;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace slimy_scylla;

public abstract class slimy_scylla_base : IPositionedPipelineElement<IDeviceReport>
{
    public Vector2 lines_per_pixel() {
        if (output_mode_type == OutputModeType.absolute && absolute_output_mode != null) {
            float digitizer_width = absolute_output_mode.Tablet.Properties.Specifications.Digitizer.Width;
            float digitizer_height = absolute_output_mode.Tablet.Properties.Specifications.Digitizer.Height;
            float digitizer_max_x = absolute_output_mode.Tablet.Properties.Specifications.Digitizer.MaxX;
            float digitizer_max_y = absolute_output_mode.Tablet.Properties.Specifications.Digitizer.MaxY;
            Vector2 lpmm = new Vector2(digitizer_max_x / digitizer_width, digitizer_max_y / digitizer_height);

            float area_width = absolute_output_mode.Input.Width;
            float area_height = absolute_output_mode.Input.Height;
            float monitor_width = absolute_output_mode.Output.Width;
            float monitor_height = absolute_output_mode.Output.Height;
            Vector2 pxpmm = new Vector2(monitor_width / area_width, monitor_height / area_height);

            return new Vector2(lpmm.X / pxpmm.X, lpmm.Y / pxpmm.Y);
        }

        if (output_mode_type == OutputModeType.relative && relative_output_mode != null) {
            float digitizer_width = relative_output_mode.Tablet.Properties.Specifications.Digitizer.Width;
            float digitizer_height = relative_output_mode.Tablet.Properties.Specifications.Digitizer.Height;
            float digitizer_max_x = relative_output_mode.Tablet.Properties.Specifications.Digitizer.MaxX;
            float digitizer_max_y = relative_output_mode.Tablet.Properties.Specifications.Digitizer.MaxY;
            Vector2 lpmm = new Vector2(digitizer_max_x / digitizer_width, digitizer_max_y / digitizer_height);

            float sens_x = relative_output_mode.Sensitivity.X;
            float sens_y = relative_output_mode.Sensitivity.Y;
            Vector2 pxpmm = new Vector2(sens_x, sens_y);

            return new Vector2(lpmm.X / pxpmm.X, lpmm.Y / pxpmm.Y);
        }

        try_resolve_output_mode();
        return default;
    }

    public uint get_max_pressure() {
        if (output_mode_type == OutputModeType.absolute && absolute_output_mode != null) {
            return absolute_output_mode.Tablet.Properties.Specifications.Pen.MaxPressure;
        }

        if (output_mode_type == OutputModeType.relative && relative_output_mode != null) {
            return relative_output_mode.Tablet.Properties.Specifications.Pen.MaxPressure;
        }

        try_resolve_output_mode();
        return default;
    }

    public Vector2 get_max_coords() {
        if (output_mode_type == OutputModeType.absolute && absolute_output_mode != null) {
            return new Vector2(absolute_output_mode.Tablet.Properties.Specifications.Digitizer.MaxX, absolute_output_mode.Tablet.Properties.Specifications.Digitizer.MaxY);
        }

        if (output_mode_type == OutputModeType.relative && relative_output_mode != null) {
            return new Vector2(relative_output_mode.Tablet.Properties.Specifications.Digitizer.MaxX, relative_output_mode.Tablet.Properties.Specifications.Digitizer.MaxY);
        }

        try_resolve_output_mode();
        return default;
    }

    [Resolved]
    public IDriver driver;
    private OutputModeType output_mode_type;
    private AbsoluteOutputMode absolute_output_mode;
    private RelativeOutputMode relative_output_mode;
    private void try_resolve_output_mode()
    {
        if (driver is Driver drv)
        {
            IOutputMode output = drv.InputDevices
                .Where(dev => dev?.OutputMode?.Elements?.Contains(this) ?? false)
                .Select(dev => dev?.OutputMode).FirstOrDefault();

            if (output is AbsoluteOutputMode abs_output) {
                absolute_output_mode = abs_output;
                output_mode_type = OutputModeType.absolute;
                return;
            }
            if (output is RelativeOutputMode rel_output) {
                relative_output_mode = rel_output;
                output_mode_type = OutputModeType.relative;
                return;
            }
            output_mode_type = OutputModeType.unknown;
        }
    }

    public abstract event Action<IDeviceReport> Emit;
    public abstract void Consume(IDeviceReport value);
    public abstract PipelinePosition Position { get; }
}
enum OutputModeType {
    absolute,
    relative,
    unknown
}