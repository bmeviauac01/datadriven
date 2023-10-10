# Install Visual Studio & .NET SDK

In some of the exercises require Microsoft Visual Studio **version 2022**. The free [Community edition](https://visualstudio.microsoft.com/vs/community/) is sufficient for solving these exercises.

!!! info "VS Code"
    The exercises can also be solved using the platform-independent **Visual Studio Code**. The skeletons of the exercises are prepared for Visual Studio. If you are working with VS Code, you need to configure your environment.

## Visual Studio workloads

When installing Visual Studio, the _ASP.NET and web development_ [workload](https://docs.microsoft.com/en-us/visualstudio/install/install-visual-studio?view=vs-2022#step-4---choose-workloads) have to be selected.

![Visual Studio 2022 workload](vs-workload.png)

An existing installation can be [_modified_](https://docs.microsoft.com/en-us/visualstudio/install/modify-visual-studio?view=vs-2022) using the _Visual Studio Installer_.

## Check and install .NET SDK

Visual Studio might install certain versions of the .NET SDK. To check if you have the right version, use the `dotnet` CLI: in a console, execute the `dotnet --list-sdks` command. This command works on Linux and Mac too. It will print something similar:

```hl_lines="2"
C:\>dotnet --list-sdks
6.0.300 [C:\Program Files\dotnet\sdk]
```

If you see version **6.0** in this list, then you are good to go. Otherwise, install the SDK [from here](https://dotnet.microsoft.com/download/dotnet/6.0).
