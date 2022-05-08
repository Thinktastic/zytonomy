global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.IdentityModel.Tokens.Jwt;
global using System.Linq.Expressions;
global using System.Net.Http;
global using System.Reflection;
global using System.Security.Authentication;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Security.Cryptography.X509Certificates;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Text.RegularExpressions;

global using Azure.Storage;
global using Azure.Storage.Blobs;
global using Azure.Storage.Blobs.Models;
global using Azure.Storage.Sas;

global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.StaticFiles;
global using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
global using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
global using Microsoft.Azure.Cosmos;
global using Microsoft.Azure.Cosmos.Linq;
global using Microsoft.Azure.Functions.Extensions.DependencyInjection;
global using Microsoft.Azure.WebJobs;
global using Microsoft.Azure.WebJobs.Extensions.DurableTask;
global using Microsoft.Azure.WebJobs.Extensions.Http;
global using Microsoft.Azure.WebJobs.Extensions.OpenApi.Configurations.AppSettings.Extensions;
global using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
global using Microsoft.Azure.WebJobs.Extensions.SignalRService;
global using Microsoft.Azure.WebJobs.Host;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.IdentityModel.Tokens;
global using Microsoft.OpenApi.Models;
global using Microsoft.WindowsAzure.Storage;

global using Zytonomy.Api.DataAccess.Core;
global using Zytonomy.Api.DataAccess.Mutators;
global using Zytonomy.Api.DataAccess.Repositories;
global using Zytonomy.Api.DataAccess.Support;
global using Zytonomy.Api.Model;
global using Zytonomy.Api.Model.Embedded;
global using Zytonomy.Api.Model.Support;
global using Zytonomy.Api.Model.Visitors;
global using Zytonomy.Api.Support;

global using SendGrid.Helpers.Mail;