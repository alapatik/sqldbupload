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
using System.Configuration;
using QM.ETL.DAL.Helpers;
using System.Collections;
using System.Collections.Specialized;

namespace AzureDBExport_ConsoleApp
{
    public class AzureDatabaseExportService : IAzureDbService
    {
        IAzure azure;
        ISqlServer sqlServer;
        public static NameValueCollection _configSettings;
        //Backup backup = new Backup();
        public AzureDatabaseExportService(ICollection configSettings)
        {
            _configSettings = configSettings as NameValueCollection;
            string subscriptionId = _configSettings[AzureConstants.SubscriptionId];
            string clientId = _configSettings[AzureConstants.ClientId];
            string clientSecret = _configSettings[AzureConstants.ClientSecret];
            string tenantId = _configSettings[AzureConstants.TenantId];

            var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                                           clientId, clientSecret, tenantId,
                                           environment: AzureEnvironment.AzureGlobalCloud);
            azure = Azure.Configure()
                        .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                        .Authenticate(credentials).WithSubscription(subscriptionId);
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
                                    _configSettings[AzureConstants.StorageAccountResourceGroup],
                                    _configSettings[AzureConstants.StorageAccountName]);
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
                return sqlServer.Databases.Get(_configSettings[AzureConstants.SqlDatabaseName]);
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
            string fileName = DateTime.Now.ToString("yyyy’-‘MM’-‘dd’T’HH’:’mm’:’ss");
            sqlServer = GetSqlServer(_configSettings[AzureConstants.SqlServerResourceGroup],
                                                _configSettings[AzureConstants.SqlServerName]);
            IStorageAccount storageAccount = GetStorageAccount();
            ISqlDatabase sqlDatabase = sqlServer.Databases.Get(_configSettings[AzureConstants.SqlDatabaseName]);
            try
            {
                ISqlDatabaseImportExportResponse exportedSqlDatabase = sqlDatabase.ExportTo(
                                            storageAccount,
                                             _configSettings[AzureConstants.StorageContainerName],
                                            fileName)
                                        .WithSqlAdministratorLoginAndPassword(
                                            _configSettings[AzureConstants.SqlAdminUsername],
                                            _configSettings[AzureConstants.SqlAdminPassword])
                                        .Execute();
                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception("Error exporting database to blob stoage. ", ex);
            }
        }
    }
}
