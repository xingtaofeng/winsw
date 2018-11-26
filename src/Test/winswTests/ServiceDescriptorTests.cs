﻿using System;
using System.Diagnostics;
using System.ServiceProcess;
using NUnit.Framework;
using winsw;
using winswTests.Util;

namespace winswTests
{
    [TestFixture]
    public class ServiceDescriptorTests
    {
        private ServiceDescriptor _extendedServiceDescriptor;

        private const string ExpectedWorkingDirectory = @"Z:\Path\SubPath";
        private const string Username = "User";
        private const string Password = "Password";
        private const string Domain = "Domain";
        private const string AllowServiceAccountLogonRight = "true";

        [SetUp]
        public void SetUp()
        {
            string seedXml =
$@"<service>
  <id>service.exe</id>
  <name>Service</name>
  <description>The service.</description>
  <executable>node.exe</executable>
  <arguments>My Arguments</arguments>
  <log mode=""roll""></log>
  <serviceaccount>
    <username>{Domain}\{Username}</username>
    <password>{Password}</password>
    <allowservicelogon>{AllowServiceAccountLogonRight}</allowservicelogon>
  </serviceaccount>
  <workingdirectory>{ExpectedWorkingDirectory}</workingdirectory>
  <logpath>C:\logs</logpath>
</service>";
            _extendedServiceDescriptor = ServiceDescriptor.FromXML(seedXml);
        }

        [Test]
        public void DefaultStartMode()
        {
            Assert.That(_extendedServiceDescriptor.StartMode, Is.EqualTo(ServiceStartMode.Automatic));
        }

        [Test]
        public void IncorrectStartMode()
        {
            string seedXml =
$@"<service>
  <id>service.exe</id>
  <name>Service</name>
  <description>The service.</description>
  <executable>node.exe</executable>
  <arguments>My Arguments</arguments>
  <startmode>roll</startmode>
  <log mode=""roll""></log>
  <serviceaccount>
    <username>{Domain}\{Username}</username>
    <password>{Password}</password>
    <allowservicelogon>{AllowServiceAccountLogonRight}</allowservicelogon>
  </serviceaccount>
  <workingdirectory>{ExpectedWorkingDirectory}</workingdirectory>
  <logpath>C:\logs</logpath>
</service>";

            _extendedServiceDescriptor = ServiceDescriptor.FromXML(seedXml);
            Assert.That(() => _extendedServiceDescriptor.StartMode, Throws.ArgumentException);
        }

        [Test]
        public void ChangedStartMode()
        {
            string seedXml =
$@"<service>
  <id>service.exe</id>
  <name>Service</name>
  <description>The service.</description>
  <executable>node.exe</executable>
  <arguments>My Arguments</arguments>
  <startmode>manual</startmode>
  <log mode=""roll""></log>
  <serviceaccount>
    <username>{Domain}\{Username}</username>
    <password>{Password}</password>
    <allowservicelogon>{AllowServiceAccountLogonRight}</allowservicelogon>
  </serviceaccount>
  <workingdirectory>{ExpectedWorkingDirectory}</workingdirectory>
  <logpath>C:\logs</logpath>
</service>";

            _extendedServiceDescriptor = ServiceDescriptor.FromXML(seedXml);
            Assert.That(_extendedServiceDescriptor.StartMode, Is.EqualTo(ServiceStartMode.Manual));
        }

        [Test]
        public void VerifyWorkingDirectory()
        {
            Debug.WriteLine("_extendedServiceDescriptor.WorkingDirectory :: " + _extendedServiceDescriptor.WorkingDirectory);
            Assert.That(_extendedServiceDescriptor.WorkingDirectory, Is.EqualTo(ExpectedWorkingDirectory));
        }

        [Test]
        public void VerifyServiceLogonRight()
        {
            Assert.That(_extendedServiceDescriptor.AllowServiceAcountLogonRight, Is.True);
        }

        [Test]
        public void VerifyUsername()
        {
            Debug.WriteLine("_extendedServiceDescriptor.WorkingDirectory :: " + _extendedServiceDescriptor.WorkingDirectory);
            Assert.That(_extendedServiceDescriptor.ServiceAccountUserName, Is.EqualTo(Domain + "\\" + Username));
        }

        [Test]
        public void VerifyPassword()
        {
            Debug.WriteLine("_extendedServiceDescriptor.WorkingDirectory :: " + _extendedServiceDescriptor.WorkingDirectory);
            Assert.That(_extendedServiceDescriptor.ServiceAccountPassword, Is.EqualTo(Password));
        }

        [Test]
        public void Priority()
        {
            var sd = ServiceDescriptor.FromXML("<service><id>test</id><priority>normal</priority></service>");
            Assert.That(sd.Priority, Is.EqualTo(ProcessPriorityClass.Normal));

            sd = ServiceDescriptor.FromXML("<service><id>test</id><priority>idle</priority></service>");
            Assert.That(sd.Priority, Is.EqualTo(ProcessPriorityClass.Idle));

            sd = ServiceDescriptor.FromXML("<service><id>test</id></service>");
            Assert.That(sd.Priority, Is.EqualTo(ProcessPriorityClass.Normal));
        }

        [Test]
        public void StopParentProcessFirstIsFalseByDefault()
        {
            Assert.That(_extendedServiceDescriptor.StopParentProcessFirst, Is.False);
        }

        [Test]
        public void CanParseStopParentProcessFirst()
        {
            const string seedXml = "<service>"
                                   + "<stopparentprocessfirst>true</stopparentprocessfirst>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);

            Assert.That(serviceDescriptor.StopParentProcessFirst, Is.True);
        }

        [Test]
        public void CanParseStopTimeout()
        {
            const string seedXml = "<service>"
                                   + "<stoptimeout>60sec</stoptimeout>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);

            Assert.That(serviceDescriptor.StopTimeout, Is.EqualTo(TimeSpan.FromSeconds(60)));
        }

        [Test]
        public void CanParseStopTimeoutFromMinutes()
        {
            const string seedXml = "<service>"
                                   + "<stoptimeout>10min</stoptimeout>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);

            Assert.That(serviceDescriptor.StopTimeout, Is.EqualTo(TimeSpan.FromMinutes(10)));
        }

        [Test]
        public void CanParseLogname()
        {
            const string seedXml = "<service>"
                                   + "<logname>MyTestApp</logname>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);

            Assert.That(serviceDescriptor.LogName, Is.EqualTo("MyTestApp"));
        }

        [Test]
        public void CanParseOutfileDisabled()
        {
            const string seedXml = "<service>"
                                   + "<outfiledisabled>true</outfiledisabled>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);

            Assert.That(serviceDescriptor.OutFileDisabled, Is.True);
        }

        [Test]
        public void CanParseErrfileDisabled()
        {
            const string seedXml = "<service>"
                                   + "<errfiledisabled>true</errfiledisabled>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);

            Assert.That(serviceDescriptor.ErrFileDisabled, Is.True);
        }

        [Test]
        public void CanParseOutfilePattern()
        {
            const string seedXml = "<service>"
                                   + "<outfilepattern>.out.test.log</outfilepattern>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);

            Assert.That(serviceDescriptor.OutFilePattern, Is.EqualTo(".out.test.log"));
        }

        [Test]
        public void CanParseErrfilePattern()
        {
            const string seedXml = "<service>"
                                   + "<errfilepattern>.err.test.log</errfilepattern>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);

            Assert.That(serviceDescriptor.ErrFilePattern, Is.EqualTo(".err.test.log"));
        }

        [Test]
        public void LogModeRollBySize()
        {
            const string seedXml = "<service>"
                                   + "<logpath>c:\\</logpath>"
                                   + "<log mode=\"roll-by-size\">"
                                   + "<sizeThreshold>112</sizeThreshold>"
                                   + "<keepFiles>113</keepFiles>"
                                   + "</log>"
                                   + "</service>";

            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);
            serviceDescriptor.BaseName = "service";

            var logHandler = serviceDescriptor.LogHandler as SizeBasedRollingLogAppender;
            Assert.That(logHandler, Is.Not.Null);
            Assert.That(logHandler.SizeTheshold, Is.EqualTo(112 * 1024));
            Assert.That(logHandler.FilesToKeep, Is.EqualTo(113));
        }

        [Test]
        public void LogModeRollByTime()
        {
            const string seedXml = "<service>"
                                   + "<logpath>c:\\</logpath>"
                                   + "<log mode=\"roll-by-time\">"
                                   + "<period>7</period>"
                                   + "<pattern>log pattern</pattern>"
                                   + "</log>"
                                   + "</service>";

            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);
            serviceDescriptor.BaseName = "service";

            var logHandler = serviceDescriptor.LogHandler as TimeBasedRollingLogAppender;
            Assert.That(logHandler, Is.Not.Null);
            Assert.That(logHandler.Period, Is.EqualTo(7));
            Assert.That(logHandler.Pattern, Is.EqualTo("log pattern"));
        }

        [Test]
        public void LogModeRollBySizeTime()
        {
            const string seedXml = "<service>"
                                   + "<logpath>c:\\</logpath>"
                                   + "<log mode=\"roll-by-size-time\">"
                                   + "<sizeThreshold>10240</sizeThreshold>"
                                   + "<pattern>yyyy-MM-dd</pattern>"
                                   + "<autoRollAtTime>00:00:00</autoRollAtTime>"
                                   + "</log>"
                                   + "</service>";

            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);
            serviceDescriptor.BaseName = "service";

            var logHandler = serviceDescriptor.LogHandler as RollingSizeTimeLogAppender;
            Assert.That(logHandler, Is.Not.Null);
            Assert.That(logHandler.SizeTheshold, Is.EqualTo(10240 * 1024));
            Assert.That(logHandler.FilePattern, Is.EqualTo("yyyy-MM-dd"));
            Assert.That(logHandler.AutoRollAtTime, Is.EqualTo((TimeSpan?)new TimeSpan(0, 0, 0)));
        }

        [Test]
        public void VerifyServiceLogonRightGraceful()
        {
            const string seedXml = "<service>"
                                   + "<serviceaccount>"
                                   + "<domain>" + Domain + "</domain>"
                                   + "<user>" + Username + "</user>"
                                   + "<password>" + Password + "</password>"
                                   + "<allowservicelogon>true1</allowservicelogon>"
                                   + "</serviceaccount>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);
            Assert.That(serviceDescriptor.AllowServiceAcountLogonRight, Is.False);
        }

        [Test]
        public void VerifyServiceLogonRightOmitted()
        {
            const string seedXml = "<service>"
                                   + "<serviceaccount>"
                                   + "<domain>" + Domain + "</domain>"
                                   + "<user>" + Username + "</user>"
                                   + "<password>" + Password + "</password>"
                                   + "</serviceaccount>"
                                   + "</service>";
            var serviceDescriptor = ServiceDescriptor.FromXML(seedXml);
            Assert.That(serviceDescriptor.AllowServiceAcountLogonRight, Is.False);
        }

        [Test]
        public void VerifyWaitHint_FullXML()
        {
            var sd = ConfigXmlBuilder.create()
                .WithTag("waithint", "20 min")
                .ToServiceDescriptor(true);
            Assert.That(sd.WaitHint, Is.EqualTo(TimeSpan.FromMinutes(20)));
        }

        /// <summary>
        /// Test for https://github.com/kohsuke/winsw/issues/159
        /// </summary>
        [Test]
        public void VerifyWaitHint_XMLWithoutVersion()
        {
            var sd = ConfigXmlBuilder.create(printXMLVersion: false)
                .WithTag("waithint", "21 min")
                .ToServiceDescriptor(true);
            Assert.That(sd.WaitHint, Is.EqualTo(TimeSpan.FromMinutes(21)));
        }

        [Test]
        public void VerifyWaitHint_XMLWithoutComment()
        {
            var sd = ConfigXmlBuilder.create(xmlComment: null)
                .WithTag("waithint", "22 min")
                .ToServiceDescriptor(true);
            Assert.That(sd.WaitHint, Is.EqualTo(TimeSpan.FromMinutes(22)));
        }

        [Test]
        public void VerifyWaitHint_XMLWithoutVersionAndComment()
        {
            var sd = ConfigXmlBuilder.create(xmlComment: null, printXMLVersion: false)
                .WithTag("waithint", "23 min")
                .ToServiceDescriptor(true);
            Assert.That(sd.WaitHint, Is.EqualTo(TimeSpan.FromMinutes(23)));
        }

        [Test]
        public void VerifySleepTime()
        {
            var sd = ConfigXmlBuilder.create().WithTag("sleeptime", "3 hrs").ToServiceDescriptor(true);
            Assert.That(sd.SleepTime, Is.EqualTo(TimeSpan.FromHours(3)));
        }

        [Test]
        public void VerifyResetFailureAfter()
        {
            var sd = ConfigXmlBuilder.create().WithTag("resetfailure", "75 sec").ToServiceDescriptor(true);
            Assert.That(sd.ResetFailureAfter, Is.EqualTo(TimeSpan.FromSeconds(75)));
        }

        [Test]
        public void VerifyStopTimeout()
        {
            var sd = ConfigXmlBuilder.create().WithTag("stoptimeout", "35 secs").ToServiceDescriptor(true);
            Assert.That(sd.StopTimeout, Is.EqualTo(TimeSpan.FromSeconds(35)));
        }

        /// <summary>
        /// https://github.com/kohsuke/winsw/issues/178
        /// </summary>
        [Test]
        public void Arguments_LegacyParam()
        {
            var sd = ConfigXmlBuilder.create().WithTag("arguments", "arg").ToServiceDescriptor(true);
            Assert.That(sd.Arguments, Is.EqualTo("arg"));
        }

        [Test]
        public void Arguments_NewParam_Single()
        {
            var sd = ConfigXmlBuilder.create()
                .WithTag("argument", "--arg1=2")
                .ToServiceDescriptor(true);
            Assert.That(sd.Arguments, Is.EqualTo(" --arg1=2"));
        }

        [Test]
        public void Arguments_NewParam_MultipleArgs()
        {
            var sd = ConfigXmlBuilder.create()
                .WithTag("argument", "--arg1=2")
                .WithTag("argument", "--arg2=123")
                .WithTag("argument", "--arg3=null")
                .ToServiceDescriptor(true);
            Assert.That(sd.Arguments, Is.EqualTo(" --arg1=2 --arg2=123 --arg3=null"));
        }

        /// <summary>
        /// Ensures that the new single-argument field has a higher priority.
        /// </summary>
        [Test]
        public void Arguments_Bothparam_Priorities()
        {
            var sd = ConfigXmlBuilder.create()
                .WithTag("arguments", "--arg1=2 --arg2=3")
                .WithTag("argument", "--arg2=123")
                .WithTag("argument", "--arg3=null")
                .ToServiceDescriptor(true);
            Assert.That(sd.Arguments, Is.EqualTo(" --arg2=123 --arg3=null"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DelayedStart_RoundTrip(bool enabled)
        {
            var bldr = ConfigXmlBuilder.create();
            if (enabled)
            {
                bldr = bldr.WithDelayedAutoStart();
            }

            var sd = bldr.ToServiceDescriptor();
            Assert.That(sd.DelayedAutoStart, Is.EqualTo(enabled));
        }
    }
}
