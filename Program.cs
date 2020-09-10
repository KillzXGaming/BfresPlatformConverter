using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Syroot.NintenTools.Bfres;
using Syroot.NintenTools.Bfres.WiiU;
using Syroot.NintenTools.Bfres.PlatformConverters;
using Syroot.NintenTools.Bfres.TextConvert;
using EveryFileExplorer;
using Syroot.BinaryData;

namespace BfresPlatformConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Supported Games :");
                Console.WriteLine("    Breath Of The Wild (Wii U => Switch)");
                Console.WriteLine("Arguments :");
                Console.WriteLine("     BfresPlatformConverter.exe file.bfres");
                return;
            }

            if (!Directory.Exists("SwitchConverted"))
                Directory.CreateDirectory("SwitchConverted");
            if (!Directory.Exists("WiiUConverted"))
                Directory.CreateDirectory("WiiUConverted");

            string externalShader = "Turbo_UBER.bfsha";

            foreach (var arg in args) {
                string name = Path.GetFileNameWithoutExtension(arg);
                string ext = Path.GetExtension(arg);

                if (arg.Contains(".Tex2"))
                    continue;

                bool compressed = IsCompressed(arg);

                ResFile resFile = LoadBFRES(arg, compressed);
                if (arg.Contains(".Tex1") && resFile.Textures.Values.Max(x => x.MipCount > 1))
                {
                    //Load tex2 mip maps
                    string tex2File = arg.Replace("Tex1", "Tex2");
                    if (!File.Exists(tex2File)) {
                        Console.WriteLine("Cannot find Tex2 file for mipmap data! Skipping...");
                        continue;
                    }

                    var resFileTex2 = LoadBFRES(tex2File, compressed);
                    foreach (var tex in resFileTex2.Textures.Values) {
                        ((Texture)resFile.Textures[tex.Name]).MipSwizzle = ((Texture)tex).Swizzle;
                        ((Texture)resFile.Textures[tex.Name]).MipData = ((Texture)tex).MipData;
                    }
                    name = name.Replace("Tex1", "Tex");
                    resFile.Name = name;
                }

                int alignment = 256;
                if (arg.Contains(".tex"))
                    alignment = 4096;

                //Change the platform.
                if (!resFile.IsPlatformSwitch)
                {
                    resFile.ChangePlatform(true, 4096, 0, 5, 0, 3, ConverterHandle.BOTW);
                    resFile.Alignment = 0x0C;

                    if (File.Exists(externalShader)) {
                        resFile.ExternalFiles.Add("Turbo_UBER.bfsha", new ExternalFile() { Data = File.ReadAllBytes(externalShader) });
                    }

                    if (compressed)
                        SaveCompressedResFile(resFile, $"SwitchConverted/{name}{ext}");
                    else
                        resFile.Save($"SwitchConverted/{name}{ext}");
                }
                else
                {
                    resFile.ChangePlatform(false, alignment, 4, 5, 0, 3, ConverterHandle.BOTW);

                    if (compressed)
                        SaveCompressedResFile(resFile, $"SwitchConverted/{name}{ext}");
                    else
                        resFile.Save($"WiiUConverted/{name}{ext}");
                }
            }
        }

        static bool IsCompressed(string path)
        {
            if (!File.Exists(path))
                return false;

            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryDataReader(fileStream)) {
                string sig = reader.ReadString(4, Encoding.ASCII);
                return sig == "Yaz0";
            }
        }

        static void SaveCompressedResFile(ResFile resFile, string path)
        {
            var mem = new MemoryStream();
            resFile.Save(mem);
            File.WriteAllBytes(path, YAZ0.Compress(mem.ToArray()));
        }

        static ResFile LoadBFRES(string filename, bool compressed)
        {
            if (compressed)
                return new ResFile(new MemoryStream(YAZ0.Decompress(filename)));
            else
                return new ResFile(filename);
        }
    }
}
