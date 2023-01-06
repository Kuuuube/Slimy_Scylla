# Recommended Settings

Various sets of settings and their typical use cases. Listed in order of general usefulness.

## Subtle Smoothing

A small amount of smoothing that wont create much lag or much alteration to lines.

**Position Smoothing Moving Average:**

```
Amount: 7
Catch Up: True
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Apply to Hover: False
Never Intercept Pressure on/off: False
```

**Pressure Processing Moving Average:**

```
Amount: 7
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Remove Tail Pressure Reports: 1
```

## Massive Smoothing

A large amount of smoothing that can make drawing big lines easier.

**Position Smoothing Moving Average:**

```
Amount: 30
Catch Up: True
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Apply to Hover: False
Never Intercept Pressure on/off: False
```

**Pressure Processing Moving Average:**

```
Amount: 25
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Remove Tail Pressure Reports: 1
```

## Pulled String

Helps retain control and eliminate jitter while drawing slowly.

**Position Smoothing Pulled String:**

```
String Length: 40
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Apply to Hover: False
Remove Tail Pressure Reports: 1
Never Intercept Pressure on/off: False
```

**Pressure Processing Moving Average:**

```
Amount: 10
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Remove Tail Pressure Reports: 1
```

## Pressure Gain

A small amount of smoothing and a pressure curve to make reaching higher pressure easier.

**Position Smoothing Moving Average:**

```
Amount: 10
Catch Up: True
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Apply to Hover: False
Never Intercept Pressure on/off: False
```

**Pressure Curve:**

```
Gain: 1.8
Softness: -0.32
Minimum Value: 0
Maximum Value: 100
Invert: False
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
```

**Pressure Processing Moving Average:**

```
Amount: 8
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Remove Tail Pressure Reports: 1
```

## Weighted

Converts speed into pressure. Keep your pen down for a second after ending a line to get a clean tapered end.

**Dynamic Weighted:**

```
Drag: 0.2
Mass: 31.25
Max Pressure Speed: 20
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Remove Tail Pressure Reports: 1
```

## Speed Smooth

Applies smoothing only when moving slowly.

**Dynamic Speed Smooth:**

```
Min Smooth Speed: 0
Max Smooth Speed: 55
Smooth Amount: 0.98
Inertia Accel: 0.5
Inertia Decel: 0.85
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Remove Tail Pressure Reports: 1
```

**Pressure Processing Moving Average:**

```
Amount: 5
Pressure Deadzone: Match your Tip Threshold in the Pen Settings tab.
Remove Tail Pressure Reports: 1
```