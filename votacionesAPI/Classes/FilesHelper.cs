using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace votacionesAPI.Classes
{
    public class FilesHelper
    {
        public static bool UploadPhoto(MemoryStream stream, string folder, string name)
        {
            try
            {
                stream.Position = 0;
                var path = Path.Combine(HttpContext.Current.Server.MapPath(folder), name);
                if (File.Exists(path))
                    File.Delete(path);
                File.WriteAllBytes(path, stream.ToArray());
            }
            catch
            {
                return false;
            }

            return true;
        }
    }

}