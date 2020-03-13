﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace avaino.Code.Parser
{
    public class Parser
    {
        private LibraryFinder libraryFinder;

        public Parser(LibraryFinder libraryFinder)
        {
            this.libraryFinder = libraryFinder;
        }

        public IEnumerable<ICodeEntity> FindDeclaredEntities(string content, bool library = false)
        {
            var entities = new List<ICodeEntity>();
            var reader = new StringReader(content);

            while (reader.Peek() >= 0)
            {
                var line = reader.ReadLine().Trim();
                if (line.StartsWith("#include") && !library)
                {
                    var libName = FindBetween(line, "<", ">");
                    if (libName == null) continue;
                    var libPath = libraryFinder.FindLibrary(libName);
                    if (libPath == null) continue;
                    var libData = File.ReadAllText(libPath);
                    entities.AddRange(FindDeclaredEntities(libData, true));
                }
                else
                {
                    var lineEntities = ParseLine(line);
                    if (lineEntities == null) continue;
                    entities.AddRange(lineEntities);
                }
            }

            return entities;
        }

        private IEnumerable<ICodeEntity> ParseLine(string line)
        {
            var entities = new List<ICodeEntity>();
            int idx;
            while ((idx = line.IndexOf("class")) != -1)
            {
                var data = FindBetween(line, "class", "{") ?? FindBetween(line, "class", ";");
                line = line.Substring(idx + "class".Length);
                if (data == null)
                    continue;
                entities.Add(new ClassDef(data.Trim()));
            }
            if (entities.Count == 0) return null;
            else return entities;
        }

        private string FindBetween(string data, string start, string end)
        {
            var startIdx = data.IndexOf(start);
            if (startIdx < 0) return null;
            var tmp = data.Substring(startIdx + start.Length);
            var endIdx = tmp.IndexOf(end);
            if (endIdx < 0) return null;
            return tmp.Remove(endIdx);
        }

    }
}