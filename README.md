# Trysac.BlazorDashboardKit

Teams building internal dashboards in Blazor often ship charts that screen readers ignore, colours that fail contrast, and meaningless axis ticks. This library provides a dependency-free, accessible SVG chart renderer that enforces WCAG 2.2 contrast, computes human-readable axis ticks, provides HTML table fallbacks, and filters datasets without JavaScript interop.

## Installation

Add the `Trysac.BlazorDashboardKit` NuGet package to your .NET 10+ project.

## Usage

```csharp
using Trysac.BlazorDashboardKit;

var spec = new ChartSpec(
    Title: "Monthly sales",
    Description: "Sales revenue for Q1 2026",
    Categories: new[] { "Jan", "Feb", "Mar" },
    Series: new[] { new Series("2026", new[] { 120.0, 80.0, 150.0 }) },
    Kind: ChartKind.Bar);
string svg = ChartRenderer.RenderSvg(spec);
```

## API Summary

- `ChartRenderer.RenderSvg(ChartSpec, Theme?)`: Renders an accessible `<svg>` element with `role="img"`, ARIA titles/descriptions, and data markers.
- `ChartRenderer.RenderTable(ChartSpec)`: Renders an HTML `<table>` with appropriate row and column scopes.
- `Contrast.Ratio(string, string)`: Calculates WCAG 2.2 contrast ratios between hex colors.
- `NiceTicks.Compute(double, double, int)`: Generates human-readable axis tick values.
- `Filters.Apply(IReadOnlyList<...>, IEnumerable<Filter>)`: Filters datasets with `Filter.Equal`, `Filter.Range`, and `Filter.In` predicates.
- `DashboardException`: Thrown on validation errors (e.g., `invalid_color`, `low_contrast`, `too_many_series`, `too_many_points`, `invalid_id`).

## Limits and Options

- **Max Points**: 10,000 per chart.
- **Max Series**: 5 per chart.
- **Max Rows**: 10,000 for filter operations.
- **Colors**: Must be `#RRGGBB` format.
- **ID Prefix**: Must match `[a-z][a-z0-9-]{0,31}`.

## Why not Chart.js or Recharts?

Existing suites rely on large JavaScript interop packages, which often treat accessibility as an afterthought. This library generates deterministic, pure SVG server-side, ensuring compliance with accessibility standards by design rather than configuration.

## Security

See [docs/SECURITY.md](docs/SECURITY.md) for the threat model and input validation limits.