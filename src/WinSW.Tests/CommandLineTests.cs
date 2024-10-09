using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using WinSW.Tests.Util;
using Xunit;
using Helper = WinSW.Tests.Util.CommandLineTestHelper;

namespace WinSW.Tests
{
    public class CommandLineTests
    {
        [ElevatedFact]
        public void Install_Start_Stop_Uninstall_Console_App()
        {
            using var config = Helper.TestXmlServiceConfig.FromXml(Helper.SeedXml);

            try
            {
                using var controller = CommandLineTestsUtils.ExecuteInstall(config);

#if NET
                InterProcessCodeCoverageSession session = null;
                try
                {
                    try
                    {
                        session = CommandLineTestsUtils.ExecuteStart(config, controller);
                    }
                    finally
                    {
                        CommandLineTestsUtils.ExecuteStop(config, controller);
                    }
                }
                finally
                {
                    session?.Wait();
                }
#endif
            }
            finally
            {
                CommandLineTestsUtils.ExecuteUninstall(config);
            }
        }

        [Fact]
        public void FailOnUnknownCommand()
        {
            const string commandName = "unknown";

            var result = Helper.ErrorTest(new[] { commandName });

            Assert.Equal($"Unrecognized command or argument '{commandName}'.\r\n\r\n", result.Error);
        }

        /// <summary>
        /// https://github.com/kohsuke/winsw/issues/206
        /// </summary>
        [Fact(Skip = "unknown")]
        public void ShouldNotPrintLogsForStatusCommand()
        {
            string cliOut = Helper.Test(new[] { "status" });
            Assert.Equal("NonExistent" + Environment.NewLine, cliOut);
        }

        [Fact]
        public void Customize()
        {
            const string OldCompanyName = "CloudBees, Inc.";
            const string NewCompanyName = "CLOUDBEES, INC.";

            string inputPath = Layout.WinSWExe;

            Assert.Equal(OldCompanyName, FileVersionInfo.GetVersionInfo(inputPath).CompanyName);

            // deny write access
            using var file = File.OpenRead(inputPath);

            string outputPath = Path.GetTempFileName();
            Program.TestExecutablePath = inputPath;
            try
            {
                _ = Helper.Test(new[] { "customize", "-o", outputPath, "--manufacturer", NewCompanyName });

                Assert.Equal(NewCompanyName, FileVersionInfo.GetVersionInfo(outputPath).CompanyName);
            }
            finally
            {
                Program.TestExecutablePath = null;
                File.Delete(outputPath);
            }
        }

        [ElevatedFact]
        public void RestartConsoleAppTest()
        {
            using var config = Helper.TestXmlServiceConfig.FromXml(Helper.SeedXml);

            try
            {
                using var controller = CommandLineTestsUtils.ExecuteInstall(config);

#if NET
                InterProcessCodeCoverageSession session = null;
                try
                {
                    try
                    {
                        session = CommandLineTestsUtils.ExecuteStart(config, controller);
                        session = CommandLineTestsUtils.ExecuteStart(config, controller, true);
                    }
                    finally
                    {
                        CommandLineTestsUtils.ExecuteStop(config, controller);
                    }
                }
                finally
                {
                    session?.Wait();
                }
#endif
            }
            finally
            {
                CommandLineTestsUtils.ExecuteUninstall(config);
            }
        }

        [ElevatedFact]
        public void StatusConsoleAppBeforeAndAfterStartTest()
        {
            using var config = Helper.TestXmlServiceConfig.FromXml(Helper.SeedXml);

            try
            {
                using var controller = CommandLineTestsUtils.ExecuteInstall(config);

#if NET
                InterProcessCodeCoverageSession session = null;
                try
                {
                    try
                    {
                        Assert.Contains("Inactive (stopped)", CommandLineTestsUtils.ExecuteStatus(config));

                        session = CommandLineTestsUtils.ExecuteStart(config, controller);

                        Assert.Contains("Active (running)", CommandLineTestsUtils.ExecuteStatus(config));
                    }
                    finally
                    {
                        if (controller.Status == ServiceControllerStatus.Running)
                        {
                            CommandLineTestsUtils.ExecuteStop(config, controller);
                        }

                        Assert.Contains("Inactive (stopped)", CommandLineTestsUtils.ExecuteStatus(config));
                    }
                }
                finally
                {
                    session?.Wait();
                }
#endif
            }
            finally
            {
                CommandLineTestsUtils.ExecuteUninstall(config);
            }
        }

        [ElevatedFact]
        public void RefreshConsoleAppTest()
        {
            using var config = Helper.TestXmlServiceConfig.FromXml(Helper.SeedXml);

            try
            {
                using var controller = CommandLineTestsUtils.ExecuteInstall(config);

                Assert.Equal(Helper.DisplayName, controller.DisplayName);

                var newXml = Helper.CreateSeedXml("This is a test app");
                using var newConfig = Helper.TestXmlServiceConfig.FromXml(newXml, "RefreshConsoleAppTest_Bis");
                using var refreshController = CommandLineTestsUtils.ExecuteRefresh(newConfig);
                Assert.Equal("This is a test app", refreshController.DisplayName);
            }
            finally
            {
                CommandLineTestsUtils.ExecuteUninstall(config);
            }
        }

        [ElevatedFact]
        public void DevListConsoleAppNotRunTest()
        {
            using var config = Helper.TestXmlServiceConfig.FromXml(Helper.SeedXml);

            try
            {
                using var controller = CommandLineTestsUtils.ExecuteInstall(config);

                Program.TestExecutablePath = Layout.WinSWExe;

                Assert.Contains($"{config.DisplayName} ({config.Name})", CommandLineTestsUtils.ExecuteDevList());
            }
            finally
            {
                Program.TestExecutablePath = null;
                CommandLineTestsUtils.ExecuteUninstall(config);
            }
        }
    }
}
