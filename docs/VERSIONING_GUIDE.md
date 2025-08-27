# Versioning Guide

DeepComparer follows **Semantic Versioning (SemVer 2.0.0)**.

## Version Number Format

MAJOR.MINOR.PATCH

### MAJOR

- Incremented when **backward-incompatible changes** are introduced.
- Examples:
  - Removing or renaming public API methods.
  - Changing method signatures in a breaking way.
  - Modifying default behavior significantly.

### MINOR

- Incremented when **new features** are added in a backward-compatible manner.
- Examples:
  - Adding a new overload of `CompareProperties`.
  - Introducing new configuration options to `CompareOptions`.
  - Enhancing performance without altering API behavior.

### PATCH

- Incremented when **backward-compatible bug fixes** are released.
- Examples:
  - Fixing a bug in collection comparison.
  - Resolving edge cases in circular reference detection.
  - Documentation updates (if released as a package version bump).

---

## Example

- **1.0.0** – Initial stable release.
- **1.1.0** – Added support for custom equality maps.
- **2.0.0** – Refactored API to support async comparison (breaking change).

---

## Pre-release Versions
- Tagged with suffixes like:
  - `-alpha` (early testing),
  - `-beta` (feature-complete but possibly unstable),
  - `-rc` (release candidate, close to stable).

Example:

```
2.0.0-beta.1
```

---

## Checklist for Updating Version
1. Determine change type: MAJOR, MINOR, PATCH.
2. Update version in:
   - `DeepComparer.csproj`
   - `DeepComparerAnalyzer.csproj`
   - `CHANGELOG.md` (if maintained separately).
3. Commit with message:

```
chore: bump version to x.y.z
```

1. 4. Tag release:

```bash
git tag vX.Y.Z
git push origin vX.Y.Z
```
## Automation

GitHub Actions workflow publish-nuget.yml will:

- Build and run tests.

- Pack NuGet package.

- Push to NuGet.org using version from .csproj.