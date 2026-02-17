# Lessons Learned

## TextMeshPro and DOTween
- **Pattern**: `TextMeshProUGUI` does not always have the `DOText` extension method available even if `DG.Tweening` and `TMPro` namespaces are used.
- **Cause**: The DOTween TMPro module must be specifically enabled in the DOTween Setup window, which generates the extension methods. If this hasn't been done, or the generated files are missing, the code will fail to compile.
- **Solution**: Use `DOTween.To(() => textComponent.text, x => textComponent.text = x, targetString, duration)` as a robust alternative. This targets the property directly and doesn't rely on the generated extension module.

## Task Management
- **Pattern**: Skipping plan mode for "simple" tasks.
- **Cause**: Underestimating steps (e.g. usage finding, verification count as steps).
- **Solution**: Always list steps in `tasks/todo.md` first if >3 steps.

## Input Handling
- **Pattern**: Using legacy `Input` for buttons/axes (`Input.GetButtonDown`).
- **Cause**: The legacy Input Manager is deprecated and being replaced by the Unity Input System package.
- **Solution**: NEVER use `Input.Get...` or custom string-based managers. Use `UnityEngine.InputSystem` actions (`InputAction.WasPressedThisFrame()`) via a typed wrapper like `GameInput`.

## C# Features
- **Pattern**: Using nullable reference types (`string?`) without enabling the feature.
- **Cause**: The `#nullable enable` context is required for the compiler to correctly interpret `?` as a nullable reference type annotation. Without it, the syntax is valid but misleading or may error depending on language version.
- **Solution**: Always add `#nullable enable` at the top of the file before using nullable reference type annotations.
