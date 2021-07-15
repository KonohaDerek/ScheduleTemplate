using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace ScheduleTemplate.Jobs.Tests
{
    [TestClass()]
    public class JobTestTests
    {
        [TestMethod()]
        public async Task ExecuteAsyncTestAsync()
        {
            var job = new JobTest();
            try
            {
                await job.ExecuteAsync(new System.Threading.CancellationToken());
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}