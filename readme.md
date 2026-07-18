# Freefall - A Terra Nova: Strike Force Centauri Thing

This is not a port of Terra Nova. I'm focusing on my Ultima Underworld project. Maybe when I am done with Underworld this will be my next project, but for now I just want to give some love to Terra Nova.

For now all this project is aiming to do is 

- [x] Read Terra Nova Map Files.
- [ ] Read Terra Nova Textures and Art.
- [ ] Render a Terra Nova Level.


## Technical Roadblocks to Reverse Engineering TNOVA

Terra Nova is a Dos4GW application I have had limited progress with getting under the hood using my usual toolset of IDA 5.0 and DosBox debugger. Opening the .exe (__FF.exe) in IDA only gives me visibility of Dos4GW code. I have to extract the actual game code out of the exe.

So far all I have been able to do is

- Use the SunBind Extract Tool from DOS32A [https://www.javiergutierrezchamorro.com/wp-content/uploads/2013/07/dos32a/html/util/1.html]() to extract a Linear Executable (.LZ) file.
  - Import that LZ into IDA  but unfortunately I am unable to match the data addresses (strings etc) back to the code. So the code is hard to get under the hood. 
    - What seems to be happening is that a 32bit address eg (0x0014fa22) gets interpreted as 0xFA22 in IDA. This gets even weirder when I look at the hex views in IDA/hex editor and the 0x14 part of that address is 0. Whereas in the Ghidra hex view is showing the 0x14. I'm not sure what is going on here unless Ghidra is doing some segment adjustments.
  - Things are also more complicated because (as I understand it) code switches from the game exe back into dos4gw. Eg when opening a file this is handled by Dos4GW and not the game code.
- Alternatively I can import the .LZ into Ghidra using this plugin [https://github.com/yetmorecode/ghidra-lx-loader](). Ghidra appears to handle the file better and on the face of it data offsets are being managed by the file. I'm not really used Ghidra much before but on the face of it Ghidra seems to be the more promising route.

Basically I've yet to have my eureka moment with the code. Not helped that Underworld was 16 bit Borland C++ and TNova is 16/32 bit Watcom compiler (with Dos4GW) so the patterns that I'm used to seeing are all changed.


> PS. I'm not interested in AI solutions to the above problems. It sucks the fun out of the experience.

## See Also

 See [https://github.com/hankmorgan/TerraNovaStrikeForceCentauri] for collected documentation and tools (from other projects) I have archived over the years.

