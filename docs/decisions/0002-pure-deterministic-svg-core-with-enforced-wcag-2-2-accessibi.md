# Pure Deterministic SVG Core with Enforced WCAG 2.2 Accessibility

Status: accepted (generated, pending human review)

## Context

Blazor dashboard teams need accessible charts, readable axis ticks, and dataset filters without heavy JavaScript interop libraries or browser-dependent rendering that cannot be verified in unit tests.

## Decision

Implement a pure standard-library C# rendering core that validates WCAG 2.2 contrast ratios (4.5:1 for text, 3.0:1 for series), computes Heckbert nice axis ticks, encodes all labels via WebUtility.HtmlEncode, formats numbers with CultureInfo.InvariantCulture, and distinguishes line series with both color and stroke-dasharray patterns.

## Consequences

Charts and fallback HTML tables are 100% deterministic strings that Blazor components can emit with MarkupString; inaccessible palettes and malformed inputs fail fast with typed DashboardException codes.
