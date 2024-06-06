﻿using Autofac;
using Clio.Workspaces;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Terrasoft.Common.Json;

namespace Clio.Tests
{
	[TestFixture]
	internal class WorkspaceTest
	{
		private EnvironmentSettings GetTestEnvironmentSettings() {
			var envSettingsJson = @"{
				  ""Uri"": ""https://forrester-lcap-demo-dev.creatio.com/"",
				  ""Login"": ""Supervisor"",
				  ""Password"": ""Supervisor1"",
				  ""Maintainer"": """",
				  ""IsNetCore"": false,
				  ""ClientId"": """",
				  ""ClientSecret"": """",
				  ""WorkspacePathes"": """",
				  ""AuthAppUri"": ""https://forrester-lcap-demo-dev-is.creatio.com/connect/token/"",
				  ""SimpleloginUri"": ""https://forrester-lcap-demo-dev.creatio.com/0/Shell/?simplelogin=true"",
				  ""Safe"": false,
				  ""DeveloperModeEnabled"": false
			}";
			return Json.Deserialize<EnvironmentSettings>(envSettingsJson);
		}

		private IWorkspace GetTestWorkspace(EnvironmentSettings envSettings) {
			var bindingModule = new BindingsModule();
			var diContainer = bindingModule.Register(envSettings);
			var workspace = diContainer.Resolve<IWorkspace>();
			return workspace;
		}

		[Test]
		public void PublishWorkspaceTest() {
			// Arrange
			string appStorePath = @"C:\Temp\clioAppStore";
			string appName = "iframe-sample";
			string appVersion = "1.0.0";
			string fileName = $"{appName}_{appVersion}.zip";
			string originClioSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			var expectedFileName = Path.Combine(appStorePath, appName, appVersion, fileName);
			string exampleWorkspacePath = Path.Combine(originClioSourcePath, "Examples", "workspaces", appName);
			try {
				// Act
				var envSettings = GetTestEnvironmentSettings();
				var workspace = GetTestWorkspace(envSettings);
				var releaseFileName = workspace.PublishToFolder(exampleWorkspacePath, appStorePath, appName, appVersion);
				// Assert
				Assert.AreEqual(expectedFileName, releaseFileName);
				var versionFileExist = File.Exists(expectedFileName);
				Assert.AreEqual(true, versionFileExist);
				Assert.True(new FileInfo(expectedFileName).Length > 80000);
			} finally {
				if (File.Exists(expectedFileName)) {
					File.Delete(expectedFileName);
				}
			}
		}

		[TestCase("trunk")]
		[TestCase("master")]
		[TestCase("feature/rnd-2035")]
		[TestCase("bugf%i_:{}x/rnd-2035")]
		[TestCase("feature/rnd-2035")]
		public void PublishWorkspaceWithBranchTest(string branch) {
			// Arrange
			string appStorePath = @"C:\Temp\clioAppStore";
			string appName = "iframe-sample";
			string appVersion = "1.0.0";
			string expectedFileName = Workspace.GetSanitizeFileNameFromString($"{appName}_{branch}_{appVersion}.zip");
			string originClioSourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory);
			string exampleWorkspacePath = Path.Combine(originClioSourcePath, "Examples", "workspaces", appName);
			string branchFolderName = Workspace.GetSanitizeFileNameFromString(branch);
			// app_store_path\app_name\bramch\file_name
			var expectedFilePath = Path.Combine(appStorePath, appName, branchFolderName, expectedFileName);
			try {
				// Act
				var envSettings = GetTestEnvironmentSettings();
				var workspace = GetTestWorkspace(envSettings);
				var releaseFileName = workspace.PublishToFolder(exampleWorkspacePath, appStorePath, appName, appVersion, branch);
				// Assert
				Assert.AreEqual(expectedFilePath, releaseFileName);
				var versionFileExist = File.Exists(expectedFilePath);
				Assert.AreEqual(true, versionFileExist);
				Assert.True(new FileInfo(expectedFilePath).Length > 80000);
			} finally {
				if (File.Exists(expectedFilePath)) {
					File.Delete(expectedFilePath);
				}
			}

		}
	}
}
