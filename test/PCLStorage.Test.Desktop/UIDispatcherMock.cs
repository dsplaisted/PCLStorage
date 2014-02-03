using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCLStorage.Test
{
    internal class UIDispatcherMock : SynchronizationContext
    {
        private readonly object syncObject = new object();
        private readonly Queue<KeyValuePair<SendOrPostCallback, object>> messageQueue = new Queue<KeyValuePair<SendOrPostCallback, object>>();
#if !NETFX_CORE
        private readonly Thread mainThread;
#endif
        private bool continueProcessingMessages;

        internal UIDispatcherMock()
        {
#if !NETFX_CORE
            this.mainThread = Thread.CurrentThread;
#endif
            this.continueProcessingMessages = true;
        }

        public bool Continue
        {
            get
            {
                return this.continueProcessingMessages;
            }

            set
            {
                if (this.continueProcessingMessages != value)
                {
                    this.continueProcessingMessages = value;
                    lock (this.syncObject)
                    {
                        Monitor.Pulse(this.syncObject);
                    }
                }
            }
        }

        public static void MainThreadEntrypoint(Action mainThreadEntrypoint)
        {
            MainThreadEntrypoint(delegate
            {
                mainThreadEntrypoint();
                return Task.FromResult(true);
            });
        }

        public static void MainThreadEntrypoint(Func<Task> mainThreadEntrypoint)
        {
            var syncContext = new UIDispatcherMock();
            var oldSyncContext = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(syncContext);
            Exception unhandledException = null;
            syncContext.Post(
                async state =>
                {
                    try
                    {
                        await mainThreadEntrypoint();
                    }
                    catch (Exception ex)
                    {
                        unhandledException = ex;
                    }
                    finally
                    {
                        syncContext.Continue = false;
                    }
                },
                null);
            syncContext.Loop();
            SynchronizationContext.SetSynchronizationContext(oldSyncContext);
            if (unhandledException != null)
            {
                ExceptionDispatchInfo.Capture(unhandledException).Throw();
            }
        }

        public override void Send(SendOrPostCallback d, object state)
        {
#if NETFX_CORE
            d(state);
#else
            if (Thread.CurrentThread == this.mainThread)
            {
                d(state);
            }
            else
            {
                var completed = new ManualResetEvent(false);
                this.Post(
                    _ =>
                    {
                        try
                        {
                            d(state);
                        }
                        finally
                        {
                            completed.Set();
                        }
                    },
                    null);

                completed.WaitOne();
            }
#endif
        }

        public override void Post(SendOrPostCallback d, object state)
        {
            lock (syncObject)
            {
                messageQueue.Enqueue(new KeyValuePair<SendOrPostCallback, object>(d, state));
                Monitor.Pulse(syncObject);
            }
        }

        public void Loop()
        {
#if !NETFX_CORE
            if (this.mainThread != Thread.CurrentThread)
            {
                throw new InvalidOperationException("Wrong thread!");
            }
#endif

            while (this.Continue)
            {
                var message = this.DequeueMessage();
                if (message.Key != null)
                {
                    message.Key(message.Value);
                }
            }
        }

        private KeyValuePair<SendOrPostCallback, object> DequeueMessage()
        {
            lock (this.syncObject)
            {
                while (this.messageQueue.Count == 0 && this.Continue)
                {
                    Monitor.Wait(this.syncObject);
                }

                return this.Continue ? this.messageQueue.Dequeue() : new KeyValuePair<SendOrPostCallback, object>();
            }
        }
    }
}
