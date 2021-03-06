﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using WampSharp.Core.Serialization;
using WampSharp.Core.Utilities;
using WampSharp.V2.Core.Contracts;

namespace WampSharp.V2.Rpc
{
    public class AsyncMethodInfoRpcOperation : AsyncLocalRpcOperation
    {
        private readonly object mInstance;
        private readonly MethodInfo mMethod;
        private readonly RpcParameter[] mParameters;
        private readonly bool mHasResult;
        private readonly CollectionResultTreatment mCollectionResultTreatment;

        public AsyncMethodInfoRpcOperation(object instance, MethodInfo method) : 
            this(instance, method, GetProcedure(method))
        {
        }

        public AsyncMethodInfoRpcOperation(object instance, MethodInfo method, string procedureName) :
            base(procedureName)
        {
            mInstance = instance;
            mMethod = method;

            if (method.ReturnType != typeof (Task))
            {
                mHasResult = true;
            }
            else
            {
                mHasResult = false;
            }

            mCollectionResultTreatment = 
                method.GetCollectionResultTreatment();

            mParameters =
                method.GetParameters()
                      .Select(parameter => new RpcParameter(parameter))
                      .ToArray();
        }

        private static string GetProcedure(MethodInfo method)
        {
            WampProcedureAttribute procedureAttribute =
                method.GetCustomAttribute<WampProcedureAttribute>(true);

            if (procedureAttribute == null)
            {
                // throw 
            }

            return procedureAttribute.Procedure;
        }

        public override RpcParameter[] Parameters
        {
            get { return mParameters; }
        }

        public override bool HasResult
        {
            get { return mHasResult; }
        }

        public override CollectionResultTreatment CollectionResultTreatment
        {
            get { return mCollectionResultTreatment; }
        }

        protected override Task<object> InvokeAsync<TMessage>(IWampRawRpcOperationRouterCallback caller, IWampFormatter<TMessage> formatter, InvocationDetails options, TMessage[] arguments, IDictionary<string, TMessage> argumentsKeywords)
        {
            object[] unpacked =
                UnpackParameters(formatter, arguments, argumentsKeywords);

            try
            {
                Task result =
                    mMethod.Invoke(mInstance, unpacked) as Task;

                Task<object> casted = result.CastTask();

                return casted;
            }
            catch (TargetInvocationException ex)
            {
                Exception actual = ex.InnerException;

                if (actual is WampException)
                {
                    throw actual;
                }
                else
                {
                    throw ConvertExceptionToRuntimeException(actual);
                }
            }
        }
    }
}