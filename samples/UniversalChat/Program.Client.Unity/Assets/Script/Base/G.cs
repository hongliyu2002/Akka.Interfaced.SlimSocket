﻿using Common.Logging;
using UniversalChat.Interface;
using Akka.Interfaced.SlimSocket.Client;

public static class G
{
    static G()
    {
        _logger = LogManager.GetLogger("G");
        _debugLogAdapter = new UnityDebugLogAdapter(LogLevel.All);
        _debugLogAdapter.Attach();
    }

    // Communicator

    private static Communicator _comm;

    public static Communicator Comm
    {
        get { return _comm; }
        set
        {
            _comm = value;
            _comm.ObserverEventPoster = c => ApplicationComponent.Post(c, null);

            _slimRequestWaiter = new SlimRequestWaiter(_comm, ApplicationComponent.Instance);
        }
    }

    private static SlimRequestWaiter _slimRequestWaiter;

    public static SlimRequestWaiter SlimRequestWaiter
    {
        get { return _slimRequestWaiter; }
    }

    // Logger

    private static readonly ILog _logger;

    public static ILog Logger
    {
        get { return _logger; }
    }

    private static readonly UnityDebugLogAdapter _debugLogAdapter;

    public static UnityDebugLogAdapter DebugLogAdapter
    {
        get { return _debugLogAdapter; }
    }

    // Chat specific data

    public static UserRef User
    {
        get; set;
    }

    public static string UserId
    {
        get; set;
    }
}