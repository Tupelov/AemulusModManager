﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace AemulusModManager.Utilities.FileMerging
{
    public static class FlowMerger
    {
        public static void Merge(List<string> ModList, string game)
        {
            if (!Utils.CompilerExists()) return;

            List<string[]> compiledFiles = new List<string[]>();

            foreach (string dir in ModList)
            {
                var flowFiles = Directory.EnumerateFiles(dir, "*.*", SearchOption.AllDirectories)
                    .Where(s => (s.ToLower().EndsWith(".flow") || s.ToLower().EndsWith(".bf")) && !s.ToLower().EndsWith(".bf.flow"));

                foreach (string file in flowFiles)
                {

                    string bf = Path.ChangeExtension(file, "bf");
                    string filePath = Utils.GetRelativePath(bf, dir, game);
                    // If the current file is a bf check if it has a corresponding flow
                    if (file.Equals(bf, StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Ignore the current file if a flow exists (as it'll be compiled)
                        if (File.Exists(Path.ChangeExtension(file, "flow")))
                        {
                            continue;
                        }
                        // This is a standalone bf, add it so it can be used as a base
                        else
                        {
                            string[] bfFile = { filePath, dir, bf };
                            compiledFiles.Add(bfFile);
                            continue;
                        }
                    }

                    string[] previousFileArr = compiledFiles.FindLast(p => p[0] == filePath);
                    string previousFile = previousFileArr == null ? null : previousFileArr[2];
                    // Copy a previously compiled bf so it can be merged
                    if (previousFile != null)
                    {
                        File.Copy(previousFile, bf, true);
                    }
                    else
                    {
                        // Get the path of the file in original
                        string ogPath = $@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\Original\{game}\{Utils.GetRelativePath(bf, dir, game, false)}";
                        // Copy the original file to be used as a base
                        if (FileIOWrapper.Exists(ogPath))
                        {
                            File.Copy(ogPath, bf, true);
                        }
                        else
                        {
                            Console.WriteLine($@"[WARNING] Cannot find {ogPath}. Make sure you have unpacked the game's files if merging is needed");
                            continue;
                        }
                    }
                    if (!Utils.Compile(file, bf, game))
                        continue;
                    string[] compiledFile = { filePath, dir, bf };
                    compiledFiles.Add(compiledFile);
                }
            }
        }
    }
}
