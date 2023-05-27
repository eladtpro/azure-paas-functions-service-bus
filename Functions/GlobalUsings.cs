global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading.Tasks;
global using System.Runtime.Serialization;
global using System.Text.RegularExpressions;

global using Microsoft.Extensions.Logging;

global using Microsoft.Azure.WebJobs;
global using Microsoft.Azure.WebJobs.Extensions.DurableTask;

global using Azure;
global using Azure.Storage.Blobs;
global using Azure.Storage.Blobs.Models;
global using Azure.Storage.Sas;
global using Azure.Storage.Blobs.Specialized;

global using Newtonsoft.Json;

global using Functions.Extensions;
global using Functions.Core;
global using Functions.Model;


