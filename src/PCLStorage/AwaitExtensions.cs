using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCLStorage
{
    /// <summary>
    /// Extensions for use internally by PCLStorage for awaiting.
    /// </summary>
    internal static partial class AwaitExtensions
    {
        /// <summary>
        /// Causes the caller who awaits this method to
        /// switch off the Main thread. It has no effect if
        /// the caller is already off the main thread.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An awaitable that does the thread switching magic.</returns>
        internal static TaskSchedulerAwaiter SwitchOffMainThreadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return new TaskSchedulerAwaiter(
                SynchronizationContext.Current != null ? TaskScheduler.Default : null,
                cancellationToken);
        }

        internal struct TaskSchedulerAwaiter : INotifyCompletion
        {
            private TaskScheduler taskScheduler;
            private CancellationToken cancellationToken;

            internal TaskSchedulerAwaiter(TaskScheduler taskScheduler, CancellationToken cancellationToken)
            {
                this.taskScheduler = taskScheduler;
                this.cancellationToken = cancellationToken;
            }

            internal TaskSchedulerAwaiter GetAwaiter()
            {
                return this;
            }

            public bool IsCompleted
            {
                get { return this.taskScheduler == null; }
            }

            public void OnCompleted(Action continuation)
            {
                if (this.taskScheduler == null)
                {
                    throw new InvalidOperationException("IsCompleted is true, so this is unexpected.");
                }

                Task.Factory.StartNew(
                    continuation,
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    this.taskScheduler);
            }

            public void GetResult()
            {
                this.cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }
}
