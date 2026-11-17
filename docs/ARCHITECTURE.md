# Architecture

## Overview

`Trysac.BlazorDashboardKit` is a dependency-free .NET class library designed for pure server-side rendering of accessible data visualizations. It provides deterministic SVG output that avoids JavaScript interop, making it ideal for Blazor markup bindings.

## Core Modules

- **Configuration**: `ChartSpec`, `Series`, `ChartKind`, and `Theme` records define the immutable state of a chart.
- **Rendering**: `ChartRenderer` handles the logic for converting specs into deterministic SVG strings and HTML tables. It uses `0.##` number formatting with `CultureInfo.InvariantCulture`.
- **Calculation**: `Contrast` computes WCAG 2.2 relative luminance; `NiceTicks` implements Heckbert's *Graphics Gems* (1990) algorithm.
- **Data**: `Filters` and `Filter` evaluate typed predicates across collections of rows.
- **Exceptions**: `DashboardException` encapsulates validation failures with stable machine-readable codes.