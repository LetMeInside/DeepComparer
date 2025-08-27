# Contributing to DeepComparer

Thank you for your interest in contributing to **DeepComparer**!  
We welcome bug reports, feature requests, performance improvements, and documentation updates.

---

## Code of Conduct
Please follow respectful and constructive communication in all interactions.

---

## How to Contribute

### 1. Fork and Clone
```bash
git clone https://github.com/yourusername/DeepComparer.git
cd DeepComparer
```

### 2. Create a Branch

```bash
git checkout -b feature/my-feature
```

### 3. Implement Changes

- Follow .NET 9 coding standards.

- Ensure unit tests are added for new features.

### 4. Run Tests

```bash
dotnet test
```

### 5. Submit Pull Request

- Push your branch:
```bash
git push origin feature/my-feature
```

- Open a pull request on GitHub against main.

---

# Development Guidelines
## Coding Standards

- Use var only when type is obvious.

- Keep methods under 50 lines where possible.

- Favor expression-bodied members for trivial properties.

## Performance

- Cache reflection results where possible.

- Avoid allocations in tight loops.

- Use ConcurrentDictionary for thread-safe caching.

## Testing

- Unit tests use xUnit.

- All tests must pass on Windows, Linux, and macOS (via GitHub Actions).

- Add benchmarks for performance-sensitive changes (see benchmarks.md).

## Documentation

- Update README.md for new features.

- Add examples in REACTIVEUI_EXAMPLES.md where relevant.

## Licensing

- All contributions are licensed under the MIT License.

---

## Questions or Suggestions?

Please open an issue on GitHub or start a discussion.


