﻿using System;
using System.Net;
using Akka.Actor;
using Akka.Interfaced;
using Akka.Interfaced.SlimSocket.Server;
using Common.Logging;
using HelloWorld.Interface;
using Akka.Interfaced.SlimSocket;
using Akka.Interfaced.SlimServer;

namespace HelloWorld.Program.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (typeof(IGreeter) == null)
                throw new Exception("Force interface module to be loaded");

            using (var system = ActorSystem.Create("MySystem", "akka.loglevel = DEBUG \n akka.actor.debug.lifecycle = on"))
            {
                DeadRequestProcessingActor.Install(system);

                var tcpGateway = StartGateway(system, ChannelType.Tcp, 5001);
                var udpGateway = StartGateway(system, ChannelType.Udp, 5001);

                Console.WriteLine("Please enter key to quit.");
                Console.ReadLine();

                tcpGateway.Stop().Wait();
                udpGateway.Stop().Wait();
            }
        }

        private static GatewayRef StartGateway(ActorSystem system, ChannelType type, int port)
        {
            var serializer = PacketSerializer.CreatePacketSerializer();

            var initiator = new GatewayInitiator
            {
                ListenEndPoint = new IPEndPoint(IPAddress.Any, port),
                GatewayLogger = LogManager.GetLogger("Gateway"),
                CreateChannelLogger = (ep, _) => LogManager.GetLogger($"Channel({ep}"),
                ConnectionSettings = new TcpConnectionSettings { PacketSerializer = serializer },
                PacketSerializer = serializer,
                CreateInitialActors = (context, connection) => new[]
                {
                    Tuple.Create(context.ActorOf(Props.Create(() => new EntryActor(context.Self.Cast<ActorBoundChannelRef>()))),
                                 new TaggedType[] { typeof(IEntry) },
                                 (ActorBindingFlags)0)
                }
            };

            var gateway = (type == ChannelType.Tcp)
                ? system.ActorOf(Props.Create(() => new TcpGateway(initiator))).Cast<GatewayRef>()
                : system.ActorOf(Props.Create(() => new UdpGateway(initiator))).Cast<GatewayRef>();
            gateway.Start().Wait();
            return gateway;
        }
    }
}
