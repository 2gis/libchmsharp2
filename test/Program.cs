﻿/*
 * libchmsharp2 - a C# port of chmlib
 * Copyright (C) 2011 MindTouch, Inc.
 * www.mindtouch.com  oss@mindtouch.com
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */
using System;
using System.Collections.Generic;
using System.IO;

using CHMsharp;

namespace test
{
    class Program
    {
        static string _outdir;

        private static string _fileName;

        static void Main(string[] args)
        {
            if (args.Length != 1 && args.Length != 2)
            {
                Console.WriteLine("USAGE: test.exe <input file> [ <output directory> ]");
                return;
            }

            // check if we can find the source file
            if (!File.Exists(args[0]))
            {
                Console.WriteLine("ERROR: cannot find " + args[0]);
                return;
            }

            // check if an output directory was specified
            if (args.Length == 1)
            {
                _outdir = Path.Combine(Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]));
                if (!Directory.Exists(_outdir))
                {
                    Console.WriteLine("Creating output folder: " + _outdir);
                    Directory.CreateDirectory(_outdir);
                }
                else
                {
                    Console.WriteLine("Using output folder: " + _outdir);
                }
            }
            else
            {
                _outdir = args[1];
            }

            _fileName = args[0];
            var chmf = ChmFile.Open(_fileName);
            chmf.Enumerate(
                EnumerateLevel.Normal,
                EnumeratorCallback,
                new EnumeratorContext());
            chmf.Close();
        }

        static EnumerateStatus EnumeratorCallback(ChmFile file, ChmUnitInfo ui, EnumeratorContext context)
        {
            if (!ui.path.EndsWith("/"))
                Console.WriteLine(_fileName + ": " + ui.path);

            if (ui.length > 0)
            {
                var buf = new byte[ui.length];
                var ret = file.RetrieveObject(ui, ref buf, 0, buf.Length);

                if (ret > 0)
                {
                    try
                    {
                        var fi = new FileInfo(Path.Combine(_outdir, ui.path.Trim('/')));
                            
                        List<DirectoryInfo> created;

                        fi.Directory.CreateDirectory(out created);
                        File.WriteAllBytes(fi.FullName, buf);
                    }
                    catch (ArgumentException ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }

            return EnumerateStatus.Continue;
        }
    }
}
