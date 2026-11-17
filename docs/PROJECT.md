# Project record

## Objective

Render dashboard bar and line charts as accessible SVG with WCAG 2.2 contrast checks, nice axis ticks, data table fallbacks, and typed filters, optimized for Blazor server-side rendering.

## Scope

- In scope: Implementation of accessible SVG charts, HTML table fallbacks, WCAG contrast verification, Paul Heckbert's nice tick algorithm, and a typed filtering engine.
- Out of scope: JavaScript interop, interactivity (tooltips, zooms), pie/area charts, and responsive resizing logic.

## Effort metrics

| Metric | Value |
|---|---|
| Tokens (input / output) | 1289320 / 26928 |
| Hours (human) | pending measurement |
| Cost (USD) | 0.7451 |

## Factory run u930 (measured from API usage)

| Date (UTC) | Phase | Model | Batch | Input | Output | Cache write | Cache read | Cost (USD) |
|---|---|---|---|---|---|---|---|---|
| 2026-11-17 | generation | google/gemini-4-argon | yes | 6166 | 21219 | 0 | 0 | 0.4490 |
| 2026-11-17 | fix-2 | google/gemini-3.8-flash | no | 82162 | 2169 | 0 | 327511 | 0.0943 |
| 2026-11-17 | audit | google/gemini-3.8-flash | no | 84121 | 886 | 0 | 465645 | 0.1013 |
| 2026-11-17 | threat-model | google/gemini-3.8-flash | no | 98670 | 656 | 0 | 201752 | 0.0916 |
| 2026-11-17 | docs | google/gemini-3.1-flash-lite | no | 23293 | 1998 | 0 | 0 | 0.0088 |

Total: $0.7451. Prices: vertex line, as measured by its adapter. Providers serve the current version of each model id, so the date tells which version answered; versions, launch stages and price sources per line are in the factory's model register (docs/MODELS.md).
