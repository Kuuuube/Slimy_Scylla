# Input Degradation

## Jitter

Adds somewhat controlled noise to position and pressure.

**Pressure Noise Amount:** Min: 0.00, Max: 0.50, Default: 0.05

The variability in pressure per report. It is recommended to keep this value very low.

**Pressure Noise Period:** Min: 0.01, Max: 50.00, Default: 10.00

The variability in pressure over time.

**Position Noise Amount:** Min: 0.01, Max: 40.00, Default: 7.00

The variability in position per report.

**Position Noise Period:** Min: 0.00, Max: 10.00, Default: 15.00

The variability in position over time.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

## Pressure Random

Randomizes pressure based on settings.

**Amount:** Min: 0%, Max: 100%, Default: 10%

The maximum amount of pressure change added or subtracted to the current pressure percent.

**Completely Random Pressure:** Min: False, Max: True, Default: False

When true, ignores the current pressure and apply a completely randomized value.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

## Quantize

Restricts the position to a grid.

**X Grid:** Min: 0, Max: 100, Default: 10

The smallest step in pixels the position can move in the X axis.

**Y Grid:** Min: 0, Max: 100, Default: 20

The smallest step in pixels the position can move in the Y axis.

**Scale:** Min: 0.00, Max: 2.00, Default: 1.00

Multiplier for the size of the grid in both axes.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

**Apply to Hover:** Min: False, Max: True, Default: False

When true, the filter is applied while hovering. When false, filter is turned off while hovering.