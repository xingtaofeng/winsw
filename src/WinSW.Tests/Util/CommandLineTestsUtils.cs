using System;
using System.IO;
using System.ServiceProcess;
using Xunit;
using Helper = WinSW.Tests.Util.CommandLineTestHelper;

namespace WinSW.Tests.Util
{
    internal static class CommandLineTestsUtils
    {
        internal static ServiceController ExecuteInstall(Helper.TestXmlServiceConfig config, string expectedName = Helper.DisplayName)
        {
            Helper.Test(["install", config.FullPath], config);
            var controller = new ServiceController(Helper.Name);
            Assert.Equal(expectedName, controller.DisplayName);
            Assert.False(controller.CanStop);
            Assert.False(controller.CanShutdown);
            Assert.False(controller.CanPauseAndContinue);
            Assert.Equal(ServiceControllerStatus.Stopped, controller.Status);
            Assert.Equal(ServiceType.Win32OwnProcess, controller.ServiceType);
            return controller;
        }

        internal static void ExecuteUninstall(Helper.TestXmlServiceConfig config) =>
            Helper.Test(["uninstall", config.FullPath], config);

        internal static InterProcessCodeCoverageSession ExecuteStart(Helper.TestXmlServiceConfig config, ServiceController controller, bool isRestart = false)
        {
            var command = isRestart ? "restart" : "start";
            Helper.Test([command, config.FullPath], config);
            controller.Refresh();
            Assert.Equal(ServiceControllerStatus.Running, controller.Status);
            Assert.True(controller.CanStop);

            var wrapperOutput = File.ReadAllText(Path.ChangeExtension(config.FullPath, ".wrapper.log"));
            Assert.EndsWith(ServiceMessages.StartedSuccessfully + Environment.NewLine, wrapperOutput);

            if (Environment.GetEnvironmentVariable("System.DefinitionId") != null)
            {
                return new InterProcessCodeCoverageSession(Helper.Name);
            }

            return null;
        }

        internal static void ExecuteStop(Helper.TestXmlServiceConfig config, ServiceController controller)
        {
            Helper.Test(["stop", config.FullPath], config);
            controller.Refresh();
            Assert.Equal(ServiceControllerStatus.Stopped, controller.Status);

            var wrapperOutput = File.ReadAllText(Path.ChangeExtension(config.FullPath, ".wrapper.log"));
            Assert.EndsWith(ServiceMessages.StoppedSuccessfully + Environment.NewLine, wrapperOutput);
        }

        internal static string ExecuteStatus(Helper.TestXmlServiceConfig config) =>
            Helper.Test(["status", config.FullPath], config);

        internal static ServiceController ExecuteRefresh(Helper.TestXmlServiceConfig config)
        {
            Helper.Test(["refresh", config.FullPath], config);
            var controller = new ServiceController(config.Name);
            Assert.Equal(config.DisplayName, controller.DisplayName);
            return controller;
        }

        internal static string ExecuteDevList() =>
            Helper.Test(["dev", "list"]);
    }
}
