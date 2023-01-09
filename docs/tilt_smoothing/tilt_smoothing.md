# Tilt Smoothing

## Moving Average

Applies smoothing by averaging a set amount of recent reports.

**Amount:** Min: 2, Max: 200, Default: 10

The number of reports to average. The higher the value, the higher the smoothing.

## Exponential Moving Average

Similar to Moving Average but changes the weight of current and past reports. This gives it a slightly different feeling and easily allows for much stronger smoothing is desired.

**Amount:** Min: 0.00, Max: 1.00, Default: 0.10

Changes the amount of smoothing applied. The higher the value within the min and max, the higher the smoothing.