using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ba2Explorer.Settings.Serializer
{
    public class SettingsDeserializationError
    {
        public string PropertyName { get; private set; }

        public Exception Exception { get; private set; }

        public string Message => Exception.Message;

        public SettingsDeserializationError(string propertyName, Exception exception)
        {
            Contract.Requires(propertyName != null);
            Contract.Requires(exception != null);

            PropertyName = propertyName;
            Exception = exception;
        }
    }
}
