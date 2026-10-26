# Adding a sample

Each sample lives in its own folder under `samples/` and stands on its own: someone can copy that folder out of the repository and follow its README without reading anything else.

## Folder layout

```text
samples/
  <sample-name>/                 Lowercase, words separated by hyphens: case-management
    README.md                    Required
    dataverse/                   When the sample has Logic Fabric artifacts
      README.md                  What's inside, how to import, known issues
      <Solution>_managed.zip
      <Solution>_unmanaged.zip
      <Project_Name>.flop
      mappings/                  configuration, entity and localizedresources data maps
    <app-name>/                  When the sample has code, for example api-host/
      README.md
      <Name>.slnx
      src/
      tests/
```

A sample can have several app folders, for example `api-host/` and `portal/`, each with its own solution and README.

## The sample README

Start from [the Case Management README](samples/case-management/README.md). It should say:

1. What the sample shows, in one or two sentences, with a link to the matching docs.flowon.com page.
2. A table of the sample's folders.
3. What to import, with the exact `flowon-dynamics import` command, and what to run, in order.

Link to [Install the Logic Fabric CLI](README.md#install-the-logic-fabric-cli) instead of repeating the install steps.

## Logic Fabric artifacts

- **Build them in a development environment** and export them with the Logic Fabric CLI, so the files are exactly what the product produces:

  ```bash
  flowon-dynamics export -c "<connection string>" --project-name "<Project>" \
    --solution-managed --solution-unmanaged --output-directory ./dataverse
  flowon-dynamics generate-mappings --project-file-path ./dataverse/<Project_Name>.flop \
    --output-directory ./dataverse/mappings
  ```

- **Increase the solution version** before you export an update (`flowon-dynamics isv`), so an environment that already has the sample imports the new version.
- **Use a dedicated publisher and solution** for each sample, not a default one.
- **Note the Logic Fabric version** the sample needs in its `dataverse/README.md`.

## Code

- Target the .NET SDK in [`global.json`](global.json). Packages restore from nuget.org only ([`nuget.config`](nuget.config)), so use published package versions.
- **Never commit secrets.** Put placeholders in `appsettings.json` and read real values from user secrets or the host's settings.
- Add tests where the sample has logic of its own.
- The [Build workflow](.github/workflows/build.yml) finds every `.slnx` or `.sln` under `samples/` and builds and tests it, so there is nothing to register.

## Before you open a pull request

- Add the sample to the **Samples** table in the [README](README.md).
- Check every link in the sample's READMEs.
- Run `dotnet build` and `dotnet test` in each app folder.
