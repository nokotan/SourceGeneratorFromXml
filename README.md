# SourceGeneratorFromXml

Create source code from MSBuild property pages definition xml files.

Input:

```xml
<StringListProperty Subtype="folder" Name="AdditionalIncludeDirectories" DisplayName="Additional Include Directories" Description="Specifies one or more directories to add to the include path; separate with semi-colons if more than one. (-I[path])." Category="General" Switch="I" />
```

Output:

```cs
      public virtual string[] AdditionalIncludeDirectories
      {
          get
          {
              if (IsPropertySet("AdditionalIncludeDirectories"))
              {
                  return base.ActiveToolSwitches["AdditionalIncludeDirectories"].StringList;
              }
  
              return null;
          }
          set
          {
              base.ActiveToolSwitches.Remove("AdditionalIncludeDirectories");
              ToolSwitch toolSwitch = new ToolSwitch(ToolSwitchType.StringArray);
              toolSwitch.DisplayName = "Additional Include Directories";
              toolSwitch.Description = "Specifies one or more directories to add to the include path; separate with semi-colons if more than one. (-I[path]).";
              toolSwitch.ArgumentRelationList = new ArrayList();
              toolSwitch.SwitchValue = "-I";
              toolSwitch.Name = "AdditionalIncludeDirectories";
              toolSwitch.StringList = value;
              base.ActiveToolSwitches.Add("AdditionalIncludeDirectories", toolSwitch);
              AddActiveSwitchToolValue(toolSwitch);
          }
      }
```
