This tool will generate TC dependency tree for project

# Usage

dotnet run <path to repository root> <path to csproj file>

# Example

```
dotnet run c:\src\wb C:\src\wb\src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj

+:C:\src\wb\src\Core\BoundedContexts\Interviewer\WB.Core.BoundedContexts.Interviewer\**
+:C:\src\wb\src\Core\GenericSubdomains\WB.Core.GenericSubdomains.Portable\**
+:C:\src\wb\src\Core\Infrastructure\WB.Core.Infrastructure\**
+:C:\src\wb\src\Core\SharedKernels\DataCollection\DataCollection.Portable\**
+:C:\src\wb\src\Core\SharedKernels\DataCollection\DataCollection\**
+:C:\src\wb\src\Core\SharedKernels\Enumerator\Enumerator\**
+:C:\src\wb\src\Core\SharedKernels\Questionnaire\WB.Core.SharedKernels.Questionnaire\**
+:C:\src\wb\src\UI\Interviewer\WB.UI.Interviewer\**
+:C:\src\wb\src\UI\Shared\WB.UI.Shared.Enumerator\**
+:C:\src\wb\src\UI\Shared\WB.UI.Shared.Extensions\**
```
