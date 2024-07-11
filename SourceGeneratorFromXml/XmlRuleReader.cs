using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Emscripten.Build.Definition.CodeGen
{
    internal class XmlRuleReader
    {
        private XmlReader xmlReader;

        public XmlRuleReader(string path)
        {
            this.xmlReader = new XmlTextReader(path);
        }

        public void Process(TextWriter emitter)
        {
            ProcessRoot(emitter);
        }

        private void ProcessRoot(TextWriter emitter)
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                if (xmlReader.Name != "Rule")
                {
                    continue;
                }

                ProcessElements(xmlReader.GetAttribute("SwitchPrefix"), emitter);
                break;
            }           
        }

        private void ProcessElements(string prefix, TextWriter emitter)
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (xmlReader.Name)
                {
                    case "StringListProperty":
                        EmitStringListProperty(prefix, emitter);
                        break;
                    case "BoolProperty":
                        EmitBoolProperty(prefix, emitter);
                        break;
                    case "IntProperty":
                        EmitIntProperty(prefix, emitter);
                        break;
                    case "EnumProperty":
                        EmitEnumProperty(prefix, emitter);
                        break;
                }
            }
        }

        private string ProcessSwitchValue(string switchValue, string prefix)
        {
            if (string.IsNullOrEmpty(switchValue))
            {
                return string.Empty;
            }
            else
            {
                return prefix + switchValue;
            }
        }

        private void EmitStringListProperty(string prefix, TextWriter emitter)
        {
            var formatText = @"    public virtual string[] {0}
    {{
        get
        {{
            if (IsPropertySet(""{0}""))
            {{
                return base.ActiveToolSwitches[""{0}""].StringList;
            }}

            return null;
        }}
        set
        {{
            base.ActiveToolSwitches.Remove(""{0}"");
            ToolSwitch toolSwitch = new ToolSwitch(ToolSwitchType.StringArray);
            toolSwitch.DisplayName = ""{1}"";
            toolSwitch.Description = ""{2}"";
            toolSwitch.ArgumentRelationList = new ArrayList();
            toolSwitch.SwitchValue = ""{3}"";
            toolSwitch.Name = ""{0}"";
            toolSwitch.StringList = value;
            base.ActiveToolSwitches.Add(""{0}"", toolSwitch);
            AddActiveSwitchToolValue(toolSwitch);
        }}
    }}";
            var content = string.Format(formatText,
                xmlReader.GetAttribute("Name"),
                xmlReader.GetAttribute("DisplayName"),
                xmlReader.GetAttribute("Description"),
                ProcessSwitchValue(xmlReader.GetAttribute("Switch"), prefix)
            );

            emitter.WriteLine(content);
        }

        private void EmitBoolProperty(string prefix, TextWriter emitter)
        {
            var formatText = @"    public virtual bool {0}
    {{
        get
        {{
            if (IsPropertySet(""{0}""))
            {{
                return base.ActiveToolSwitches[""{0}""].StringList;
            }}

            return false;
        }}
        set
        {{
            base.ActiveToolSwitches.Remove(""{0}"");
            ToolSwitch toolSwitch = new ToolSwitch(ToolSwitchType.Boolean);
            toolSwitch.DisplayName = ""{1}"";
            toolSwitch.Description = ""{2}"";
            toolSwitch.ArgumentRelationList = new ArrayList();
            toolSwitch.SwitchValue = ""{3}"";
            toolSwitch.ReverseSwitchValue = ""{4}"";
            toolSwitch.Name = ""{0}"";
            toolSwitch.BooleanValue = value;
            base.ActiveToolSwitches.Add(""{0}"", toolSwitch);
            AddActiveSwitchToolValue(toolSwitch);
        }}
    }}";
            var content = string.Format(formatText,
                xmlReader.GetAttribute("Name"),
                xmlReader.GetAttribute("DisplayName"),
                xmlReader.GetAttribute("Description"),
                ProcessSwitchValue(xmlReader.GetAttribute("Switch"), prefix),
                ProcessSwitchValue(xmlReader.GetAttribute("ReverseSwitch"), prefix)
            );

            emitter.WriteLine(content);
        }

        private void EmitIntProperty(string prefix, TextWriter emitter)
        {
            var formatText = @"    public virtual int {0}
    {{
        get
        {{
            if (IsPropertySet(""{0}""))
            {{
                return base.ActiveToolSwitches[""{0}""].StringList;
            }}

            return false;
        }}
        set
        {{
            base.ActiveToolSwitches.Remove(""{0}"");
            ToolSwitch toolSwitch = new ToolSwitch(ToolSwitchType.Integer);
            toolSwitch.DisplayName = ""{1}"";
            toolSwitch.Description = ""{2}"";
            toolSwitch.ArgumentRelationList = new ArrayList();
            if (ValidateInteger(""{0}"", int.MinValue, int.MaxValue, value))
            {{
                toolSwitch.IsValid = true;
            }}
            else
            {{
                toolSwitch.IsValid = false;
            }}
            toolSwitch.SwitchValue = ""{3}"";
            toolSwitch.Name = ""{0}"";
            toolSwitch.Number = value;
            base.ActiveToolSwitches.Add(""{0}"", toolSwitch);
            AddActiveSwitchToolValue(toolSwitch);
        }}
    }}";
            var content = string.Format(formatText,
                xmlReader.GetAttribute("Name"),
                xmlReader.GetAttribute("DisplayName"),
                xmlReader.GetAttribute("Description"),
                ProcessSwitchValue(xmlReader.GetAttribute("Switch"), prefix)
            );

            emitter.WriteLine(content);
        }

        private string[][] ReadEnumElements(string prefix)
        {
            var elements = new List<string[]>();

            while (xmlReader.Read())
            {
                switch (xmlReader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (xmlReader.Name == "EnumValue")
                        {
                            elements.Add(new string[2]
                            {
                                xmlReader.GetAttribute("Name"),
                                ProcessSwitchValue(xmlReader.GetAttribute("Switch"), prefix)
                            });
                        }
                        break;
                    case XmlNodeType.EndElement:
                        goto ExitWhile;
                }
            }   

        ExitWhile:
            return elements.ToArray();
        }

        private string SerializeEnumElements(string[][] elements)
        {
            var builder = new StringBuilder();

            builder.AppendLine($"new string[{ elements.Length }][] {{");

            var elementString = elements.Select(element =>
            {
                return $@"new string[2] {{ ""{element[0]}"", ""{element[1]}"" }}";
            });
            builder.AppendLine(string.Join(",", elementString.ToArray()));
            
            builder.AppendLine($"}}");

            return builder.ToString();
        }

        private void EmitEnumProperty(string prefix, TextWriter emitter)
        {
            var formatText = @"    public virtual string {0}
    {{
        get
        {{
            if (IsPropertySet(""{0}""))
            {{
                return base.ActiveToolSwitches[""{0}""].StringList;
            }}

            return false;
        }}
        set
        {{
            base.ActiveToolSwitches.Remove(""{0}"");
            ToolSwitch toolSwitch = new ToolSwitch(ToolSwitchType.String);
            toolSwitch.DisplayName = ""{1}"";
            toolSwitch.Description = ""{2}"";
            toolSwitch.ArgumentRelationList = new ArrayList();
            string[][] switchMap = {3};
            toolSwitch.SwitchValue = ReadSwitchMap(""{0}"", switchMap, value);
            toolSwitch.Name = ""{0}"";
            toolSwitch.Value = value;
            base.ActiveToolSwitches.Add(""{0}"", toolSwitch);
            AddActiveSwitchToolValue(toolSwitch);
        }}
    }}";
            var content = string.Format(formatText,
                xmlReader.GetAttribute("Name"),
                xmlReader.GetAttribute("DisplayName"),
                xmlReader.GetAttribute("Description"),
                SerializeEnumElements(ReadEnumElements(prefix))
            );

            emitter.WriteLine(content);
        }
    }
}
