# Position Smoothing

## Moving Average

Applies smoothing by averaging a set amount of recent reports.

**Amount:** Min: 2, Max: 200, Default: 10

The number of reports to average. The higher the value, the higher the smoothing.

**Catch Up:** Min: False, Max: True, Default: True

When true, the current position is always updated. When false, the current position is held until it moves at least one pixel.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

**Apply to Hover:** Min: False, Max: True, Default: False

When true, the smoothing is applied while hovering. When false, smoothing is turned off while hovering.

**Remove Tail Position Reports:** Min: 0, Max: 10, Default: 1

Stops drawing programs from adding their own smoothing at the end of lines which commonly creates "shoelace line endings" or "line tails". 

Usually setting this to 1 is enough for it function properly. Only increase the value if required.

**Never Intercept Pressure on/off:** Min: False, Max: True, Default: False

When true, while Catch Up is true, pressure on/off will send even if the position has moved less than one pixel. When false, while Catch Up is true, pressure will wait to send until movement is detected. 

This setting does not apply when Catch Up is false.

## Exponential Moving Average

Reacts quicker to changes than Moving Average while retaining smoothness.

**Amount:** Min: 0.00, Max: 1.00, Default: 0.10

Changes the amount of smoothing applied. The higher the value within the min and max, the higher the smoothing.

**Catch Up:** Min: False, Max: True, Default: True

When true, the current position is always updated. When false, the current position is held until it moves at least one pixel.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

**Apply to Hover:** Min: False, Max: True, Default: False

When true, the smoothing is applied while hovering. When false, smoothing is turned off while hovering.

**Remove Tail Position Reports:** Min: 0, Max: 10, Default: 1

Stops drawing programs from adding their own smoothing at the end of lines which commonly creates "shoelace line endings" or "line tails". 

Usually setting this to 1 is enough for it function properly. Only increase the value if required.

**Never Intercept Pressure on/off:** Min: False, Max: True, Default: False

When true, while Catch Up is true, pressure on/off will send even if the position has moved less than one pixel. When false, while Catch Up is true, pressure will wait to send until movement is detected.

This setting does not apply when Catch Up is false.

## Pulled String

Creates a circular deadzone around the position and when leaving the deadzone, the positon is pulled in the direction of movement.

**String Length:** Min: 0, Max: 200, Default: 35

The radius of the circular deadzone in pixels.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

**Apply to Hover:** Min: False, Max: True, Default: False

When true, the smoothing is applied while hovering. When false, smoothing is turned off while hovering.

**Remove Tail Position Reports:** Min: 0, Max: 10, Default: 1

Stops drawing programs from adding their own smoothing at the end of lines which commonly creates "shoelace line endings" or "line tails". 

Using this on Pulled String is important as you may commonly finish lines at a different position than the real position. A sudden position change when transitioning to hover can cause unintended lines.

Usually setting this to 1 is enough for it function properly. Only increase the value if required.

**Never Intercept Pressure on/off:** Min: False, Max: True, Default: False

When true, pressure on/off will send even if the position has moved less than one pixel. When false, pressure will wait to send until movement is detected.