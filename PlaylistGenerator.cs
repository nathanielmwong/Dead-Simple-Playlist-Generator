using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    class Program
    {
        static string[] wplHeader =
        {
            "<?wpl version=\"1.0\"?>",
            "<smil>",
            "<head>",
            "<author />",
            "</head>",
            "<body>",
            "<seq>"
        };

        static string[] wplCloser =
        {
            "</seq>",
            "</body>",
            "</smil>"
        };

        static string[] m3uHeader =
        {
            @"#EXTM3U"
        };

        static string[] m3uCloser =
        {

        };

        static void Main(string[] args)
        {
            Console.WriteLine("Playlist generation defaults to UserDir\\Music unless otherwise specified");
            //Console.WriteLine("Press return to continue or any other input to end");

            string rootDir = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            string outputDir = String.Format(@"{0}\Playlists",rootDir);
            if (!Directory.Exists(outputDir)) { Directory.CreateDirectory(outputDir); }
            string playlistFiles = @"*.mp3";
            GeneratePlaylists(rootDir, playlistFiles, outputDir, ".m3u");
            //playlistformat = @"*.flac";
            //GeneratePlaylists(rootDir, playlistformat, outputDir);
        }

        static void GeneratePlaylists(string rootDir, string fileType, string outputPath = null, string listFormat = ".wpl")
        {
            string[] directoryList = Directory.GetDirectories(rootDir, "*", SearchOption.AllDirectories);

            if (directoryList.Count() < 1)
            {
                GeneratePlaylist(rootDir, fileType, rootDir.Split(Path.DirectorySeparatorChar).Last(), outputPath, listFormat);
                return;
            }

            foreach (string nextDirectory in directoryList)
            {
                GeneratePlaylist(nextDirectory, fileType, nextDirectory.Split(Path.DirectorySeparatorChar).Last(), outputPath, listFormat);
            }
        }

        static void GeneratePlaylist(string rootDir, string fileType, string listName = null, string outputPath = null, string listFormat = ".wpl")
        {
            List<string> fileList = Directory.GetFiles(rootDir, fileType, SearchOption.TopDirectoryOnly).ToList();
            if (fileList.Count() < 1) { return; }

            if (fileList.First().ToLower().IndexOf(@"(disc") != -1) 
            {
                List<string> disc1List = fileList.Where(filePath => filePath.ToLower().IndexOf(@"(disc") < 1).ToList();
                fileList.RemoveRange(fileList.Count() - disc1List.Count(), disc1List.Count());
                fileList = disc1List.Concat(fileList).ToList();
            }

            string commonRoot = rootDir.Substring(0, rootDir.Zip(outputPath, (c1, c2) => c1 == c2).TakeWhile(b => b).Count());

            Console.WriteLine(string.Format("Generating playlist for [{0}]", listName));

            string[] fileContent = null;
            string iconPath = Directory.GetFiles(rootDir, "*album*.jpg", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (iconPath is null) { iconPath = Directory.GetFiles(rootDir, "*album*.png", SearchOption.TopDirectoryOnly).FirstOrDefault(); }
            List<string> fileBodyList = new List<string>();

            if (listFormat.Equals(".wpl"))
            {
                foreach (string nextFile in fileList)
                { fileBodyList.Add(string.Format("<media src=\"{0}\" />", nextFile.Replace(commonRoot, @"..\"))); }
                fileBodyList.AddRange(wplCloser);
                fileContent = wplHeader.Concat(fileBodyList).ToArray();
            }

            if (listFormat.Equals(".m3u"))
            {
                if (!String.IsNullOrEmpty(iconPath)) { fileBodyList.Add(string.Format("#EXTIMG:\"{0}\"", iconPath.Replace(commonRoot, @"..\"))); }
                fileBodyList.Add(String.Format("#PLAYLIST:{0}", listName));
                //if (!String.IsNullOrEmpty(iconPath)) { fileBodyList.Add(string.Format(@"#EXTIMG:{0}", iconPath)); }
                foreach (string nextFile in fileList)
                { fileBodyList.Add(string.Format("{0}", nextFile.Replace(commonRoot, @"..\"))); }
                fileBodyList.AddRange(m3uCloser);
                fileContent = m3uHeader.Concat(fileBodyList).ToArray();
            }

            string playlistDir = String.IsNullOrEmpty(outputPath) ? rootDir : outputPath;
            File.WriteAllLines(string.Format("{0}\\{1}{2}", playlistDir, listName, listFormat), fileContent);
        }
    }
}
