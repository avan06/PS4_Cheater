PS4_Cheater
====

PS4 Cheater is homebrew APP to find game cheat codes, and it is based on [ps4debug](https://github.com/jogolden/ps4debug).

This is a work in progress.

|  FW  | Status          | Comment
|------|-----------------|--------------------
| 5.05 | Minimal testing | 5.05 `ps4debug.bin`
| 6.72 | Minimal testing | 6.72 `ps4debug.bin`
| 7.02 | Untested        | 7.02 `ps4debug.bin`

## Building

Open `PS4_Cheater.sln` with Visual Studio and build.

**Note:**
- `payloads` directory will be copied to the `debug`/`release` directory as a post-build step.
- If using `ps4debug` `payloads` directory on its own, be sure to grab `libdebug.dll`.
  - Need both for the speedfix.
- Included pre-compiled `ps4debug` binaries are built from:
  - 5.05 - https://github.com/jogolden/ps4debug.git @ b446dced06009705c6f8d70e79113637d1690210
  - 6.72 - https://github.com/GiantPluto/ps4debug.git @ 0cc311cd1ff62e1020657b7f5715b57ba18a28a3
  - 7.02 - https://github.com/ChendoChap/ps4debug.git @ 7c114ba1b8fe2d5bbdad0079dec442deff10c4e0

## Acknowledgements & Thanks!
- Countless contributors to `jkpatch`, `ps4-ksdk`, `ps4-payload-sdk`, `ps4debug` and `PS4_Cheater`.
- [DeathRGH](https://github.com/DeathRGH) for the speedfix tweak for ps4debug.
- [Al-Azif](https://github.com/Al-Azif) for his [ps4-exploit-host](https://github.com/Al-Azif/ps4-exploit-host) - Very useful for local testing.

---
Discord chat site: https://discord.gg/PwBwUKf.
Please join us!

---
Version 1.4.9 enhancement description
-------

* Search  
Add "`group search`" features, Ability to use group search when you know the data structure.  
Input value format:  
`[ValueType1:]ValueNumber1 [,] [ValueType2:]ValueNumber2 [,] [ValueType3:]ValueNumber3...`  
The default ValueType is `4` when the ValueType is omitted, which can be filled in `1, 2, 4, 8, F, D, H`, and ignore case.  
(1: 1 byte, 2: 2 byte, 4: 4 byte, F: Float, D: Double, H: Hex)  
The delimiter can be "`,`" or "` `" (comma or space).  
Example:  
Assuming the data structure: HP:950(4 byte) / MaxHP:1000(4 byte) / MP:50(2 byte) / MaxMP:100(2 byte),  
Input value: 950 1000 2:50 2:100  

* Processes Box  
Executing the Refresh Processes will automatically select the default program "`eboot.bin`".  

* Section List  
Use ListView instead of ListBox to show all sections info.  
Add search section button, Now the section item can be search in sectionList which is convenient for selecting the target section.  
Add color for a specific section in sectionList to enhanced recognition.  
Add filter section list checkbox, will ignore section when the section name matches the filter rules.  
The preset filter rules is "`libSce, libc.prx, SceShell, SceLib, SceNp, SceVoice, SceFios, libkernel, SceVdec`", These rules can also be customized.  

* Hex Editor  
Add display current address information.  
Add "`Add current address to Cheat List`" in HexEditor.  

* Cheat List  
Add edit features in ContextMenu of cheat list.  
The edit interface is presented by the existing NewAddress Form.  

* Result List  
Add the find-pointer menu in the result_list_menu.  

* Pointer Finder  
Fix Cross-thread operation not valid issues, Call event to invoke delegate method when next pointer finder needs to update control.  
Add confirm message before executing the "`First Scan or Next Scan`" in PointerFinder.  
Add "`Add to Cheat List`" in the ContextMenu of pointer_list_view, Now can select multiple pointers list add to Cheat List.  
Add "`Filter`" checkbox in PointerFinder, will ignore section when the section name matches the filter rules.  
Add "`FastNextScan`" checkbox in PointerFinder, Ability to choose whether to skip sections that may be irrelevant when performing the next scan.

---
PointerFinder checkbox description
-------

* FastScan  
Limit the base address of the pointer to be executable section.
* FastNextScan  
Ability to choose whether to skip sections that may be irrelevant when performing the next scan.
* Filter  
Skip the system lib or custom sections when performing scan pointer.
