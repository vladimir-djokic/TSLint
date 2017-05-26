using System;
using System.Threading;
using System.Threading.Tasks;

namespace TSLint
{
    internal class TsLintQueue
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly Func<string, Task> _collectTags;

        public TsLintQueue(Func<string, Task> collectTags)
        {
            this._semaphore = new SemaphoreSlim(1);
            this._collectTags = collectTags;
        }

        internal async Task Enqueue(string tsFilename)
        {
            await this._semaphore.WaitAsync();

            try
            {
                await this._collectTags(tsFilename);
            }
            finally
            {
                this._semaphore.Release();
            }
        }
    }
}
