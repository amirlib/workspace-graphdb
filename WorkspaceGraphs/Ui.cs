using System;
using System.Text;
using System.Threading.Tasks;

namespace WorkspaceGraphs
{
	public class Ui
	{
        #region Fields
        private readonly Strater _starter;
        #endregion

        #region Properties
        /// <summary>
        /// Root path of the workspace
        /// </summary>
        public string RootPath { get; private set; }
        #endregion

        public Ui(Strater starter)
		{
			_starter = starter;
		}

		/// <summary>
		/// The entry of the UI
		/// </summary>
		public async Task RunAsync()
        {
			Welcome();
			Menu();

			await HandleUSerChoise();
        }

        #region Private Methods
        /// <summary>
        /// Welcoming section
        /// </summary>
        private static void Welcome()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Welcome to the Workspace Graph program");

            Console.Write(sb.ToString());
        }
        /// <summary>
        /// Menu section
        /// </summary>
        private static void Menu()
        {
            var sb = new StringBuilder();

            sb.AppendLine("1. Enter your root path");
            sb.AppendLine("2. Build graph");
            sb.AppendLine("3. Run a query to find the directory that has the most sub-directories");
            sb.AppendLine("4. Run a query to find a directory that has a sub-directory that has at least 1 executable file");
            sb.AppendLine("5. Run a query to calculate how many total subdirectories the root directory has");
            //sb.AppendLine("6. Run a A query to find a directory tree that has a directory with exactly 3 empty subdirectories");
            //sb.AppendLine("7. Run a query to find 2 files that match the requested conditions");
            sb.AppendLine("8. Reset database");

            Console.Write(sb.ToString());
        }
        /// <summary>
        /// Handles the user choise
        /// </summary>
        private async Task HandleUSerChoise()
        {
            while (true)
            {
                var result = Console.ReadLine() switch
                {
                    "1" => HandleEnterPathChoise(),
                    "2" => await HandleBuildGraphChoiseAsync(),
                    "3" => await HandleQueryDirWithMostSubDirChoiseAsync(),
                    "4" => await HandleQueryDirWithSubDirContainExeChoiseAsync(),
                    "5" => await HandleQueryCountRootSunDirChoiseAsync(),
                    //case "6":
                    //	Console.WriteLine($"Your result: {num1} / {num2} = " + (num1 / num2));
                    //	break;
                    //case "7":
                    //	Console.WriteLine($"Your result: {num1} / {num2} = " + (num1 / num2));
                    //	break;
                    "8" => await HandleResetDbChoiseAsync(),
                    _ => "Please choose from the presented choises",
                };

                if (!string.IsNullOrWhiteSpace(result))
                {
                    Console.WriteLine(result);
                }

                Menu();
            }
        }
        /// <summary>
        /// Handles the saving path
        /// </summary>
        /// <returns>String that represents the result of this action</returns>
        private string HandleEnterPathChoise()
        {
            RootPath = Console.ReadLine();

            return $"The path: {RootPath} saved successfully";
        }
        /// <summary>
        /// Handles the creating graph option
        /// </summary>
        /// <returns>String that represents the result of this action</returns>
        private async Task<string> HandleBuildGraphChoiseAsync()
        {
            var result = await _starter.RunAsync(RootPath);

            if (result == 0)
            {
                return "Your graph has been built successfully";
            }
            else if (result > 0)
            {
                return $"Your graph has been built successfully but with {result} files/folders that are not readable";
            }

            return "Encountered an error. Please check your path or the connection to the db and try again";
        }
        /// <summary>
        /// Handles the query of finding a directory that has the most sub-directories
        /// </summary>
        /// <returns>String that represents the result of this action</returns>
        private async Task<string> HandleQueryDirWithMostSubDirChoiseAsync()
        {
            var result = await _starter.RunDirWithMostSubDirQueryAsync();

            return !string.IsNullOrEmpty(result)
                ? $"The result is: {result}"
                : "There no such directory";
        }
        /// <summary>
        /// Handles the query of finding a directory that has a sub-directory that has at least 1 executable file
        /// </summary>
        /// <returns>String that represents the result of this action</returns>
        private async Task<string> HandleQueryDirWithSubDirContainExeChoiseAsync()
        {
            var result = await _starter.RunDirWithSubDirContainExeQueryAsync();

            return !string.IsNullOrEmpty(result)
                ? $"The result is: {result}"
                : "There no such directory";
        }
        /// <summary>
        /// Handles the query of calculating how many total subdirectories the root directory has
        /// </summary>
        /// <returns>String that represents the result of this action</returns>
        private async Task<string> HandleQueryCountRootSunDirChoiseAsync()
        {
            var result = await _starter.RunCountRootSunDirQueryAsync();

            return $"The result is: {result}";
        }
        /// <summary>
        /// Handles the reseting the database
        /// </summary>
        /// <returns>String that represents the result of this action</returns>
        private async Task<string> HandleResetDbChoiseAsync()
        {
            try
            {
                await _starter.ResetDbAsync();

                return $"Reset the database successfully";
            }
            catch (Exception)
            {
                return $"Encountered an error. Please check your connection to the db and try again";
            }
        }
        #endregion
    }
}

