using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
#endif
using PCLStorage.TestFramework;


namespace PCLStorage.Test
{
    [TestClass]
    public class FolderTests
    {
        [TestMethod]
        public async Task DeleteAppLocalStorageThrows()
        {
            IFolder folder = Storage.AppLocalStorage;
            await ExceptionAssert.ThrowsAsync<IOException>(async () => await folder.DeleteAsync());
            

            
        }
    }
}
