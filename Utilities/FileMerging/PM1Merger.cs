﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace AemulusModManager.Utilities.FileMerging
{
    class PM1Merger
    {
        private static string compilerPath = $@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Dependencies\PM1MessageScriptEditor\PM1MessageScriptEditor.exe";

        public static void Merge(List<string> ModList, string game)
        {
            if (!Utils.CompilerExists(true)) return;
            List<string[]> foundFiles = new List<string[]>();

            foreach (string dir in ModList)
            {
                string[] bmdFiles = Directory.GetFiles(dir, "*.pm1", SearchOption.AllDirectories);
                foreach (string file in bmdFiles)
                {
                    string filePath = Utils.GetRelativePath(file, dir, game);
                    string[] previousFileArr = foundFiles.FindLast(p => p[0] == filePath);
                    string previousFile = previousFileArr == null ? null : previousFileArr[2];
                    // Merge pm1s if there are two
                    if (previousFile != null)
                    {
                        // Get the path of the file in original
                        string ogPath = $@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Original\{game}\{Utils.GetRelativePath(file, dir, game, false)}";
                        MergePm1s(new string[] { previousFile, file }, ogPath, game);
                    }
                    string[] foundPm1 = { filePath, dir, file };
                    foundFiles.Add(foundPm1);
                }
            }
        }

        // Merge two bmds the second one being the higher priority
        private static void MergePm1s(string[] bmds, string ogPath, string game)
        {
            // Check that the original bmd exists
            if (!File.Exists(ogPath))
            {
                Console.WriteLine($@"[WARNING] Cannot find {ogPath}. Make sure you have unpacked the game's files if merging is needed");
                return;
            }

            // Get the contents of the bmds
            Dictionary<string, string>[] messages = new Dictionary<string, string>[2];
            Dictionary<string, string> ogMessages = new Dictionary<string, string>();
            messages[0] = GetPm1Messages(bmds[0], game);
            messages[1] = GetPm1Messages(bmds[1], game);
            ogMessages = GetPm1Messages(ogPath, game);

            // Merge the bmds
            Utils.MergeFiles(game, bmds, messages, ogMessages);
        }

        private static Dictionary<string, string> GetPm1Messages(string file, string game)
        {
            try
            {
                // Decompile the pm1 to a msg that can be read easily
                string msgFile = Path.ChangeExtension(file, "msg");
                Utils.RunCommand(compilerPath, $"\"{file}\"");

                return Utils.GetMessages(msgFile);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[ERROR] Error reading {file}. Cancelling pm1 merging");
            }
            return null;
        }
    }
}
