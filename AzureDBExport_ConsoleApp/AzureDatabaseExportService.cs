using AzureDatabaseExport.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;

namespace AzureDBExport_ConsoleApp
{
    public class AzureDatabaseExportService
    {
        IAzure azure;
        ISqlServer sqlServer;
        public static IConfigurationRoot configuration;
        IStorageAccount storageAccount;
        CloudBlobContainer cloudBlobContainer;
        Backup backup;
        public AzureDatabaseExportService(string subscriptionId,
            string clientId, string clientSecret, string tenantId)
        {
            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                                           clientId, clientSecret, tenantId,
                                           environment: AzureEnvironment.AzureGlobalCloud);
            azure = Azure.Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials).WithSubscription(subscriptionId);

            configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile("config.json", false, true)
                                .Build();
            configuration.GetSection("Backup").Bind(backup);
        }

        private ISqlServer GetSqlServer(string sqlServerResourceGroup, string sqlServerName)
        {
            try
            {
                return azure.SqlServers.GetByResourceGroup(sqlServerResourceGroup,
                                            sqlServerName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting Azure SQL server resource info. ", ex);
            }

        }
        private IStorageAccount GetStorageAccount()
        {
            try
            {
                return azure.StorageAccounts.GetByResourceGroup(
                                    backup.Destination.StorageAccountResourceGroup,
                                    backup.Destination.StorageAccountName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting Azure storage account info. ", ex);
            }
        }
        private ISqlDatabase GetSqlDatabase()
        {
            try
            {
                return sqlServer.Databases.Get(backup.Source.SqlDatabaseName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting Azure Database info. ", ex);
            }
        }
        /// <summary>
        /// Exports a copy of database to blob storage
        /// </summary>
        /// <param name="backup"></param>
        /// <returns>Returns status of export</returns>
        public string ExportAzureDatabase()
        {
            sqlServer = GetSqlServer(backup.Source.SqlServerResourceGroup,
                                                backup.Source.SqlServerName);
            IStorageAccount storageAccount = GetStorageAccount();
            ISqlDatabase sqlDatabase = sqlServer.Databases.Get(backup.Source.SqlDatabaseName);
            try
            {
                ISqlDatabaseImportExportResponse exportedSqlDatabase = sqlDatabase.ExportTo(
                                            storageAccount,
                                            backup.Destination.StorageContainerName,
                                            backup.Destination.FileName)
                                        .WithSqlAdministratorLoginAndPassword(
                                            backup.Source.SqlAdminUsername,
                                            backup.Source.SqlAdminPassword)
                                        .Execute();
                return exportedSqlDatabase.Status;
            }
            catch (Exception ex)
            {
                throw new Exception("Error exporting database to blob stoage. ", ex);
            }
        }
    }
}
