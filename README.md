# Geodetic Adjustment & Numerical Coordinate Transformation Engine

A high-precision, standalone numerical geodesy calculation engine implemented in C# (.NET 8), designed in strict compliance with Turkish National Large-Scale Map Standards (**BÖHYY**, **TUREF**, and **HGM** geodetic specifications).

---

## Key Mathematical Capabilities

### 1. 3D Coordinate Transformations & Ellipsoidal Geometry
* **GRS80 Ellipsoid Parameters:** Implements exact geometric constants:
  * Semi-major axis: $a = 6,378,137.0 \text{ m}$
  * Flattening: $f = 1 / 298.257222101$
  * First eccentricity squared: $e^2 = 0.00669438002290$
* **Closed-Form Cartesian-to-Geodetic Mapping:** Converts Earth-Centered, Earth-Fixed (ECEF) coordinates $(X, Y, Z)$ to geodetic coordinates $(\phi, \lambda, h)$ using Bowring's closed-form formulation, eliminating transcendental iteration divergence while guaranteeing sub-millimeter positioning accuracy.
* **8th-Order Meridian Arc Expansion:** Performs conformal $3^\circ$ Transverse Mercator (Gauss-Krüger) projection calculations with an $e^8$-order series expansion, preserving scale factors ($k_0 = 1.0$) and handling false eastings accurately.

### 2. Tectonic Epoch Transformation via IDW Velocity Interpolation
* **Velocity Vector Interpolation:** For non-continuous observation points, the engine interpolates 3D velocity vectors $(V_x, V_y, V_z)$ from surrounding CORS-TR / TUREF reference stations using an Inverse Distance Weighting (IDW, power parameter $p=2$) algorithm:
  $$V(p) = \frac{\sum_{i=1}^n \frac{V_i}{d_i^2}}{\sum_{i=1}^n \frac{1}{d_i^2}}$$
* **Epoch Shifting:** Shifts spatial coordinates from observation epochs ($T_{\text{meas}}$) to the national static reference epoch ($2005.00$) accounting for Anatolian tectonic plate drift:
  $$\mathbf{X}_{2005.00} = \mathbf{X}_{T} + \mathbf{V} \times (2005.00 - T)$$

### 3. Vertical Datum & Geoid Corrections
* Integrates local hybrid geoid undulation models (**TG09** and **TG20**) to convert geometric ellipsoidal heights ($h$) to physically meaningful orthometric heights ($H$):
  $$H = h - N$$

---

## Build & Run

```bash
dotnet build
dotnet run
```
