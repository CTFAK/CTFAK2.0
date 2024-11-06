> [!CAUTION]
> CTFAK 2.0 is no longer being worked on and thus has been archived.

# Welcome to CTFAK 2.0!
By Kostya and Yunivers

[Discord](https://www.discord.com/invite/wsH3KNtvvJ)
| Table of Contents | Description |
|--|--|
| [What was CTFAK 2.0?](https://github.com/CTFAK/CTFAK2.0#what-was-ctfak-20) | A short description of what CTFAK 2.0 was and what it was used for. |
| [Installation](https://github.com/CTFAK/CTFAK2.0#installation) | How to install a precompiled version of CTFAK 2.0. |
| [Compilation](https://github.com/CTFAK/CTFAK2.0#compilation) | How to compile CTFAK 2.0 manually. |
| [Usage](https://github.com/CTFAK/CTFAK2.0#usage) | How to use CTFAK 2.0. |
| [Parameters](https://github.com/CTFAK/CTFAK2.0#parameters) | All CTFAK 2.0 parameters. |
| [Command Arguments](https://github.com/CTFAK/CTFAK2.0#command-arguments) | All CTFAK 2.0 command arguments. |
| [Full Credits](https://github.com/CTFAK/CTFAK2.0#full-credits) | Everyone who helped make CTFAK 2.0 a reality. |

# What was CTFAK 2.0?
CTFAK 2.0 (Standing for **C**lick**T**eam **F**usion **A**rmy **K**nife **2.0**) was a tool developed by Kostya with help from Yunivers which can be used to either decompile or dump assets of games made with the Clickteam Fusion 2.5 game engine.

With CTFAK 2.0's plugin system, it was easy for anyone to make a plugin compatible with CTFAK 2.0 which allows you to do anything with the data read by CTFAK 2.0 including, but not limited to, converting the data to other game engines, programming your own dumping method to suit however you want to organize your data, or messing with the outputted data by modifying the `FTDecompile` plugin.

CTFAK 2.0 was split between 2 branches; the [master](https://github.com/CTFAK/CTFAK2.0/tree/master) branch and the [2.3](https://github.com/CTFAK/CTFAK2.0/tree/CTFAK-2.3) branch.<br/>
CTFAK 2.3 is more unstable than CTFAK 2.2, thus this README will only be referring to the 'master' branch, also known as CTFAK 2.2, which is the recommended version.

# Installation
> [!NOTE]  
> Due to unforseen circumstances, Actions are no longer being offered. See [Compilation](https://github.com/CTFAK/CTFAK2.0#compilation)

# Compilation
## Dependencies
CTFAK 2.0 required [.NET 6.0's Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

After running the x64 installer, you may proceed with the compilation.

## Cloning the repo with Visual Studio 2022

Make sure you have [Visual Studio 2022](https://visualstudio.microsoft.com/) installed and open.

On the GitHub branch, click `Code` and copy the HTTPS URL.

In Visual Studio 2022, under `Get started`, click `Clone a repository`, then paste the HTTPS URL from earlier. Input your desired path and press `Clone`.

## Compiling CTFAK 2.0

Right click the solution on the right and press `Build Solution` or do it through the key bind `Control + Shift + B`, then right click the solution once again and press `Open Folder in File Explorer`.

In the File Explorer go to `Interface\CTFAK.Cli\bin\Debug\net6.0-windows` and create a folder called `Plugins`, then go back to the solution folder and browse into `Plugins` and in both `CTFAK.Decompiler` and `Dumper`, browse to `bin\Debug\net6.0-windows`, and copy `CTFAK.Decompiler.dll`, `CTFAK.Decompiler.pdb`, `Dumper.dll`, and `Dumper.pdb` then paste them into the `Plugins` folder you created earlier.

Finally, download [ctfakrequirements.zip](https://github.com/CTFAK/.github/raw/main/ctfakrequirements.zip) and extract the contents into the same folder you created `Plugins` in. To know you've extracted it properly, `template.mfa` should be found in the same folder as `CTFAK.Cli.exe`.

Now, you should be able to run `CTFAK.Cli.exe` without problems!

# Usage
CTFAK 2.0 was very easy to use and required little input from the user.

To get started, open `CTFAK.Cli.exe` and drag in your Clickteam Fusion 2.5 exe, apk, ccn, dat, bin, or mfa file and press enter.

In parameters, you could input anything listed in [Parameters](https://github.com/CTFAK/CTFAK2.0#parameters), but make sure to put a `-` before each one. If you don't want to input any parameters (which you normally shouldn't need to do) then you can leave it blank. After you've filled out your parameters, press enter.

If you're using a `.ccn`, `.dat`, or `.bin` file it would bring up a prompt asking you to select a file reader. In any case, select CCN.

If all goes according to plan, you should see a screen saying `Reading finished in _ seconds` along with some information about the game. From here you may run any plugins you have installed. Normal installations should have `Export as MFA`, `Dump Everything`, `Image Dumper`, `Sound Dumper`, `Packed Data Dumper`, and `Sorted Image Dumper`.

> [!WARNING]  
> Please do not report any issues. CTFAK 2.0 is no longer being worked on. Please do not bother us about it.

Finally, you may close CTFAK 2.0 and find any outputs your plugins gave, in the `Dumps` folder.

# Parameters
All parameters should start with `-`.
| Parameter | Description |
|--|--|
| onlyimages | Prevents CTFAK 2.0 from reading any data unrelated to images. |
| noimg | Prevents CTFAK 2.0 from reading any images. |
| noevnt | Prevents CTFAK 2.0 from reading any events. |
| nosounds | Prevents CTFAK 2.0 from reading any sounds. |
| noalpha | Prevents CTFAK 2.0 from reading any alpha on images. |
| sorteddumpstrings | Has CTFAK 2.0 dump strings to text files alongside images in the Sorted Image Dumper. |
| srcexp | Forces CTFAK 2.0 to read a Source Explorer output. The unsorted output should be in a newly made `ImageBank` folder within your CTFAK 2.0 folder. |
| notrans | Prevents CTFAK 2.0 from applying Alpha, Color, or Shaders to objects. |
| noicons | Prevents CTFAK 2.0 from writing any object icons. |
| trace_chunks | Forces CTFAK 2.0 to write all chunks to `CHUNK_TRACE`. |
| dumpnewchunks | Forces CTFAK 2.0 to write chunks without a reader to `UnkChunks`. You must create this folder yourself. |
| f1.5 | Forces CTFAK 2.0 to read the input as MMF 1.5. |
| f3 | Forces CTFAK 2.0 to read the input as CTF 3.0. |
| android | Forces CTFAK 2.0 to read the input as android. |
| excludeframe([id]) | Forces CTFAK 2.0 to ignore the specified frame. ID indexes at 0. |
| log | Causes CTFAK 2.0 to log thread information about the Sorted Image Dumper. |
| badblend | Forces CTFAK 2.0 to revert to the old blend coeff fix. |
| chunk_info | Has CTFAK 2.0 log the size and offset of chunks. Also logs effects. |

# Command Arguments
These are command arguments for batch files or running CTFAK through cmd.
All arguments should start with `-` and should be followed up by data wrapped in quotations if data is required for said argument.
| Argument | Description |
|--|--|
| path | Automatically starts reading the inputted file path. |
| ____ | Leaving the arguments blank allows you to do the same thing as '-path'. |
| parameters | Allows you to input parameters that CTFAK 2.0 will read. |
| forcetype | Forces the kind of file type the file will be read as. Options: 'exe', 'apk', 'ccn', 'mfa' |
| tool | Uses the name of a plugin (such as 'Decompiler') to run said tool as soon as it's done reading. |
| closeonfinish | Closes CTFAK 2.0 after it finishes reading, or if you are using '-tool', after it finishes running the plugin. |

# Full Credits
|Name| Credit for... |
|--|--|
| [Mathias Kaerlev](https://github.com/matpow2) | Developer of Anaconda Mode 3. |
| [Kostya](https://github.com/1987kostya1) | Developer of CTFAK and CTFAK 2.0. |
| [Yunivers](https://github.com/AITYunivers) | Developer of CTFAK 2.0. |
| [Slxdy](https://github.com/Slxdy) | Assistant developer of CTFAK 2.0. |
| [RED_EYE](https://github.com/REDxEYE) | Developer of the decryption library. |
| [LAK132](https://github.com/LAK132) | Coding help for the Image Bank rewrite. |
| [Liz](https://github.com/lily-snow-9) | Coding help for Child Events, Sub-App port. |

CTFAK 2.0 is licensed under [AGPL-3.0](https://github.com/CTFAK/CTFAK2.0/blob/master/LICENSE).

Readme Last Updated November 6th, 2024.
