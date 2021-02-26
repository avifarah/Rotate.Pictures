using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Rotate.Pictures.Utility
{
    public static class DoNotDisplayUtil
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        enum DoNotDisplayAction { AddPicture, SaveAll };

        /// <summary>
        /// TODO: when we are reading the pictures do not save the pictures to file or configuration
        /// TODO: when we are done reading then save the do-not-display to both file and configuration
        /// </summary>
        /// <param name="noDisplayItems"></param>
        /// <param name="repositoryFilePath"></param>
        /// <returns></returns>
        public static bool AddDoNotDisplay(IEnumerable<string> noDisplayItems, string repositoryFilePath = null)
	        => UpdateDoNotDisplay(noDisplayItems, repositoryFilePath, DoNotDisplayAction.AddPicture);

        public static bool SaveDoNotDisplay(IEnumerable<string> noDisplayItems, string repositoryFilePath = null)
	        => UpdateDoNotDisplay(noDisplayItems, repositoryFilePath, DoNotDisplayAction.SaveAll);

        public static IEnumerable<string> RetrieveDoNotDisplay(string repositoryFilePath = null)
        {
	        var items = new List<string>();
	        var fileName = repositoryFilePath == null
		        ? ConfigValue.Inst.FilePathToSavePicturesToAvoid()
		        : Environment.ExpandEnvironmentVariables(repositoryFilePath);

	        // If neither repositoryFilePath is passed in nor is it provided in the configuration file then return;
	        if (string.IsNullOrWhiteSpace(fileName)) return items;

	        var fullFn = Path.GetFullPath(fileName);
	        if (!File.Exists(fullFn)) return items;

	        try
	        {
		        using var sr = new StreamReader(fullFn);
		        while (!sr.EndOfStream)
		        {
			        var item = sr.ReadLine();
			        if (item != null && item.Trim() != string.Empty)
				        items.Add(item);
		        }
		        return items;
	        }
	        catch (Exception e)
	        {
		        const string msg = "Could not retrieve pictures to avoid";
		        Log.Error(msg, e);
		        MessageBox.Show(msg, @"DoNotDisplay Retrieve");
		        return items;
	        }
        }

        private static bool UpdateDoNotDisplay(IEnumerable<string> noDisplayItems, string repositoryFilePath, DoNotDisplayAction actionFlag)
        {
            noDisplayItems ??= new List<string>();

            var fileName = repositoryFilePath == null
                ? ConfigValue.Inst.FilePathToSavePicturesToAvoid()
                : Environment.ExpandEnvironmentVariables(repositoryFilePath);

            // If neither repositoryFilePath is passed in nor is it provided in the configuration file then return;
            if (string.IsNullOrWhiteSpace(fileName))
            {
                Log.Warn($"fileName is empty.  Unexpected. Stack trace:{Environment.NewLine}{DebugStackTrace.GetStackFrameString()}");
                return true;
            }

            try
            {
                var fullFn = Path.GetFullPath(fileName);
                if (!File.Exists(fullFn)) return true;

                bool actionValue = actionFlag == DoNotDisplayAction.AddPicture;
                using var sw = new StreamWriter(fullFn, actionValue);
                foreach (var item in noDisplayItems)
                    sw.WriteLine(item);
                return true;
            }
            catch (Exception e)
            {
                const string msg = "Could not save pictures to avoid";
                Log.Error(msg, e);
                MessageBox.Show(msg, @"DoNotDisplay Save");
                return false;
            }
        }
    }
}
