using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage;
using Azure.Storage.Sas;

namespace stottle.blobStorage
{
  public class SasTokenDetails
  {
    public string storageUri { get; set; }
    public string storageAccessToken { get; set; }
  }

  public static class GenerateSasToken
  {
    [FunctionName("GenerateSasToken")]
    public static IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
      log.LogInformation("SAS Token Requests");

      var accountName = Environment.GetEnvironmentVariable("AZURE_ACCOUNT_NAME");
      var key = Environment.GetEnvironmentVariable("AZURE_ACCOUNT_KEY");
      var sharedKeyCredentials = new StorageSharedKeyCredential(accountName, key);
      var sasBuilder = new AccountSasBuilder()
      {
        StartsOn = DateTimeOffset.UtcNow,
        ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5),
        Services = AccountSasServices.Blobs,
        ResourceTypes = AccountSasResourceTypes.All,
        Protocol = SasProtocol.Https
      };
      sasBuilder.SetPermissions(AccountSasPermissions.All);

      var sasToken = sasBuilder.ToSasQueryParameters(sharedKeyCredentials).ToString();
      var storageUri = $"https://{accountName}.blob.core.windows.net/";

      return new OkObjectResult(new SasTokenDetails()
      {
        storageAccessToken = sasToken,
        storageUri = storageUri
      });
    }
  }
}




