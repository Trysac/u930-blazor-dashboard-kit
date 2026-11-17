# Testing

## Test Coverage

- `DashboardPropertiesTests.cs`: Property-based tests using `FsCheck` verifying invariants for `Contrast.Ratio` (symmetry, bounds), `NiceTicks.Compute` (increasing order, encloses range), and determinism of `ChartRenderer.RenderSvg`.
- `DashboardTests.cs`: Unit and acceptance tests covering:
  - Accessibility: `RenderSvg` produces role="img", valid `aria-labelledby`, and correct titles.
  - Contrast: `Contrast.Ratio` calculations and rejection of `low_contrast` themes.
  - NiceTicks: Correct output for edge cases and input validation.
  - Filters: Correct application of equality, range, and membership predicates; validation of row counts.
  - Rendering: HTML table correctness, SVG escaping, culture-invariant number formatting.
  - Validation: Exhaustive verification of `DashboardException` codes (`length_mismatch`, `invalid_value`, `too_many_points`, `invalid_id`, `too_many_series`).