using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;

namespace WorkspaceGraphs.Extensions
{
    public static class AsyncSessionExtensions
    {
        public static async Task<IResultSummary> RunAndConsumeAsync(this IAsyncSession session, string query)
        {
            var cursor = await session
                .RunAsync(query)
                .ConfigureAwait(false);

            return await cursor
                .ConsumeAsync()
                .ConfigureAwait(false);
        }

        public static async Task<IResultSummary> RunAndConsumeAsync(
            this IAsyncSession session,
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

        public static async Task<IResultSummary> RunAndConsumeAsync(
            this IAsyncSession session,
            string query,
            IDictionary<string, object> parameters)
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

