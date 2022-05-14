using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using WorkspaceGraphs.Extensions;
using WorkspaceGraphs.Models;

namespace WorkspaceGraphs.Repositories
{
    public class WorkspacesRepository : IDisposable
	{
        #region Fields
        private readonly IDriver _driver;
        #endregion

        public WorkspacesRepository(IDriver driver)
		{
			_driver = driver;
		}

        /// <summary>
        /// Configurate the database with indexes and constraints
        /// </summary>
        public async Task InitAsync()
        {
            var session = CreateAsyncSession();

            try
            {
                var queries = GetConstraintsQueries();

                queries.AddRange(GetIndexesQueries());

                foreach (var query in queries)
                { 
                    await session.RunAndConsumeAsync(query);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await session.CloseAsync();
            }
        }
        /// <summary>
        /// Resetes the database
        /// </summary>
        public async Task ResetDbAsync()
        {
            var session = CreateAsyncSession();

            try
            {
                var query = @"
                    MATCH (n)
                    DETACH DELETE n";

                await session.RunAndConsumeAsync(query);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                await session.CloseAsync();
            }
        }
        /// <summary>
        /// Creating the directory, its files and relationships
        /// If prevPath given, the current directory will be the child of the prevPath
        /// Files are optional
        /// </summary>
        /// <param name="directory">Directory to create</param>
        /// <param name="files">Files to create</param>
        /// <param name="prevPath">Directory parent's path/param>
        /// <returns></returns>
        public async Task CreateDirectoryAsync(
            Directory directory,
            IList<File> files,
            string prevPath)
        {
            var session = _driver.AsyncSession();

            try
            {
                await session.WriteTransactionAsync(async tx =>
                {
                    var query = BuildCreateDirQuery(prevPath);
                    var parameters = new
                    {
                        directory = directory.AsDictionary(),
                        files = files.Select(f => f.AsDictionary()),
                        prevPath,
                    };

                    await tx.RunAndConsumeAsync(query, parameters);
                });
            }
            finally
            {
                await session.CloseAsync();
            }
        }
        /// <summary>
        /// Matching a directory with the most sub-directories
        /// </summary>
        /// <returns>Path of the directory</returns>
        public async Task<string> GetDirWithMostSubDirAsync()
        {
            var result = new List<string>();
            var session = CreateAsyncSession();

            try
            {
                var query = @"
                    MATCH (d:Directory)-[:HAS_CHILD]->(q:Directory)
                    WITH d, count(d) as connected
                    RETURN d.path
                    ORDER BY connected DESC 
                    LIMIT 1";

                result = await session.ReadTransactionAsync(async tx =>
                {
                    var pathes = new List<string>();
                    var reader = await tx.RunAsync(query);

                    while (await reader.FetchAsync())
                    {
                        pathes.Add(reader.Current[0].ToString());
                    }

                    return pathes;
                });

                return result.FirstOrDefault();
            }
            finally
            {
                await session.CloseAsync();
            }
        }
        /// <summary>
        /// Matching a directory with a sub-directorie that contains an executable file
        /// Assumes that the executable files are with the .exe extension in all platforms
        /// </summary>
        /// <returns>Path of the directory</returns>
        public async Task<string> GetDirWithSubDirContainExeAsync()
        {
            var result = new List<string>();
            var session = CreateAsyncSession();

            try
            {
                var query = @"
                    MATCH (d:Directory)-[r:HAS_CHILD]->(q:Directory)-[r2:HAS_CHILD]->(f:File)
                    WHERE f.extension = "".exe""
                    RETURN d.path
                    LIMIT 1";

                result = await session.ReadTransactionAsync(async tx =>
                {
                    var pathes = new List<string>();
                    var reader = await tx.RunAsync(query);

                    while (await reader.FetchAsync())
                    {
                        pathes.Add(reader.Current[0].ToString());
                    }

                    return pathes;
                });

                return result.FirstOrDefault();
            }
            finally
            {
                await session.CloseAsync();
            }
        }
        /// <summary>
        /// Counting the sun-directories of the root directory
        /// Assumes that the executable files are with the .exe extension in all platforms
        /// </summary>
        /// <returns>Path of the directory</returns>
        public async Task<long> CountRootSunDirAsync()
        {
            var session = CreateAsyncSession();

            try
            {
                var query = @"
                    MATCH (root:Directory)-[:HAS_CHILD*]->(child:Directory)
                    WHERE NOT (:Directory)-->(root)
                    RETURN count(DISTINCT child)";

                var result = await session.ReadTransactionAsync(async tx =>
                {
                    var reader = await tx.RunAsync(query);
                    var count = await reader.SingleAsync();

                    return count[0];
                });

                return (long)result;
            }
            finally
            {
                await session.CloseAsync();
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }

        #region Private Methods
        private static string BuildCreateDirQuery(string prevPath)
        {
            var dirQuery = GetCreateDirQueryByPrevDir(prevPath);
            var filesQuery = GetCreateFilesForDirQuery();

            return $"{ dirQuery}{filesQuery}";
        }

        private static string GetCreateDirQueryByPrevDir(string prevPath)
        {
            return !string.IsNullOrEmpty(prevPath)
                ? @"
                    MATCH (p:Directory)
                    WHERE p.path = $prevPath
                    
                    MERGE (d:Directory {path: $directory.path})<-[r:HAS_CHILD]-(p)
                    ON CREATE
                        SET d = $directory"
                : @"
                    MERGE (d:Directory {path: $directory.path})
                    ON CREATE
                        SET d = $directory";
        }

        private static string GetCreateFilesForDirQuery()
        {
            return @"
                    FOREACH (file in $files | 
                        MERGE (f:File {path: file.path})<-[r:HAS_CHILD]-(d)
                        ON CREATE
                            SET f = file
                    )";
        }

        private static List<string> GetConstraintsQueries()
        {
            return new List<string>
            {
                "CREATE CONSTRAINT dir_path_unique IF NOT EXISTS FOR (d:Directory) REQUIRE d.path IS UNIQUE",
                "CREATE CONSTRAINT file_path_unique IF NOT EXISTS FOR (f:File) REQUIRE f.path IS UNIQUE",
            };
        }

        private static List<string> GetIndexesQueries()
        {
            return new List<string>
            {
                "CREATE INDEX file_extension_index IF NOT EXISTS FOR (f:File) ON (f.extension)",
            };
        }

        private IAsyncSession CreateAsyncSession()
        {
            return _driver.AsyncSession(o => o.WithDatabase("neo4j"));
        }
        #endregion
    }
}
