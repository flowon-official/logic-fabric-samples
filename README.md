# Logic Fabric samples

[![Build](../../actions/workflows/build.yml/badge.svg)](../../actions/workflows/build.yml)

Samples for [Logic Fabric](https://docs.flowon.com/), the no-code business logic, process orchestration and API platform for Microsoft Dataverse and Dynamics 365. Each sample is a complete, working solution: the Logic Fabric artifacts to import into your environment, and any code that goes with them.

## Samples

| Sample | What it shows | Parts |
| --- | --- | --- |
| [Case Management](samples/case-management/) | The solution from the [case management tutorial](https://docs.flowon.com/tutorials/): validation, routing, an SLA formula, a business process with escalation, and a customer portal API. | [Dataverse solution and package](samples/case-management/dataverse/), [API host](samples/case-management/api-host/) |

Start with the sample's README: it says what to import and what to run, in order.

## Repository layout

```text
samples/
  <sample>/
    README.md        What the sample shows and how to run it
    dataverse/       Solutions, Logic Fabric package (.flop) and data maps to import
    <app>/           Code that goes with the sample, for example api-host/
.github/             Build workflow and dependency updates for every sample
global.json          .NET SDK used by every sample
nuget.config         Restores packages from nuget.org only
```

Every sample stands on its own: you can copy its folder out of the repository and use it. See [CONTRIBUTING.md](CONTRIBUTING.md) to add a sample.

## Prerequisites

- A Dataverse environment, or Dynamics 365 Customer Engagement, with the Logic Fabric solutions the sample lists.
- The [Logic Fabric CLI](#install-the-logic-fabric-cli), to import a sample's `dataverse/` folder.
- The [.NET 10 SDK](https://dotnet.microsoft.com/download), for samples with code.

## Install the Logic Fabric CLI

The Logic Fabric CLI (`flowon-dynamics`) imports and exports Logic Fabric projects. It is a .NET tool published on nuget.org as [`Flowon.Xrm.Tools.CommandLine`](https://www.nuget.org/packages/Flowon.Xrm.Tools.CommandLine) and needs the .NET 10 SDK or runtime.

Install it once for your user account:

```bash
dotnet tool install --global Flowon.Xrm.Tools.CommandLine
flowon-dynamics --version
```

The second command prints the installed version, for example `1.1.7+…`. If `flowon-dynamics` isn't found, open a new terminal so it picks up the .NET tools folder (`%USERPROFILE%\.dotnet\tools` on Windows, `~/.dotnet/tools` on macOS and Linux) on your `PATH`.

| To | Run |
| --- | --- |
| Update to the latest version | `dotnet tool update --global Flowon.Xrm.Tools.CommandLine` |
| Install a specific version | `dotnet tool install --global Flowon.Xrm.Tools.CommandLine --version 1.1.7` |
| Remove it | `dotnet tool uninstall --global Flowon.Xrm.Tools.CommandLine` |
| See the commands | `flowon-dynamics --help` |

Use a version that matches the Logic Fabric solutions in your environment, so the package format is the same at both ends.

**In a build pipeline or a team repository,** install it as a local tool instead, so everyone uses the same version:

```bash
dotnet new tool-manifest
dotnet tool install Flowon.Xrm.Tools.CommandLine --version 1.1.7
dotnet tool run flowon-dynamics --version
```

The version is recorded in `dotnet-tools.json`; commit that file, and run `dotnet tool restore` on a new machine or build agent.

**If the install fails with `401 Unauthorized` or "Unable to load the service index",** a private package feed configured on your machine is failing. Install from nuget.org only:

```bash
dotnet tool install --global Flowon.Xrm.Tools.CommandLine --add-source https://api.nuget.org/v3/index.json --ignore-failed-sources
```

## Build the samples

Each code sample has its own solution file. To build and test one:

```bash
cd samples/case-management/api-host
dotnet build
dotnet test
```

The [Build workflow](.github/workflows/build.yml) builds and tests every solution under `samples/` on each push and pull request.

## License

[MIT](LICENSE)
