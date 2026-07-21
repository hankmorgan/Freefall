using Godot;

using FreeFall;
using System.IO;

public partial class main : Node
{
	public static main instance;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		instance = this;
		var files = System.IO.Directory.EnumerateFiles("c:\\games\\tnova\\TNF108\\MAPS\\", "*.res");
		foreach (var file in files)
		{
			var output = Path.Combine(path1: "c:\\temp\\tnova", path2: $"{System.IO.Path.GetFileNameWithoutExtension(file)}.png");
			//TNovaMapLoader.BuildTNovaMap(file, output);	
		}

		// var palettedata = File.ReadAllBytes("c:\\games\\tnova\\PLNT1.PAL");
		// var referencepal = new FreeFall.Palette();
		// var addr_ref = 0;
		// for (int i = 0; i < 256; i++)
		// {
		// 	referencepal.red[i] = palettedata[addr_ref + 0];
		// 	referencepal.blue[i] = palettedata[addr_ref + 1];
		// 	referencepal.green[i] = palettedata[addr_ref + 2];
		// 	referencepal.alpha[i] = 255;
		// 	addr_ref += 3;
		// }
		// byte[] subset = new byte[100];
		// Buffer.BlockCopy(palettedata, 400, subset, 0, 100);
		// var imgref = referencepal.toImage(1);
		// imgref.GetImage().SavePng($"C:\\Temp\\tnova\\palettes\\referencepal.png");

		var enumoptions = new EnumerationOptions();
		enumoptions.RecurseSubdirectories = true;
		files = System.IO.Directory.EnumerateFiles(path: "c:\\games\\tnova\\", searchPattern: "*.res", enumerationOptions: enumoptions);
		foreach (var file in files)
		{
			var chunks = Resloader.EnumerateResFile(file);
			// byte[] filedata;//
			// Resloader.ReadStreamFile(file, out filedata);
			// foreach (var chunk in chunks)
			// {
			// 	Resloader.Chunk chunkdata;
			// 	Resloader.LoadChunk(filedata, chunk, out chunkdata);
			// 	var offset = Resloader.matchdata( subset, chunkdata.data, 100);
			// 	if (offset >= 0)
			// 	{
			// 		Debug.Print($"Match data in file {file} chunk {chunk}");
			// 		var pal = new FreeFall.Palette();
			// 		var addr = offset;
			// 		for (int i = 0; i <= chunkdata.chunkUnpackedLength; i++)
			// 		{
			// 			if ((i < 256) && (addr + 2<=chunkdata.data.GetUpperBound(0)))
			// 			{
			// 				pal.red[i] = chunkdata.data[addr + 0];
			// 				pal.blue[i] = chunkdata.data[addr + 1];
			// 				pal.green[i] = chunkdata.data[addr + 2];
			// 				pal.alpha[i] = 255;
			// 			}
			// 			addr += 3;
			// 		}
			// 		var img = pal.toImage(1);
			// 		img.GetImage().SavePng($"C:\\Temp\\tnova\\palettes\\palette_at_chunk_{chunk}.png");
			// 		// TNovaMapLoader.DumpPlanet(
			// 		// 	planetresfile: "c:\\games\\tnova\\data\\RESPLNT0.RES",
			// 		// 	planetname: "planet0", pal);
			//  	}
			//}
			// Resloader.FindDataInRes("c:\\games\\tnova\\PLNT0.PAL", file, 255);
		}

		//PaletteLoader.LoadPalette(palettefile: "c:\\games\\tnova\\data\\RESGAME.RES", chunkid: 351, 0);

		TNovaMapLoader.LoadTNovaMap("c:\\games\\tnova\\TNF108\\MAPS\\COAST.RES");
		//a compliant loader will pull the plant from the map file.
		TNovaMapLoader.LoadPlanetTextures(
			planetresfile: "c:\\games\\tnova\\data\\RESPLNT0.RES",
			planetname: "planet0");

		TilemapRender.RenderTileMap(TNovaMapLoader.height, TNovaMapLoader.texture);

		//Resloader.FindDataInRes("c:\\games\\tnova\\PLNT0.PAL", "c:\\games\\tnova\\data\\RESGAME.RES", 255 * 3);
		//palchunk_351.dat
		//Resloader.FindDataInRes("c:\\games\\tnova\\PLNT1.PAL","c:\\temp\\tnova\\palchunk_351.dat", 255);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
