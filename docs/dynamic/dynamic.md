# Dynamic

## Weighted

Converts speed into pressure.

**Drag:** Min: 0.10, Max: 0.90, Default: 0.20

Adds drag to the position movement.

**Mass:** Min: 0.00, Max: 50.00, Default: 31.25

Increases or decreases the feeling of inertia in position movement.

**Max Pressure Speed:** Min: 1.00, Max: 50.00, Default: 20.00

The speed at which maximum pressure will be sent.

**Pressure Deadzone:** Min: 0%, Max: 100%, Default: 0%

Adds a pressure deadzone at the set pressure percent. Match this value to your Tip Threshold in the Pen Settings tab.

**Remove Tail Pressure Reports:** Min: 0, Max: 10, Default: 1

Stops drawing programs from adding their own smoothing at the end of lines which commonly creates "shoelace line endings" or "line tails".

This may cause lines to not taper properly if you release while moving quickly. Keep your pen down for a second after ending a line to get a clean tapered end.

Usually setting this to 1 is enough for it function properly. Only increase the value if required.

## Speed Smooth

Applies smoothing only when moving slowly.

**Min Smooth Speed:** Min: 0, Max: 1000, Default: 0

The minimum speed where smoothing is applied.

**Max Smooth Speed:** Min: 0, Max: 1000, Default: 55

The maximum speed where smoothing is applied.

**Smooth Amount:** Min: 0.00, Max: 0.98, Default: 0.98

The amount of smoothing to apply.

**Inertia Accel:** Min: 0.00, Max: 0.95, Default: 0.50

Scales the amount of inertia present when accelerating in speed.

**Inertia Decel:** Min: 0.00, Max: 0.95, Default: 0.85

Scales the amount of inertia present when decelerating in speed.