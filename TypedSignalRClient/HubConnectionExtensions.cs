using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace TypedSignalRClient
{
    public static class HubConnectionExtensions
    {
        public static IDisposable AddClientHandler<TClient>(
            this HubConnection hubConnection,
            TClient client
        )
        {
            if (hubConnection == null)
            {
                throw new ArgumentNullException(nameof(hubConnection));
            }

            if (client == null)
            {
                throw new ArgumentNullException(nameof(client));
            }

            var methods = GetClientInterfaceMethods<TClient>();

            var handlerRegistrations = new List<IDisposable>();

            foreach (var method in methods)
            {
                var parameterTypes = method
                    .GetParameters()
                    .Select(parameter => parameter.ParameterType)
                    .ToArray();

                var handlerRegistration = hubConnection.On(
                    method.Name,
                    parameterTypes,
                    parameters => (Task) method.Invoke(client, parameters)
                );

                handlerRegistrations.Add(handlerRegistration);
            }

            var handlersRegistration = new HandlersRegistration(handlerRegistrations);

            return handlersRegistration;
        }


        public static void RemoveClientHandler<TClient>(
            this HubConnection hubConnection
        )
        {
            if (hubConnection == null)
            {
                throw new ArgumentNullException(nameof(hubConnection));
            }

            var methods = GetClientInterfaceMethods<TClient>();

            foreach (var method in methods)
            {
                hubConnection.Remove(method.Name);
            }
        }


        private static MethodInfo[] GetClientInterfaceMethods<TClient>()
        {
            var type = typeof(TClient);

            if (!type.IsInterface)
            {
                Fail();
            }

            var methods = EnummerateMethods().ToArray();

            return methods;


            IEnumerable<MethodInfo> EnummerateMethods()
            {
                foreach (var currentType in new[] { type }.Concat(type.GetInterfaces()))
                {
                    foreach (var currentMethod in currentType.GetMethods())
                    {
                        if (currentMethod.ReturnType != typeof(Task))
                        {
                            Fail();
                        }

                        yield return currentMethod;
                    }
                }
            }

            void Fail()
            {
                throw new ArgumentException(
                    "A client should be an interface, having every method returning Task.",
                    nameof(TClient)
                );
            }
        }
    }
}
