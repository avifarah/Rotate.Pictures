using System;
using System.Collections.Generic;
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

        public static bool SaveDoNotDisplay(string repositoryFilePath, IEnumerable<string> noDisplayItems)
        {
            // TODO: Write a Verify function
            if (repositoryFilePath == null)
            {
                var msg = $"Argument \"{nameof(repositoryFilePath)}\" is passed erroneously";
                Log.Error(msg);
                MessageBox.Show(msg, "DoNotDisplay Save");
                throw new ArgumentException($@"{nameof(repositoryFilePath)} argument may not be null", nameof(repositoryFilePath));
            }

            if (noDisplayItems == null)
            {
                var msg = $"Argument \"{nameof(noDisplayItems)}\" is passed erroneously";
                Log.Error(msg);
                MessageBox.Show(msg, "DoNotDisplay Save");
                throw new ArgumentException($@"{nameof(noDisplayItems)} argument may not be null", nameof(noDisplayItems));
            }

            var fileName = repositoryFilePath == null
                ? ConfigValue.Inst.FilePathToSavePicturesToAvoid()
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
                MessageBox.Show(msg, "DoNotDisplay Save");
                return false;
            }
        }

        public static IEnumerable<string> RetrieveDoNotDisplay(string repositoryFilePath)
        {
            return null;
        }
    }
}
