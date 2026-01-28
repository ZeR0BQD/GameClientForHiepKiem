using System;
using System.Collections.Generic;

namespace GameClient
{
    /// <summary>
    /// Du lieu kem theo su kien CMD
    /// </summary>
    public class CmdEventArgs
    {
        public int ClientId { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public TCPGameServerCmds Cmd { get; set; }
        public object? Data { get; set; }
    }

    /// <summary>
    /// Observer trung gian: AIClient -> Observer -> AIManager
    /// Su dung Dictionary de map CMD -> List<Handler>
    /// </summary>
    public static class ClientStateObserver
    {
        private static readonly Dictionary<TCPGameServerCmds, List<Action<CmdEventArgs>>> _handlers
            = new Dictionary<TCPGameServerCmds, List<Action<CmdEventArgs>>>();
        private static readonly object _lock = new object();

        /// <summary>
        /// Subscribe mot CMD cu the
        /// </summary>
        public static void Subscribe(TCPGameServerCmds cmd, Action<CmdEventArgs> handler)
        {
            lock (_lock)
            {
                if (!_handlers.ContainsKey(cmd))
                    _handlers[cmd] = new List<Action<CmdEventArgs>>();
                _handlers[cmd].Add(handler);
            }
        }

        /// <summary>
        /// Unsubscribe
        /// </summary>
        public static void Unsubscribe(TCPGameServerCmds cmd, Action<CmdEventArgs> handler)
        {
            lock (_lock)
            {
                if (_handlers.ContainsKey(cmd))
                    _handlers[cmd].Remove(handler);
            }
        }

        /// <summary>
        /// AIClient goi de notify cac subscriber
        /// </summary>
        public static void Notify(TCPGameServerCmds cmd, CmdEventArgs args)
        {
            List<Action<CmdEventArgs>> handlersCopy;
            lock (_lock)
            {
                if (!_handlers.TryGetValue(cmd, out var handlers) || handlers.Count == 0)
                    return;
                handlersCopy = new List<Action<CmdEventArgs>>(handlers);
            }

            foreach (var handler in handlersCopy)
            {
                try
                {
                    handler?.Invoke(args);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ClientStateObserver] Error in handler: {0}", ex.Message);
                }
            }
        }
    }
}
