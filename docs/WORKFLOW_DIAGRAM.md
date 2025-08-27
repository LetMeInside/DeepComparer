# Workflow Diagram

```mermaid
flowchart TD
    A[Start Comparison] --> B{Objects Null?}
    B -->|Both Null| C[Return Equal]
    B -->|One Null| D[Return Not Equal]
    B -->|Neither Null| E[Check Reference Equality]
    E -->|Same Reference| C
    E -->|Different Reference| F[Check Circular Reference Cache]
    F -->|Already Compared| C
    F -->|Not Compared| G[Add to Cache]
    G --> H{Is Simple Comparable?}
    H -->|Yes| I[Compare via Equality Operator]
    I -->|Equal| J[Return Equal]
    I -->|Not Equal| D
    H -->|No| K{Is Collection?}
    K -->|Yes| L[Compare Collection Count]
    L -->|Counts Differ| D
    L -->|Counts Equal| M[Compare Items Recursively]
    M -->|All Equal| J
    M -->|Any Not Equal| D
    K -->|No| N[Traverse Properties and Fields]
    N -->|Differences Found| D
    N -->|No Differences| J
```

## Explanation

- Circular Reference Cache prevents infinite loops during recursive comparison.

- Simple Comparable check:
  - Built-in .NET value types, strings, enums, or types handled via CustomSimpleTypePredicate.

- Collection Comparison:

  - Compares only the count first (performance optimization).

  - Then performs recursive item comparison (order-insensitive).

## Legend

- Equal Path (C/J): Indicates no differences detected.

- Not Equal Path (D): Indicates at least one difference detected.