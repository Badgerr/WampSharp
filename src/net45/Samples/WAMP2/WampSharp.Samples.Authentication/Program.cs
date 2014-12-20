using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampSharp.Binding;
using WampSharp.V2;
using WampSharp.V2.Client;
using WampSharp.V2.Rpc;

namespace WampSharp.Samples.Authentication
{
    public interface IArgumentsService
    {
        [WampProcedure("com.timeservice.now")]
        string timeservice();
    }

    public class CustomAuthenticator : IWampClientAutenticator
    {
        private string[] authenticationMethod;
        private string autenticationId;

        public CustomAuthenticator(string[] authenticationMethod, string autenticationId)
        {
            this.autenticationId = autenticationId;
            this.authenticationMethod = authenticationMethod;
        }

        public ChallengeResult Authenticate(string challenge, ChallengeDetails extra)
        {
            var challengeExtra = extra.OriginalValue.Deserialize<IDictionary<string, object>>();
            var method = (string)challengeExtra["authmethod"];
            if (method != "ticket")
            {
                throw new WampAuthenticationException("don't know how to authenticate using '" + method + "'");
            }

            var result = new ChallengeResult();
            result.Signature = "md5f39d45e1da71cf755a7ee5d5840c7b0d";
            result.Extra = new Dictionary<string, object>() { };
            return result;
        }

        public string[] AuthenticationMethod
        {
            get { return authenticationMethod; }
        }

        public string AutenticationId
        {
            get { return autenticationId; }
        }
    }

    class Program
    {
        private static void Test(IWampRealmServiceProvider serviceProvider)
        {
            IArgumentsService proxy = serviceProvider.GetCalleeProxy<IArgumentsService>();
            var time = proxy.timeservice();
            Console.WriteLine("call result {0}", time);
        }

        private static IWampRealmProxy proxy;

        static void Main(string[] args)
        {
            string url = "ws://127.0.0.1:8080/";
            string realm = "integra-s";
            string[] authmethods = new string[] { "ticket" };
            string authid = "peter";

            DefaultWampChannelFactory channelFactory = new DefaultWampChannelFactory();

            var authenticator = new CustomAuthenticator(new string[] { "custom-auth" }, authid);
            IWampChannel channel = channelFactory.CreateJsonChannel(url, realm, authenticator);
            channel.RealmProxy.Monitor.ConnectionEstablished += Monitor_ConnectionEstablished;
            channel.RealmProxy.Monitor.ConnectionBroken += Monitor_ConnectionBroken;
            Program.proxy = channel.RealmProxy;
            channel.Open().Wait();
            Test(channel.RealmProxy.Services);
            Console.ReadLine();
        }

        static void Monitor_ConnectionEstablished(object sender, V2.Realm.WampSessionEventArgs e)
        {
            var details = e.Details.Deserialize<IDictionary<string, object>>();
            
            Console.WriteLine("connected session with ID " + e.SessionId);
            Console.WriteLine("authenticated using method '" + details["authmethod"] + "' and provider '" + details["authprovider"] + "'");
            Console.WriteLine("authenticated with authid '" + details["authid"] + "' and authrole '" + details["authrole"] + "'");

            //Test(Program.proxy.Services);
        }

        static void Monitor_ConnectionBroken(object sender, V2.Realm.WampSessionCloseEventArgs e)
        {
            Console.WriteLine("disconnected '" + e.Reason);
        }
    }
}