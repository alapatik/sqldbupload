using AzureDatabaseExport.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AzureDBExport_ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            AzureDatabaseExportService service = new AzureDatabaseExportService();
            service.ExportAzureDatabase();
        }
    }
}
