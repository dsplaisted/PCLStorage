using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PCLStorage.Test
{
    [TestClass]
    public class AwaitExtensionsTest
    {
        [TestMethod]
        public async Task SwitchOffMainThreadAsync_OnMainThread()
        {
            // Make this thread look like the main thread by
            // setting up a synchronization context.
            var dispatcher = new SynchronizationContext();
            var original = SynchronizationContext.Current;
            SynchronizationContext.SetSynchronizationContext(dispatcher);
            try
            {
                Thread originalThread = Thread.CurrentThread;

                await AwaitExtensions.SwitchOffMainThreadAsync(CancellationToken.None);

                Assert.AreNotSame(originalThread, Thread.CurrentThread);
                Assert.IsNull(SynchronizationContext.Current);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(original);
            }
        }

        [TestMethod]
        public void SwitchOffMainThreadAsync_OffMainThread()
        {
            var awaitable = AwaitExtensions.SwitchOffMainThreadAsync(CancellationToken.None);
            var awaiter = awaitable.GetAwaiter();
            Assert.IsTrue(awaiter.IsCompleted); // guarantees the caller wouldn't have switched threads.
        }

        [TestMethod]
        public void SwitchOffMainThreadAsync_CanceledBeforeSwitch()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();
            try
            {
                AwaitExtensions.SwitchOffMainThreadAsync(cts.Token);
                Assert.Fail("Expected OperationCanceledException not thrown.");
            }
            catch (OperationCanceledException ex)
            {
                Assert.AreEqual(cts.Token, ex.CancellationToken);
            }
        }

        [TestMethod]
        public void SwitchOffMainThreadAsync_CanceledMidSwitch()
        {
            var cts = new CancellationTokenSource();
            var awaitable = AwaitExtensions.SwitchOffMainThreadAsync(cts.Token);
            var awaiter = awaitable.GetAwaiter();

            cts.Cancel();

            try
            {
                awaiter.GetResult();
                Assert.Fail("Expected OperationCanceledException not thrown.");
            }
            catch (OperationCanceledException ex)
            {
                Assert.AreEqual(cts.Token, ex.CancellationToken);
            }
        }
    }
}
