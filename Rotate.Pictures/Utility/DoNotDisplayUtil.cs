using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rotate.Pictures.Utility
{
    public static class DoNotDisplayUtil
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool SaveDoNotDisplay(IEnumerable<string> noDisplayItems, string repositoryFilePath = null)
        {
            noDisplayItems ??= new List<string>();

            var fileName = repositoryFilePath == null
                ? Environment.ExpandEnvironmentVariables(ConfigValue.Inst.FilePathToSavePicturesToAvoid())
                : Environment.ExpandEnvironmentVariables(repositoryFilePath);
            try
            {
                var fullFn = Path.GetFullPath(fileName);
                using var sw = new StreamWriter(fullFn, false);
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

        public static IEnumerable<string> RetrieveDoNotDisplay(string repositoryFilePath)
        {
            var items = new List<string>();
            var fileName = repositoryFilePath == null
                ? Environment.ExpandEnvironmentVariables(ConfigValue.Inst.FilePathToSavePicturesToAvoid())
                : Environment.ExpandEnvironmentVariables(repositoryFilePath);
            try
            {
                var fullFn = Path.GetFullPath(fileName);
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
    }
}
