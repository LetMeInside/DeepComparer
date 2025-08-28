# Issue Template for DeepComparer

Thank you for taking the time to report a bug or request a feature!  
Please fill out the sections below to help us understand the issue better.

---

## 1. Issue Type
- [ ] Bug Report
- [ ] Feature Request
- [ ] Performance Improvement
- [ ] Documentation Update
- [ ] Other (please specify)

---

## 2. Description
**What happened or what feature would you like to see?**  
Provide a clear and concise description.

---

## 3. Steps to Reproduce (For Bugs)
1. Provide a minimal code sample:

```csharp
   var obj1 = new MyClass { Value = 1 };
   var obj2 = new MyClass { Value = 2 };
   var result = DeepComparer.Compare(obj1, obj2);
```

2. Describe expected vs actual behavior:

- Expected: true

- Actual: false

3. Environment

- OS: Windows / macOS / Linux

- .NET Version: (e.g. .NET 9.0)

- DeepComparer Version: (e.g. 1.0.0)

4. Additional Context

- Links to related issues or pull requests.

- Any performance benchmarks, stack traces, or screenshots.

5. Proposed Solution (Optional)

- If you have an idea for a fix or improvement, please describe it.