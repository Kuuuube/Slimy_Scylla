# Pressure Processing

## Fixed Value

Sets the pressure to a constant value while the pen is pressed.

**Value:** Min: 0.00, Max: 1.00, Default: 0.22

The normalized value between 0 and 1 to set pressure to.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

**Remove Tail Pressure Reports:** Min: 0, Max: 10, Default: 1

Stops drawing programs from adding their own smoothing at the end of lines which commonly creates "shoelace line endings" or "line tails".

Usually setting this to 1 is enough for it function properly. Only increase the value if required.

## Sample and Hold

Locks the pressure at the pressure value present when the number of tablet reports reaches the sample number. Resets when the pen is no longer pressed.

**Sample Number:** Min: 1, Max: 100, Default: 15

The number of samples before locking pressure.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

**Remove Tail Pressure Reports:** Min: 0, Max: 10, Default: 1

Stops drawing programs from adding their own smoothing at the end of lines which commonly creates "shoelace line endings" or "line tails".

Usually setting this to 1 is enough for it function properly. Only increase the value if required.

## Moving Average

Applies smoothing by averaging a set amount of recent reports.

**Amount:** Min: 2, Max: 200, Default: 10

The number of reports to average. The higher the value, the higher the smoothing.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

**Remove Tail Pressure Reports:** Min: 0, Max: 10, Default: 1

Stops drawing programs from adding their own smoothing at the end of lines which commonly creates "shoelace line endings" or "line tails".

Usually setting this to 1 is enough for it function properly. Only increase the value if required.

## Exponential Moving Average

Reacts quicker to changes than Moving Average while retaining smoothness.

**Amount:** Min: 0.00, Max: 1.00, Default: 0.10

Changes the amount of smoothing applied. The higher the value within the min and max, the higher the smoothing.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

**Remove Tail Pressure Reports:** Min: 0, Max: 10, Default: 1

Stops drawing programs from adding their own smoothing at the end of lines which commonly creates "shoelace line endings" or "line tails".

Usually setting this to 1 is enough for it function properly. Only increase the value if required.