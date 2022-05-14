using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using WorkspaceGraphs.Repositories;

namespace WorkspaceGraphs
{
    public class Strater
	{
        #region Fields
        private readonly WorkspacesRepository _workspacesRepository;
        #endregion

        #region Properties
        /// <summary>
        /// Count the number of unsuccessful reading directories/files due to an error
        /// </summary>
        public int EntitiesNotReadableCount { get; set; }
        #endregion

        public Strater(WorkspacesRepository workspacesRepository)
		{
			_workspacesRepository = workspacesRepository;
		}

        /// <summary>
        /// Creating the graph according the given path
        /// </summary>
        /// <param name="path">Path of the root directory</param>
        /// <returns>The number of unsuccessful reading directories/files</returns>
		public async Task<int> RunAsync(string path)
        {
            if (string.IsNullOrEmpty(path)) return -1;

            try
            {
                // Reset each running
                EntitiesNotReadableCount = 0;

                // Initialize the database
                await _workspacesRepository.InitAsync();

                // Creating the graph
                TraversDirectory(path);

                return EntitiesNotReadableCount;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        /// <summary>
        /// Resets the database
        /// </summary>
        public async Task ResetDbAsync()
        {
            await _workspacesRepository.ResetDbAsync();
        }
        /// <summary>
        /// Runs the query to find the directory that has the most sub-directories
        /// </summary>
        /// <returns>Path</returns>
        public async Task<string> RunDirWithMostSubDirQueryAsync()
        {
            var result = await _workspacesRepository.GetDirWithMostSubDirAsync();

            return result;
        }
        /// <summary>
        /// Run the query to find a directory that has a sub-directory that has at least 1 executable file
        /// </summary>
        /// <returns>Path</returns>
        public async Task<string> RunDirWithSubDirContainExeQueryAsync()
        {
            var result = await _workspacesRepository.GetDirWithSubDirContainExeAsync();

            return result;
        }
        /// <summary>
        /// Run the query to calculate how many total subdirectories the root directory has
        /// </summary>
        /// <returns>Number of sub-directories</returns>
        public async Task<long> RunCountRootSunDirQueryAsync()
        {
            var result = await _workspacesRepository.CountRootSunDirAsync();

            return result;
        }
        /// <summary>
        /// Run the query to find directory tree that has a directory with exactly 3 empty subdirectories
        /// </summary>
        /// <returns>Directories</returns>
        public async Task RunDirTreeWithSubDirHoldsThreeEmptySubDirQueryAsync()
        {
            await _workspacesRepository.GetDirTreeWithSubDirHoldsThreeEmptySubDirAsync();
        }
        /// <summary>
        /// Run the query to find two files that match these conditions
        /// equal names
        /// the directory of one file is the subdirectory of the second file directory
        /// the file name is longer than 4 characters
        /// </summary>
        /// <returns>Files tree</returns>
        public async Task RunFilesWithSameNameInDifDirsQueryAsync()
        {
            await _workspacesRepository.GetFilesWithSameNameInDifDirsAsync();
        }

        #region Private Methods
        /// <summary>
        /// The recursion method for traversing and creating the graph
        /// </summary>
        /// <param name="path">Path of the current directory</param>
        /// <param name="prevPath">Path of the current directory's parent</param>
        private void TraversDirectory(string path, string prevPath = null)
        {
            try
            {
                // Gets the information that needed for creating the corrent directory
                var info = new DirectoryInfo(path);
                var directory = new Models.Directory(info);
                var files = info
                    .EnumerateFiles()
                    .Select(f => new Models.File(f))
                    .ToList();

                // Creating the directory
                _workspacesRepository
                    .CreateDirectoryAsync(directory, files, prevPath)
                    .Wait();

                // Create the sub-directories
                foreach (var dir in info.EnumerateDirectories())
                {
                    TraversDirectory(dir.FullName, directory.Path);
                }
            }
            catch (UnauthorizedAccessException)
            {
                EntitiesNotReadableCount++;
            }
            catch (Exception e)
            {
                // Check if it's a network problem
                if (e.InnerException is not null &&
                    e.InnerException.GetType().Equals(typeof(ServiceUnavailableException)))
                {
                    TraversDirectory(path, prevPath);
                }
                else
                {
                    EntitiesNotReadableCount++;
                }
            }
        }
        #endregion
    }
}
