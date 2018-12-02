using System.Fabric;
using VotingData.Handlers;
using VotingDatabase;

namespace VotingData
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Autofac;
    //using Autofac.Integration.ServiceFabric;

    internal static class Program
    {
        /// <summary>
        /// This is the entry point of the service host process.
        /// </summary>
        private static void Main()
        {
            try
            {
                // Register any regular dependencies.
                var builder = new ContainerBuilder();
                var databaseParams = VotingDatabaseParameters.GetDatabaseConsumerParameters(FabricRuntime.GetActivationContext());
                builder.Register(c => databaseParams).As<VotingDatabaseParameters>().SingleInstance();
                //builder.RegisterType<OtpVerificationHandler>().As<IOtpVerificationHandler>().SingleInstance();
                //builder.Register(c => SetupOtpVerificationHandler(c.Resolve<VotingDatabaseParameters>())).As<IOtpVerificationHandler>().SingleInstance();
                // The ServiceManifest.XML file defines one or more service type names.
                // Registering a service maps a service type name to a .NET type.
                // When Service Fabric creates an instance of this service type,
                // an instance of the class is created in this host process.

                ServiceRuntime.RegisterServiceAsync(
                    "VotingDataType",
                    context => new VotingData(context)).GetAwaiter().GetResult();

                ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(VotingData).Name);

                // Prevents this host process from terminating so services keeps running. 
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// Setup otp verification handler.
        /// </summary>
        /// <param name="votingDatabaseParameters"></param>
        /// <returns></returns>
        //private static IOtpVerificationHandler SetupOtpVerificationHandler(VotingDatabaseParameters votingDatabaseParameters)
            //=> new OtpVerificationHandler(votingDatabaseParameters);
    }
}