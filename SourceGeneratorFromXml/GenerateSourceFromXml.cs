using System;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using System.IO;

namespace Emscripten.Build.Definition.CodeGen
{
    public class GenerateSourceFromXml : Task
    {
        [Required]
        public string Source { get; set; }

        public override bool Execute()
        {
            var reader = new XmlRuleReader(Source);
            var stream = new MemoryStream();

            var writer = new StreamWriter(stream);
            { 
                reader.Process(writer);
                writer.Flush();
            }
            
            stream.Seek(0, SeekOrigin.Begin);
            Log.LogMessagesFromStream(new StreamReader(stream), MessageImportance.High);

            return true;
        }
    }
}
