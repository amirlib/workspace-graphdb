using System.Threading.Tasks;
using Neo4j.Driver;

namespace WorkspaceGraphs.Extensions
{
    public static class AsyncTransactionExtensions
	{
        public static async Task<IResultSummary> RunAndConsumeAsync(
            this IAsyncTransaction session,
            string query)
        {
            var cursor = await session
                .RunAsync(query)
                .ConfigureAwait(false);

            return await cursor
                .ConsumeAsync()
                .ConfigureAwait(false);
        }

        public static async Task<IResultSummary> RunAndConsumeAsync(
            this IAsyncTransaction session,
            string query,
            object parameters)
        {
            var cursor = await session
                .RunAsync(query, parameters)
                .ConfigureAwait(false);

            return await cursor
                .ConsumeAsync()
                .ConfigureAwait(false);
        }
    }
}

