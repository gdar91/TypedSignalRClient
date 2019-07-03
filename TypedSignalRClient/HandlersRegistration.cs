using System;
using System.Collections.Generic;

namespace TypedSignalRClient
{
    internal class HandlersRegistration : IDisposable
    {
        private readonly List<IDisposable> disposables;

        public HandlersRegistration(List<IDisposable> disposables)
        {
            this.disposables = disposables;
        }

        public void Dispose()
        {
            try
            {
                foreach (var disposable in disposables)
                {
                    using (var resource = disposable)
                    { }
                }
            }
            catch
            { }
        }
    }
}
