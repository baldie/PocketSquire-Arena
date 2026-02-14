# Lessons Learned

## TextMeshPro and DOTween
- **Pattern**: `TextMeshProUGUI` does not always have the `DOText` extension method available even if `DG.Tweening` and `TMPro` namespaces are used.
- **Cause**: The DOTween TMPro module must be specifically enabled in the DOTween Setup window, which generates the extension methods. If this hasn't been done, or the generated files are missing, the code will fail to compile.
- **Solution**: Use `DOTween.To(() => textComponent.text, x => textComponent.text = x, targetString, duration)` as a robust alternative. This targets the property directly and doesn't rely on the generated extension module.

## Task Management
- **Pattern**: Skipping plan mode for "simple" tasks.
- **Cause**: Underestimating steps (e.g. usage finding, verification count as steps).
- **Solution**: Always list steps in `tasks/todo.md` first if >3 steps.
