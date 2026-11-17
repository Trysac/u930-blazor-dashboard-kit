# Security & Threat Model

## Defended Threats

- **XSS**: Every user-supplied text field (`Title`, `Description`, category labels, series names) is sanitized via `WebUtility.HtmlEncode` before being written into the SVG or HTML output.
- **Input Injection**: `IdPrefix` fields are validated against the regular expression `^[a-z][a-z0-9-]{0,31}$`. Color strings are validated against `#RRGGBB` format using explicit hex byte parsing. Invalid inputs throw a `DashboardException`.

## Limits

- **Data Volume**: `ChartRenderer` limits charts to `10,000` total data points (categories * series) and `5` series maximum. `Filters.Apply` limits operations to `10,000` rows. These limits prevent resource exhaustion.
- **Tick Generation**: Tick count is strictly limited to `2..20`.
- **Exception Length**: Error messages are truncated to 200 characters to prevent log flooding.