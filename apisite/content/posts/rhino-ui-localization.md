---
title: "Rhino.UI.Localization"
date: 2020-03-10 09:24:56Z
draft: false
---

# Properties
## static int CurrentLanguageId
- (summary) 
- (since) 6.0
## static bool RunningAsEnglish
- (summary) 
- (since) 6.0
# Methods
## static string FormatArea(double area,UnitSystem units,DimensionStyle dimStyle,bool alternate)
- (summary) 
     Format an Area string from a number
     
- (since) 7.0
- (returns) string This is some return comment
## static string FormatDistanceAndTolerance(double distance,UnitSystem units,DimensionStyle dimStyle,bool alternate)
- (summary) 
- (since) 7.0
- (returns) string This is some return comment
## static string FormatNumber(double x,UnitSystem units,DistanceDisplayMode mode,int precision,bool appendUnitSystemName)
- (summary) 
     Get a string version of a number in a given unit system / display mode.
     
- (since) 5.0
- (returns) string This is some return comment
## static string FormatVolume(double volume,UnitSystem units,DimensionStyle dimStyle,bool alternate)
- (summary) 
     Format a Volume string from a number
     
- (since) 7.0
- (returns) string This is some return comment
## static string LocalizeCommandName(string english)
- (summary) 
     Commands that need to be localized should call this function.
    
- (since) 5.0
- (returns) string This is some return comment
## static string LocalizeCommandName(string english,object assemblyOrObject)
- (summary) 
- (since) 5.0
- (returns) string This is some return comment
## static LocalizeStringPair LocalizeCommandOptionName(string english,int contextId)
- (summary) 
- (since) 5.0
- (returns) LocalizeStringPair This is some return comment
## static LocalizeStringPair LocalizeCommandOptionName(string english,object assemblyOrObject,int contextId)
- (summary) 
- (since) 5.0
- (returns) LocalizeStringPair This is some return comment
## static LocalizeStringPair LocalizeCommandOptionValue(string english,int contextId)
- (summary) 
- (since) 5.0
- (returns) LocalizeStringPair This is some return comment
## static LocalizeStringPair LocalizeCommandOptionValue(string english,object assemblyOrObject,int contextId)
- (summary) 
- (since) 5.0
- (returns) LocalizeStringPair This is some return comment
## static string LocalizeDialogItem(object assemblyOrObject,string key,string english)
- (summary) 
     Look in the dialog item list for the specified key and return the translated
     localized string if the key is found otherwise return the English string.
     
- (since) 5.5
- (returns) string This is some return comment
## static void LocalizeForm(object formOrUserControl)
- (summary) 
     Look in the dialog item list for the specified key and return the translated
     localized string if the key is found otherwise return the English string.
     
- (since) 6.0
- (returns) void This is some return comment
## static string LocalizeString(string english,int contextId)
- (summary) 
     Returns localized version of a given English string. This function should be autogenerated by the
     RmaLDotNetLocalizationProcessor application for every function that uses RMASTR.
     
- (since) 5.0
- (returns) string This is some return comment
## static string LocalizeString(string english,object assemblyOrObject,int contextId)
- (summary) 
     Returns localized version of a given English string. This function should be autogenerated by the
     RmaLDotNetLocalizationProcessor application for every function that uses RMASTR.
     
- (since) 5.0
- (returns) string This is some return comment
## static bool SetLanguageId(int id)
- (summary) 
     Sets the Id used for Localization in RhinoCommon.  Only useful for when
     using RhinoCommon outside of the Rhino process
     
- (since) 5.0
- (returns) bool This is some return comment
## static string UnitSystemName(UnitSystem units,bool capitalize,bool singular,bool abbreviate)
- (summary) 
     Gets localized unit system name.  Uses current application locale id.
     
- (since) 5.0
- (returns) string This is some return comment