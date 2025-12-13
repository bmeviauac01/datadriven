# Homework

With these exercises, you can earn **points** that are added to your exam score. In the exercises and the evaluation results, you will see a text “iMsc” for optional exercises; these iMsc points are not counted! (These are special points for the Hungarian curriculum). All non-iMsc exercises are available for points in this course, 5 homeworks, with a maximum of 4 points per homework, for a total of 20 points. Here you find the exercise descriptions; the submission of the solutions is expected via GitHub Classroom. If you fail to submit the exercises exactly as in the guide, or if it is submitted late, you get no points at all! Make sure to follow the guide and do **everything in time**!

!!! important "Working code"
    You are expected to write code that actually works! Your code will be executed, and it is required to fulfill the specified task.

## The exercises

1. [MSSQL server-side programming](mssql/index.md)
1. [Entity Framework](ef/index.md)
1. [MongoDB](mongodb/index.md)
1. [REST API and Web API](rest/index.md)
1. [GraphQL](graphql/index.md)

## Submission

Each homework must be submitted in a personal git repository. Please refer to the detailed [guideline here](GitHub.md). You must carefully study these guidelines!

!!! danger "IMPORTANT"
    The submissions of the homework **must** follow these guidelines. Submissions not adhering to the expected format are not considered.

    Workflow errors, i.e., not following the guidelines (e.g., not assigning the right person to the pull request, or not assigning anyone to it at all), are penalized.

## Screenshots

Some of the exercises require you to create a screenshot. This screenshot is proof of the completion of the exercise. **The expected content of these screenshots is detailed in the exercise description.** The screenshot may include the entire desktop or just the required portion of the screen.

!!! info ""
    The screenshots must be submitted as part of the solution code, uploaded to the git repository. The repositories are private; only you and the instructors can access them. You may obscure parts of the screenshot if there is any content that is not relevant to the exercise, and you would like to remove it.

## Required tools

- Windows, Linux, or macOS: All tools are platform-independent, or a platform-independent alternative is available.
- A [GitHub](https://github.com/) account and a [git](https://git-scm.com/) client.

### MAC OS / Apple Silicon Setup Guide

If you are using a Mac with an Apple Silicon processor, you cannot run the full Windows version of Visual Studio. Instead, you should use a **native** development workflow using VS Code and Docker.

1.  **Development Environment (Native macOS):**
    * **Install .NET SDK:** Download and install the **.NET 8.0 SDK (Arm64)** from the [official Microsoft website](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).
    * **Install VS Code:** Download [Visual Studio Code for macOS](https://code.visualstudio.com/download).
    * **Install Extensions:** Open VS Code and install the following extensions to handle C#, SQL, and MongoDB homeworks:
        * [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) (for writing the code)
        * [SQL Server (mssql)](https://marketplace.visualstudio.com/items?itemName=ms-mssql.mssql) (for managing the MSSQL database)
        * [MongoDB for VS Code](https://marketplace.visualstudio.com/items?itemName=mongodb.mongodb-vscode) (for the MongoDB homework)

2.  **SQL Server Database (Docker):**
    You cannot install SQL Server directly on macOS, so you must use Docker.

    **Step A: Install Docker Desktop**
    * Download "Docker Desktop for Mac with Apple silicon" from the [Docker Website](https://www.docker.com/products/docker-desktop/) or install via Homebrew (`brew install --cask docker`).

    **Step B: Start the Database**
    Open your macOS Terminal and run the following command. This creates a container named `database-sql`.

    ```bash
    docker run -e "ACCEPT_EULA=Y" \
    -e "MSSQL_SA_PASSWORD=YourStrong@Passw0rd" \
    -p 1433:1433 \
    --name database-sql \
    -d [mcr.microsoft.com/mssql/server:2022-latest](https://mcr.microsoft.com/mssql/server:2022-latest)
    ```

3.  **Connecting to the Database:**
    * **From VS Code:** Use the SQL Server extension you installed. Connect to `localhost` with username `sa` and the password you defined above.
    * **From C# Code:** Update your `ConnectionString` in `appsettings.json`.
    ```json
    "DefaultConnection": "Server=localhost;Database=master;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"
    ```
    *Note regarding the Database name: In the Docker command, `--name database-sql` names the container. In the connection string, `Database=master` refers to the logical database. You may need to change `master` to the specific database name created by the homework script (e.g., `mssql.sql`) once you run it.*

---

### For homework using the MSSQL platform:
  - Microsoft SQL Server. The free _Express_ version is sufficient, or you may also use the _localdb_ installed with Visual Studio. 
    - A [Ubuntu Linux 22.04 version](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup) is also available, or
    - [Docker](https://learn.microsoft.com/en-us/sql/linux/quickstart-install-connect-docker) can be used to run it on Linux or macOS.
  - Database initialization script: [mssql.sql](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mssql.sql)
  - One of the following database management and developer tools:
    - [SQL Server Management Studio](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
    - [Visual Studio Code](https://code.visualstudio.com/download) with the [SQL Server (mssql) extension](https://marketplace.visualstudio.com/items?itemName=ms-mssql.mssql)
    - [JetBrains DataGrip](https://www.jetbrains.com/datagrip/download/) - Platform-independent, free for non-profit and educational use.

### For homework using a MongoDB database:
  - [MongoDB Community Server](https://www.mongodb.com/download-center/community)
  - Sample database initialization script: [mongo.js](https://raw.githubusercontent.com/bmeviauac01/datadriven/master/overrides/db/mongo.js)
  - One of the following database management and developer tools:
    - [VSCode](https://code.visualstudio.com/download) with the [MongoDB for VSCode](https://marketplace.visualstudio.com/items?itemName=mongodb.mongodb-vscode) extension
    - [JetBrains DataGrip](https://www.jetbrains.com/datagrip/download/)

### For homeworks using REST API:
- One of the following developer tools:
  - [Postman](https://www.getpostman.com/)
  - [Hoppscotch](https://docs.hoppscotch.io/) - An open-source, browser-based alternative.

### For writing C# code (most homeworks, except the first one):
  - One of the following development environments:
    - Microsoft Visual Studio 2022 [with the settings here](VisualStudio.md) (on Windows)
    - Visual Studio Code with the [C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) and the .NET SDK-installed [dotnet CLI](https://docs.microsoft.com/en-us/dotnet/tools/)
    - [JetBrains Rider](https://www.jetbrains.com/rider/download/) - available on Windows, macOS, and Linux, free for non-profit and educational use.
  - [.NET **8.0** SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

    !!! warning ".NET 8.0"
        Mind the version! You need .NET SDK version **8.0** for these exercises.

        On Windows, it might already be installed along with Visual Studio (see [here](VisualStudio.md#check-and-install-net-sdk) how to check it); if not, use the link above to install the SDK (_not_ the runtime). You may need to install it manually when using Linux or macOS.

## Submission evaluation

The evaluation of the exercises is **semi-automatic**. Your code will be executed; therefore, it is vital to follow the exercise descriptions precisely (e.g., use the provided code skeleton, change only the allowed parts of the code, etc.).

You will receive a preliminary result about your submission in GitHub; see the guidelines [here](GitHub.md). If there are some issues you need to diagnose, the entire log of the execution is available for you on the _GitHub Actions_ web page. A short introduction is provided [here](GitHub-Actions.md).

!!! danger "Verification"
    In some of the exercises, where the technology permits, you will find unit tests. These tests **help** you verify your work, but they are **no substitute for your validation**. When you upload your work, more exhaustive testing will evaluate your submission.
