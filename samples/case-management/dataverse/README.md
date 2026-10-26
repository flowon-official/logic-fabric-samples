# Case Management sample for Dataverse

The complete Logic Fabric solution from the [case management tutorial](https://docs.flowon.com/tutorials/), ready to import into a Dataverse environment. It includes the **Customer Portal API** that the [API host in this sample](../api-host/README.md) serves.

| File | What it is |
| --- | --- |
| [`ContosoService_unmanaged.zip`](ContosoService_unmanaged.zip) | The `ContosoService` solution, unmanaged. Import it into a development environment where you want to open and change the logic. |
| [`ContosoService_managed.zip`](ContosoService_managed.zip) | The same solution, managed. Import it into test and production environments. |
| [`Case_Management.flop`](Case_Management.flop) | The Logic Fabric package of the `Case Management` project: every configuration value, localized resource, logic block, flow, recipe, business process and API. |
| [`mappings/`](mappings/) | Data map files, so each environment gets its own team IDs, SLA values and escalation mailbox. |

The solution holds what Dataverse needs for the recipe (its two plug-in steps on Case). The package holds the logic itself. You import both, solution first, with one command.

## What's inside

| Artifact | Name | What it does |
| --- | --- | --- |
| Configuration | `SlaResponseDaysHigh` | Working days to respond to a high-priority case (`1`). |
| Configuration | `SlaResponseDaysStandard` | Working days to respond to any other case (`3`). |
| Configuration | `EscalationTeamEmail` | Mailbox copied on every new-case email (`support-escalations@contoso.com`). |
| Localized resource | `Case_NewCaseSubject`, `Case_NewCaseBody` | Subject and body of the triage email, in English. |
| Validation set | `New case checks` | A new case needs a title, a case type and a description of at least 20 characters. |
| Formula | `Case response due date` | Created On plus the SLA's working days for the case's priority, skipping weekends and business closures. |
| Logic flow | `Route new case` | Assigns a new case to `Service Triage`, emails the team, and notes on the case whether the email went out. |
| Logic flow | `Get case SLA` | Exposed as an MCP tool: the response due date and working days left for a case number. |
| Logic recipe | `Case rules` | On Case create: runs `New case checks` before the save, then `Route new case` and `Case handling` in the background. |
| Business process | `Case handling` (version 1, active) | Triage with an SLA due date and escalation to `Support Supervisors`; high-priority cases get a technical and a customer impact review in parallel. |
| API | `Customer Portal API` (version 1.0) | Query `caseSummary`: a signed-in customer reads the summary of one of their own cases by case number. |

## Prerequisites

- A Dataverse environment with **Dynamics 365 Customer Service** (for the Case table).
- Logic Fabric **1.0.0.37 or later**: Logic Fabric Base, Logic Composer, Process Orchestrator and API Builder, with their Tools solutions.
- The Logic Fabric CLI, installed from nuget.org. See [Install the Logic Fabric CLI](../../../README.md#install-the-logic-fabric-cli).
- An account with System Administrator, or System Customizer with the **Logic Fabric Author** and **Process Designer** roles.

## Import

1. **Create the four teams** in the target environment, as owner teams in the root business unit: `Service Triage`, `Tier 2 Support`, `Service Managers` and `Support Supervisors`. Add members with email addresses to `Service Triage`, so the triage email has recipients.

2. **Map the teams.** The logic refers to the teams by ID, and IDs differ per environment. In [`mappings/entity.datamap.xml`](mappings/entity.datamap.xml), set each **Target** `id` to the ID of the matching team in your environment (from the team's URL, or `GET /api/data/v9.2/teams?$select=name`). Leave the **Source** values as they are.

3. **Adjust the settings** you want to differ, in [`mappings/configuration.datamap.xml`](mappings/configuration.datamap.xml): change only the **Target** values, for example your own escalation mailbox.

4. **Import** the solution and the package together:

   ```bash
   flowon-dynamics import \
     -c "AuthType=OAuth;Url=https://<org>.crm.dynamics.com;Username=<you>;AppId=51f81489-12ee-4a9e-aaae-a2591f45987d;RedirectUri=http://localhost;LoginPrompt=Auto" \
     --solution-file-path ./ContosoService_managed.zip \
     --package-file-path ./Case_Management.flop \
     --map-file-path ./mappings/configuration.datamap.xml ./mappings/entity.datamap.xml
   ```

   Use `ContosoService_unmanaged.zip` instead in a development environment. The command imports the solution first, then the package with the data maps applied, and ends with `Import completed in … seconds.`

5. **Check it.** Create a case without a **Case Type**: the save is refused with `Choose a case type`. See [Part 4 of the tutorial](https://docs.flowon.com/tutorials/case-management-4-recipe/#test-it-on-a-real-case) for the full test.

To serve the API, run the [API host](../api-host/README.md) against the same environment.
