using System;

namespace LibVLCSharp.Shared
{
    /// <summary>
    /// Static hook for managed-side diagnostics emitted from inside LibVLCSharp.
    /// Consuming apps subscribe to <see cref="Warning"/> in startup to route library
    /// warnings into Serilog / Microsoft.Extensions.Logging / Sentry / App Insights / etc.
    /// </summary>
    public static class LibVLCSharpDiagnostics
    {
        /// <summary>
        /// Raised when LibVLCSharp swallows or recovers from a managed-side condition
        /// the consumer may want to observe (e.g. the GC-race ObjectDisposedException
        /// in <c>VideoView.Detach()</c> tied to VideoLAN issue #659).
        /// </summary>
        public static event EventHandler<LibVLCSharpWarningEventArgs>? Warning;

        internal static void RaiseWarning(string source, string message, Exception? exception = null)
        {
            var handler = Warning;
            if (handler == null) return;

            try
            {
                handler(null, new LibVLCSharpWarningEventArgs(source, message, exception));
            }
            catch
            {
                // A misbehaving subscriber must not crash the library; we are already on a
                // diagnostics path with no other sink available.
            }
        }
    }

    /// <summary>
    /// Payload for <see cref="LibVLCSharpDiagnostics.Warning"/>.
    /// </summary>
    public sealed class LibVLCSharpWarningEventArgs : EventArgs
    {
        internal LibVLCSharpWarningEventArgs(string source, string message, Exception? exception)
        {
            Source = source;
            Message = message;
            Exception = exception;
        }

        /// <summary>
        /// Short identifier of the emitting code path, e.g. <c>"LibVLCSharp.VideoView"</c>.
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Human-readable message describing what happened and why.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The exception that triggered the warning, if any.
        /// </summary>
        public Exception? Exception { get; }
    }
}
