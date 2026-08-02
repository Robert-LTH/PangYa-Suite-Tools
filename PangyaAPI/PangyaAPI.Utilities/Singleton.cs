using System;
using PangyaAPI.Utilities.Resources;

namespace PangyaAPI.Utilities
{
    public class Singleton<_ST> where _ST : class
    {
        private static readonly object SyncRoot = new object();

        // Retained for source compatibility with the original Suite Tools API.
        public static _ST myInstance = null!;

        public static _ST getInstance()
        {
            if (myInstance == null)
            {
                lock (SyncRoot)
                {
                    if (myInstance == null)
                    {
                        myInstance = Activator.CreateInstance<_ST>()
                            ?? throw new InvalidOperationException($"Unable to create singleton instance of {typeof(_ST).FullName}.");
                    }
                }
            }

            return myInstance;
        }

        public static void setInstance(_ST instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            lock (SyncRoot)
            {
                if (myInstance != null && !ReferenceEquals(myInstance, instance))
                {
                    throw new InvalidOperationException(UtilityMessages.Format("SingletonAlreadyConfigured", typeof(_ST).FullName));
                }

                myInstance = instance;
            }
        }

        protected Singleton()
        {
        }
    }
}
