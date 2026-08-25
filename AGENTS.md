# Engineering Rules

## General

- Preserve the existing architecture unless explicitly requested otherwise.
- Do not modify unrelated code.
- Prefer simple, maintainable implementations over clever solutions.
- Inspect the existing implementation before changing it.
- Never assume an API contract; verify existing DTOs, services and tests.

## Backend

- Follow the existing ASP.NET Core architecture.
- Keep controllers thin.
- Business rules belong in services.
- Use dependency injection.
- Respect nullable reference types.
- Avoid unnecessary abstractions.
- Do not introduce packages unless necessary.

## Frontend

- Follow the existing project structure.
- Do not duplicate business rules already implemented in the backend.
- Keep API communication inside services.
- Preserve TypeScript type safety.
- Avoid `any`.

## Validation

Before considering a task complete:

1. Build the affected project.
2. Run the relevant automated tests.
3. Fix compilation errors and test failures caused by the change.
4. Review the final diff for unintended modifications.

Do not state that a task is complete merely because the code was written.