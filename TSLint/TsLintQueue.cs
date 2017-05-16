using System;
using System.Threading;
using System.Threading.Tasks;

namespace TSLint
{
    internal class TsLintQueue
    {
        private readonly SemaphoreSlim semaphore;
        private readonly Func<string, Task> collectTags;

        public TsLintQueue(Func<string, Task> collectTags)
        {
            this.semaphore = new SemaphoreSlim(1);
            this.collectTags = collectTags;
        }

        internal async Task Enqueue(string tsFilename)
        {
            await this.semaphore.WaitAsync();

            try
            {
                await this.collectTags(tsFilename);
            }
            finally
            {
                this.semaphore.Release();
            }
        }
    }
}
