﻿using System.Collections.Concurrent;
using WampSharp.V2.Binding;
using WampSharp.V2.Core.Contracts;

namespace WampSharp.V2.Realm.Binded
{
    internal class WampBindedRealmContainer<TMessage> : IWampBindedRealmContainer<TMessage>
    {
        private readonly IWampHostedRealmContainer mRealmContainer;
        private readonly IWampSessionServer<TMessage> mSession;
        private readonly IWampEventSerializer<TMessage> mEventSerializer;
        private readonly IWampBinding<TMessage> mBinding;

        private readonly ConcurrentDictionary<string, IWampBindedRealm<TMessage>> mRealmNameToRealm =
            new ConcurrentDictionary<string, IWampBindedRealm<TMessage>>();


        public WampBindedRealmContainer(IWampHostedRealmContainer realmContainer,
                                  IWampSessionServer<TMessage> session,
                                  IWampEventSerializer<TMessage> eventSerializer,
                                  IWampBinding<TMessage> binding)
        {
            mSession = session;
            mEventSerializer = eventSerializer;
            mBinding = binding;
            mRealmContainer = realmContainer;
        }

        public IWampBindedRealm<TMessage> GetRealmByName(string name)
        {
            return mRealmNameToRealm.GetOrAdd(name, realmName => CreateRealm(realmName));
        }

        private IWampBindedRealm<TMessage> CreateRealm(string realmName)
        {
            IWampHostedRealm realm = mRealmContainer.GetRealmByName(realmName);

            return new WampBindedRealm<TMessage>(realm,
                                           mSession,
                                           mEventSerializer,
                                           mBinding);
        }
    }
}