# Actions

## AzureFunctionsPipeline
This action is composed by two jobs:
* `Build`: this job builds and publishes the Azure Functions project, and uploads the artifact on the artifact repository for the Deploy job;
* `Deploy`: this job downloads the artifact (zip file) created by the previous job and publish it on a Function App on Azure.

The action need the secret `FUNCTION_APP_PUBLISH_PROFILE` that contains the publish profile of the Function App you want to use.
To retrieve the publish profile you can run the following command:

```bash
az functionapp deployment list-publishing-profiles --name [func app name] --resource-group [func app resource group name] --xml
```

The response of the previous command is an XML similar to the following:

```xml
<publishData>
	<publishProfile profileName="******* - Web Deploy" publishMethod="MSDeploy" publishUrl="*******" msdeploySite="*******" userName="*******" userPWD="*******" destinationAppUrl="*******" SQLServerDBConnectionString="" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="http://windows.azure.com" webSystem="WebSites">
		<databases />
	</publishProfile>
	<publishProfile profileName="******* - FTP" publishMethod="FTP" publishUrl="*******" ftpPassiveMode="True" userName="*******" userPWD="*******" destinationAppUrl="*******" SQLServerDBConnectionString="" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="http://windows.azure.com" webSystem="WebSites">
		<databases />
	</publishProfile>
	<publishProfile profileName="******* - Zip Deploy" publishMethod="ZipDeploy" publishUrl="*******" userName="*******" userPWD="*******" destinationAppUrl="*******" SQLServerDBConnectionString="" mySQLDBConnectionString="" hostingProviderForumLink="" controlPanelLink="http://windows.azure.com" webSystem="WebSites">
		<databases />
	</publishProfile>
</publishData>
```

You need to copy the XML into the secret.

## IaCPipeline
This action is composed by two jobs:
* `Build`: this job use the command `az bicep build` to check if the bicep templates are well formed, and to create a ARM Template JSON. This JSON template is uploaded as artifact for the next job;
* `Deploy`: this job downloads the artifact (ARM template) created by the previous job and deploy it on Azure.

The action need the secret `AZURE_CREDENTIAL` that contains the credential of the service principal you use to deploy the template.

To create the service principal, you can use the following command:

```bash
az ad sp create-for-rbac --name "ServerlessFaceAnalyzerPipeline" --role [role] --scopes [subscription id] --sdk-auth
```

* `role` : role of the service principal. The template assign RBAC to the KeyVault to allow Function App to read secret, so the role must have the right operations;
* `subscription id` : the id of the subscription in the form of `/subscriptions/xxxxxxxx-yyyy-zzzz-wwww-kkkkkkkkkkkk`. 

The previous command returns a JSON like the following:

```json
{
  "clientId": "<GUID>",
  "clientSecret": "<STRING>",
  "subscriptionId": "<GUID>",
  "tenantId": "<GUID>",
  "resourceManagerEndpointUrl": "<URL>"
  (...)
}
```

Copy the JSON into the secret.

For more info: <a href="https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli" target="_blank">https://docs.microsoft.com/en-us/cli/azure/create-an-azure-service-principal-azure-cli</a>