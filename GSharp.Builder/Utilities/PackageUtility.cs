using GSharp.Compile;
using GSharp.Packager;
using System;
using System.IO;

namespace GSharp.Builder.Utilities
{
    public static class PackageUtility
    {
        public static void Create(string path, string title, string author, string summary, string output, GCompiler compiler)
        {
            var moduleName = Path.GetFileName(path);
            var tempResultPath = Path.Combine(Path.GetTempPath(), $"{moduleName}_{DateTime.Now.Millisecond}");

            Directory.CreateDirectory(tempResultPath);
            File.Copy(path, Path.Combine(tempResultPath, moduleName), true);

            foreach (var dll in compiler.References)
            {
                if (File.Exists(dll))
                {
                    File.Copy(dll, Path.Combine(tempResultPath, Path.GetFileName(dll)), true);
                }
            }

            var ini = new INI(Path.Combine(tempResultPath, $"{Path.GetFileNameWithoutExtension(moduleName)}.ini"));
            ini.SetValue("General", "Title", title);
            ini.SetValue("General", "Author", author);
            ini.SetValue("General", "Summary", summary);
            ini.SetValue("Assembly", "File", $@"<%LOCAL%>\{moduleName}");

            var builder = new PackageBuilder
            {
                Title = title,
                Author = author
            };

            builder.Add(tempResultPath);
            builder.Create(output);
        }
    }
}
