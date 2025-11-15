## Battlefield Baseline Snapshot

- Camera frustum: `left=-7`, `right=7`, `top=5.5`, `bottom=-5.5`, `zNear=0.1`, `zFar=100`.
- Camera transform: position `(0, 3, 12)`, lookAt `(0, 0.5, 0)`.
- Ground plane: size `60x60`, y offset `-1.5`.
- Thrall spawn:
  - Position `(-3.2, -0.8, 0)`, velocity oscillation bounds `±3.5` on X.
  - Mesh stack: body height `2.6`, head height `1`, emissive eyes at `y=1.9`.
- Horde spawn:
  - Base position `(2.2 + offset, -0.6 + height, 0)` where `offset ∈ [-0.5, 3.5]`, `height ∈ [0, 0.8]`.
  - Default radius `0.6`, elite emissive 1.2 vs fodder 0.6.

Use this reference when adjusting visuals so regressions are easy to spot.
