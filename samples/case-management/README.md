# Case Management

The complete solution from the [Logic Fabric case management tutorial](https://docs.flowon.com/tutorials/): new service cases are validated before they are saved, routed to a triage team with a response due date, tracked by a business process that escalates late triage, and exposed to a customer portal through a Logic API.

| Folder | What it holds |
| --- | --- |
| [`dataverse/`](dataverse/) | The `ContosoService` solution (managed and unmanaged) and the `Case Management` Logic Fabric package, ready to import. |
| [`api-host/`](api-host/) | An ASP.NET Core host that serves the package's **Customer Portal API** to signed-in customers. |

Start with the Dataverse solution, then run the host against the same environment.

## Import the Dataverse solution

The [`dataverse/`](dataverse/) folder holds the complete case management solution, including the published **Customer Portal API**:

| File | Use it for |
| --- | --- |
| `ContosoService_unmanaged.zip` | Development environments, where you want to open and change the logic |
| `ContosoService_managed.zip` | Test and production environments |
| `Case_Management.flop` | The Logic Fabric package: configuration values, localized resources, logic blocks, flows, the recipe, the business process and the API |
| `mappings/*.datamap.xml` | Values that differ per environment: team IDs, SLA days, escalation mailbox |

You need Dynamics 365 Customer Service (for the Case table), Logic Fabric 1.0.0.37 or later with API Builder, and the [Logic Fabric CLI](../../README.md#install-the-logic-fabric-cli).

1. **Create the teams** `Service Triage`, `Tier 2 Support`, `Service Managers` and `Support Supervisors` in the target environment, as owner teams in the root business unit.
2. **Map them.** In [`dataverse/mappings/entity.datamap.xml`](dataverse/mappings/entity.datamap.xml), set each team's **Target** `id` to the ID of the matching team in your environment. Leave the **Source** values as they are.
3. **Adjust the settings** in [`dataverse/mappings/configuration.datamap.xml`](dataverse/mappings/configuration.datamap.xml) if you like, for example your own escalation mailbox. Change only the **Target** values.
4. **Import** the solution and the package with one command, from the `dataverse` folder:

   ```bash
   flowon-dynamics import \
     -c "AuthType=OAuth;Url=https://<org>.crm.dynamics.com;Username=<you>;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=http://localhost;LoginPrompt=Auto" \
     --solution-file-path ./ContosoService_managed.zip \
     --package-file-path ./Case_Management.flop \
     --map-file-path ./mappings/configuration.datamap.xml ./mappings/entity.datamap.xml
   ```

   Use `./ContosoService_unmanaged.zip` in a development environment. The solution is imported first, then the package with the mappings applied; the command ends with `Import completed in … seconds.`

5. **Check it:** create a case without a **Case Type**. The save is refused with `Choose a case type`.

[`dataverse/README.md`](dataverse/README.md) describes every artifact in the solution and the package.

## Run the API host

The [API host](api-host/README.md) serves the **Customer Portal API** from the environment you imported into: customers sign in and read the summary of their own cases. Its README covers the Dataverse identity, the sign-in setup, running and testing it locally, and deploying it to Azure App Service.
