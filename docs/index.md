---
title: Page Title
---

Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla et euismod nulla. Curabitur feugiat, tortor non consequat finibus, justo purus auctor massa, nec semper lorem quam in massa.

!!! note "This is a custom title"
    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla et euismod nulla. Curabitur feugiat, tortor non consequat finibus, justo purus auctor massa, nec semper lorem quam in massa.

You can also create one without a title.

!!! note ""
    Lorem ipsum dolor sit amet. No title.

And create one collapsed with nested code.

??? note "Collapsible panel with embedded code"
    Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla et euismod nulla.

    ```csharp
    public static void Main()
    {
        int a = 13;
        ++a;
        Console.WriteLine(a);
    }
    ```

    Curabitur feugiat, tortor non consequat finibus, justo purus auctor massa, nec semper lorem quam in massa.

!!! tip
    See other styles here: https://squidfunk.github.io/mkdocs-material/extensions/admonition/#types

Code formatting and highlights also work.

```csharp hl_lines="3 4"
public static void Main()
{
    int a = 13;
    ++a;
    Console.WriteLine(a);
}
```
