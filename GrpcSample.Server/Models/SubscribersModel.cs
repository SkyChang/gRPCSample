using Duplex;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcSample.Server.Models
{
    public class SubscribersModel
    {
        public IServerStreamWriter<MyMessage> Subscriber { get; set; }

        public string Name { get; set; }
    }

}
