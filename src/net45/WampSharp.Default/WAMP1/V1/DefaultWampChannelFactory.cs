﻿using Newtonsoft.Json.Linq;
using WampSharp.Core.Serialization;
using WampSharp.Newtonsoft;

namespace WampSharp.V1
{
    public class DefaultWampChannelFactory : WampChannelFactory<JToken>
    {
        public DefaultWampChannelFactory()
            : this(new JsonFormatter())
        {
        }

        public DefaultWampChannelFactory(IWampFormatter<JToken> formatter)
            : base(formatter)
        {
        }
    }
}