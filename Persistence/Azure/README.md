# Simple persistence snapshot using Azure Blob Storage

A simple example on how to use Azure Blob Storage as a snapshot store for Akka.NET persistence actor using the
new `DefaultAzureCredential` setup, connecting to Azure using the credential built-in to Visual Studio.

To use this example, you have to:
1. Log in to your Microsoft account in Visual Studio 2019
2. Have an Azure Storage account
3. Set your user role as "Blob Storage Contributor" inside the Azure Storage IAM