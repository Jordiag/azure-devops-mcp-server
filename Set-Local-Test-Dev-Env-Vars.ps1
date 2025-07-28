<#
    ====================================================================
    Set-Local-Test-DevEnvVars.ps1
    --------------------------------------------------------------------
    Persists Azure DevOps, Azure OpenAI and local helper environment
    variables at **user scope** on Windows 11.

    ─── How to use ───
      1.  Save the file somewhere private.
      2.  Open PowerShell. If scripts are blocked, allow them for the
          current session only:
              Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
      3.  Run the script:
              .\Set-DevEnvVars.ps1
      4.  Close and reopen any terminals, IDEs, or shells so they
          inherit the new variables.

    ─── Security note ───
      PATs and API keys are secrets – never commit this file to source
      control, and limit NTFS permissions to yourself.

    To apply the variables system-wide, launch an *elevated* PowerShell
    session and change the scope from "User" to "Machine".
    ====================================================================
#>

$vars = @{

    # ── Azure DevOps context ───────────────────────────────────────────
    "AZURE_DEVOPS_BOT_USER_EMAIL"   = "<SET-YOUR-EMAIL>" # e.g. the bot/service identity that performs automation
    "AZURE_DEVOPS_BUILD_BRANCH"     = "refs/heads/<BRANCH-NAME>" # usually 'main' or your default CI branch
    "AZURE_DEVOPS_COMMIT_SHA"       = "<SET-LATEST-COMMIT-SHA>" # can be retrieved from your latest build metadata
    "AZURE_DEVOPS_MAIN_BRANCH_NAME" = "main" # or 'master', depending on repo setup
    "AZURE_DEVOPS_ORG_URL"          = "https://dev.azure.com/<your-org-name>" # Replace <your-org-name>

    "AZURE_DEVOPS_PAT"              = "<CHANGE-ME-PAT>" # Generate a Personal Access Token: https://dev.azure.com/ -> User Settings -> Personal Access Tokens

    "AZURE_DEVOPS_PIPELINE_ID"      = "<SET-PIPELINE-ID>" # GET https://dev.azure.com/{org}/{project}/_apis/pipelines
    "AZURE_DEVOPS_PROJECT_ID"       = "<SET-PROJECT-ID>"  # GET https://dev.azure.com/{org}/_apis/projects
    "AZURE_DEVOPS_PROJECT_NAME"     = "<SET-PROJECT-NAME>" # Usually the name of your Azure DevOps project

    "AZURE_DEVOPS_REPO_ID"          = "<SET-REPO-ID>"     # GET https://dev.azure.com/{org}/{project}/_apis/git/repositories
    "AZURE_DEVOPS_REPO_NAME"        = "<SET-REPO-NAME>"   # From same call as above
    "AZURE_DEVOPS_REPOSITORY_ID"    = "<SET-REPOSITORY-ID>" # Often same as REPO_ID — confirm based on usage

    "AZURE_DEVOPS_SRC_BRANCH"       = "refs/heads/<FEATURE-BRANCH-NAME>"
    "AZURE_DEVOPS_TARGET_BRANCH"    = "refs/heads/<MAIN-BRANCH-NAME>"

    # ── Azure OpenAI (optional) ────────────────────────────────────────
    "AZURE_OPENAI_DEPLOYMENT"       = "<SET-DEPLOYMENT-NAME>" # From Azure OpenAI Studio
    "AZURE_OPENAI_ENDPOINT"         = "<SET-ENDPOINT-URL>" # e.g. https://<your-resource>.openai.azure.com
    "AZURE_OPENAI_KEY"              = "<SET-AZURE-OPENAI-KEY>" # Azure Portal → OpenAI resource → Keys

    # ── OpenAI (public) ────────────────────────────────────────────────
    "OPENAI_API_KEY"                = "<SET-OPENAI-API-KEY>" # https://platform.openai.com/account/api-keys
    "OPENAI_MODEL"                  = "gpt-4o-mini" # or "gpt-4", "gpt-3.5-turbo" depending on your use

    # ── Local integration test infrastructure ─────────────────────────
    "MCP_SERVER_URL"                = "http://localhost:5050" # Set if running a server locally

    # ── Feature flag: toggle which provider the tests use ─────────────
    "USE_AZURE_OPENAI"              = "false" # change to "true" to use Azure OpenAI instead of public OpenAI
}

foreach ($keyValuePair in $vars.GetEnumerator()) {
    [Environment]::SetEnvironmentVariable($keyValuePair.Key, $keyValuePair.Value, "User")
    Write-Host "Set $($keyValuePair.Key)"
}

Write-Host "`nDone! Open a new PowerShell or Command Prompt session to pick up the changes."
