using System;
using System.Collections.Generic;
using System.IO;

namespace Structorian.Engine
{
    public class StructSourceContext
    {
        private string _baseDirectory;
        private Dictionary<string, string> _sourceMap = new Dictionary<string, string>();

        public string BaseDirectory
        {
            get { return _baseDirectory; }
            set { _baseDirectory = value; }
        }

        public void AddSourceText(string fileName, string source)
        {
            _sourceMap.Add(fileName, source);
        }
        
        public string GetSourceText(string fileName)
        {
            if (_sourceMap.ContainsKey(fileName))
                return _sourceMap[fileName];
            
            if (_baseDirectory != null)
            {
                string includePath = Path.Combine(_baseDirectory, fileName);
                if (File.Exists(includePath))
                {
                    StreamReader reader = new StreamReader(includePath);
                    string source;
                    try
                    {
                        source = reader.ReadToEnd();
                    }
                    finally
                    {
                        reader.Close();
                    }
                    _sourceMap.Add(fileName, source);
                    return source;
                }
            }

            throw new Exception("Could not find included file " + fileName);
        }
    }
}
