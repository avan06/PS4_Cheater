PS4_Cheater
====

PS4 Cheater is homebrew APP to find game cheat codes, and it is based on jkpatch which is from https://github.com/xemio/jkpatch.
Discord chat site: https://discord.gg/PwBwUKf.
Please join us!

---
JKPatch libRPC(librpc.dll) and Payload.bin with 6.72 support by GiantPluto  
Payload.bin needs to be loaded into the "PS4_Cheater\672\payload.bin" directory  
Link: https://twitter.com/DesignCodeRun/status/1293128260079910914  

---
Update PS4_Cheater version to 1.4.9
-------

* Enhance Search  
Add "group search" features, input value format:  
`[ValueType1:]ValueNumber1 [,] [ValueType2:]ValueNumber2 [,] [ValueType3:]ValueNumber3...`  
The default ValueType is `4` when the ValueType is omitted, which can be filled in `1, 2, 4, 8, F, D`, and ignore case.  
The delimiter can be "`,`" or "` `" (comma or space).  

* Enhance Processes Box  
Executing the Refresh Processes will automatically select the default program "`eboot.bin`", or the title starts with "`CUSA,PCAS,PCJS,PLAS,PLJS,PLJM,PCCS,PCKS`".  

* Enhance Section List  
Use ListView instead of ListBox to show all sections info.  
Add search section button, Now the section item can be search in sectionList which is convenient for selecting the target section.  
Add color for a specific section in sectionList to enhanced recognition.  
Add filter section list checkbox, will ignore section when the section name matches the filter rules.  
The preset filter rules is "`libSce, libc.prx, SceShell, SceLib, SceNp, SceVoice, SceFios, libkernel, SceVdec`", These rules can also be customized.  

* Enhance Hex Editor  
Add display current address information.  
Add "`Add current address to Cheat List`" in HexEditor.  

* Enhance Cheat List  
Add edit features in ContextMenu of cheat list.  
The edit interface is presented by the existing NewAddress Form.  

* Enhance Result List  
Add the find-pointer menu in the result_list_menu.  

* Enhance Pointer Finder  
Fix Cross-thread operation not valid issues, Call event to invoke delegate method when next pointer finder needs to update control.  
Add confirm message before executing the "`First Scan or Next Scan`" in PointerFinder.  
Add "`Add to Cheat List`" in the ContextMenu of pointer_list_view, Now can add the range pointer list to Cheat List.  
Add filter checkbox in PointerFinder, will ignore section when the section name matches the filter rules.  
