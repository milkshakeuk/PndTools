---
title: Coverage
description: Code coverage results for PndTools.
---

Coverage results are generated automatically by <https://github.com/coverlet-coverage/coverlet>
and <https://reportgenerator.io> as part of the CI build and published here on each push to master.

To run tests with coverage locally:

```bash
dotnet test test/PndTools.Tests/PndTools.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --results-directory coverage

dotnet reportgenerator \
  -reports:coverage/**/coverage.cobertura.xml \
  -targetdir:coverage/report \
  -reporttypes:Html
```

The HTML report is written to `coverage/report/index.html`.
