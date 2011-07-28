using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TurnOut.VersionUpdate.Tools;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;

namespace TurnOut.VersionUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Environment.CommandLine);
            Arguments CommandLine = new Arguments(args);
            string sProjectPath = string.Empty;
            string sWxsFile = string.Empty;
            Version version = new Version();
            Increment incVersion = Increment.Build;

            Console.WriteLine("TurnOut.NET version update");
            Console.WriteLine("Tool that updates version numbers in AssemblyInfo and WiX-files.");
            Console.WriteLine("Author: Sven Rütz<sven@daemonized.de>");
            Console.WriteLine();

            HandleArguments(CommandLine, ref sProjectPath, ref sWxsFile, ref version, ref incVersion);
            version = WriteVersion(sProjectPath, version, incVersion);
            WriteWixFile(sWxsFile, version);
        }

        private static void WriteWixFile(string sWxsFile, Version version)
        {
            if (sWxsFile.Length > 0)
            {
                string sText = string.Empty;
                Regex reg = new Regex(@"(\<\?define ProductVersion = )""(.*)""(\?\>)");

                using (TextReader reader = new StreamReader(sWxsFile))
                {
                    sText = reader.ReadToEnd();
                    reader.Close();
                }

                string sTemp = reg.Replace(sText, string.Format(CultureInfo.InvariantCulture, "$1\"{0}\"$3", version.ToString()));

                //make a backup
                string sBackup = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", Path.GetTempPath(), Path.GetFileName(sWxsFile));
                File.Copy(sWxsFile, sBackup, true);
                Console.WriteLine("Made a backup @\"{0}\"", sBackup);

                using (TextWriter writer = new StreamWriter(sWxsFile, false, Encoding.UTF8))
                {
                    writer.Write(sTemp);
                    writer.Close();
                }
            }
        }

        private static Version WriteVersion(string sProjectPath, Version version, Increment incVersion)
        {
            string sAssemblyInfo = string.Format(CultureInfo.InvariantCulture, "{0}\\Properties\\AssemblyInfo.cs", sProjectPath);
            if (File.Exists(sAssemblyInfo))
            {
                Regex reg = new Regex(@"(\[assembly: Assembly.*Version\()""(.*)""(\)])");
                string sText = string.Empty;
                using (TextReader reader = new StreamReader(sAssemblyInfo))
                {
                    sText = reader.ReadToEnd();
                    reader.Close();
                }

                if (null == version || version.Build == -1)
                {
                    try
                    {
                        version = Version.Parse(reg.Match(sText).Groups[2].Value);
                    }
                    catch (FormatException)
                    {
                        Console.Error.WriteLine("Error when I tried to parse the version.");
                        Environment.Exit(-1);
                    }

                    int iMajor = version.Major;
                    int iMinor = version.Minor;
                    int iRelease = version.Build;
                    int iBuild = version.Revision;

                    switch (incVersion)
                    {
                        case Increment.Major:
                            iMajor++;
                            goto default;
                        case Increment.Minor:
                            iMinor++;
                            goto default;
                        case Increment.Release:
                            iRelease++;
                            goto default;
                        case Increment.Build:
                            iBuild++;
                            goto default;
                        default:
                            version = new Version(iMajor, iMinor, iRelease, iBuild);
                            break;
                    }
                }

                string sTemp = reg.Replace(sText, string.Format(CultureInfo.InvariantCulture, "$1\"{0}\"$3", version.ToString()));

                //make a backup
                string sBackup = string.Format(CultureInfo.InvariantCulture, "{0}\\{1}", Path.GetTempPath(), Path.GetFileName(sAssemblyInfo));
                File.Copy(sAssemblyInfo, sBackup, true);
                Console.WriteLine("Made a backup @\"{0}\"", sBackup);

                using (TextWriter writer = new StreamWriter(sAssemblyInfo, false, Encoding.UTF8))
                {
                    writer.Write(sTemp);
                    writer.Close();
                }
            }
            else
            {
                Console.Error.WriteLine("AssemblyInfo.cs not found!");
                Environment.Exit(-1);
            }
            return version;
        }

        private static void HandleArguments(Arguments CommandLine, ref string sProjectPath, ref string sWxsFile, ref Version vGivenVersion, ref Increment incVersion)
        {
            if (CommandLine["h"] != null || CommandLine["help"] != null || CommandLine["?"] != null)
            {
                Console.WriteLine("Usage: ");
                Console.WriteLine("\t --project-path=<path>\t Specifiy the path to the project you want to process");
                Console.WriteLine("\t --wix-file=<file.wxs>\t (Optional)Specifiy the path with the wxs file you want to update");
                Console.WriteLine("\t --inc=[major|minor|release|build]\t Specifiy which part of the version string you want to increment");
                Console.WriteLine("\t --version=<0.0.0.0>\t specifiy a version to set (--inc is ignored if set)");
                Console.WriteLine("\t --help \t Display help");
                Environment.Exit(0);
            }

            if (CommandLine["project-path"] != null)
            {
                sProjectPath = CommandLine["project-path"];
                if (!Directory.Exists(sProjectPath))
                {
                    Console.Error.WriteLine("The given project path \"{0}\" does not exist!", sProjectPath);
                    Environment.Exit(-1);
                }
            }

            if (CommandLine["wix-file"] != null)
            {
                sWxsFile = CommandLine["wix-file"];
                if (!File.Exists(sWxsFile))
                {
                    Console.Error.WriteLine("The given installation file \"{0}\" does not exist!", sWxsFile);
                    Environment.Exit(-1);
                }
            }

            if (CommandLine["inc"] != null)
            {
                try { incVersion = GetInc(CommandLine["inc"]); }
                catch (ArgumentException)
                {
                    Console.Error.WriteLine("The given argument is invalid! Use \"major\", \"minor\", \"release\" or \"build\" as increment values.");
                    Environment.Exit(-1);
                }
            }
            else
                incVersion = Increment.Build;

            if (CommandLine["version"] != null)
                try { vGivenVersion = Version.Parse(CommandLine["version"]); }
                catch(FormatException)
                {
                    vGivenVersion = null;
                    Console.Error.WriteLine("There was an error parsing the given version. Use the foloowing format: \"1.0.0.1\".");
                    Environment.Exit(-1);
                }
        }
        
        private static Increment GetInc(string sArgument)
        {
            switch (sArgument.ToUpperInvariant())
            {
                case "MAJOR":
                    return Increment.Major;
                case "MINOR":
                    return Increment.Minor;
                case "RELEASE":
                    return Increment.Release;
                case "BUILT":
                    return Increment.Build;
                default:
                    throw new ArgumentException(
                        string.Format(CultureInfo.InvariantCulture,
                        "{0} is not a valid increment type. Try \"major\", \"minor\", \"release\" or \"built\".", sArgument)
                        );
            }
        }

        private enum Increment
        {
            Major,
            Minor,
            Release,
            Build
        }
    }
}
