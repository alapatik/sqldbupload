﻿using AzureDatabaseExport.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.Sql.Fluent;
using Microsoft.Azure.Management.Storage.Fluent;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
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
            AzureDatabaseExportService service = new AzureDatabaseExportService(ConfigurationManager.AppSettings);
            string fileName = service.ExportAzureDatabase();
            BlobMethods blob = new BlobMethods("mysqlbacpacstorage", "gcK8hERRkxl9M988RfJTND2ZQAp+nF430zGzUVwTJunq/yHZrCAISGMVrOiIrDO5gizjCDWTOLpTreDb5yhOnQ==", "practice");
            //blob.DeleteBlob(fileName);
            blob.DownloadFileFromBlob(fileName, "~");

        }
    }
}
