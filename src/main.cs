using Godot;

using FreeFall;
using System.IO;

public partial class main : Node
{
	public static main instance;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var mapToLoad = "c:\\games\\tnova\\TNF108\\MAPS\\MAP7.RES";
		var planetToLoad = "c:\\games\\tnova\\data\\RESPLNT0.RES";
		var skyToLoad = "c:\\games\\tnova\\data\\SKY0.RES";
		var palettename = "PLNT0.PAL";
		instance = this;
		// var files = System.IO.Directory.EnumerateFiles("c:\\games\\tnova\\TNF108\\MAPS\\", "*.res");
		// foreach (var file in files)
		// {
		// 	var output = Path.Combine(path1: "c:\\temp\\tnova", path2: $"{System.IO.Path.GetFileNameWithoutExtension(file)}.png");
		// 	//TNovaMapLoader.BuildTNovaMap(file, output);	
		// }

		var enumoptions = new EnumerationOptions();
		enumoptions.RecurseSubdirectories = true;
		var files = System.IO.Directory.EnumerateFiles(path: "c:\\games\\tnova\\", searchPattern: "COAST.RES", enumerationOptions: enumoptions);
		foreach (var file in files)
		{
			var chunks = Resloader.EnumerateResFile(file);
		}

		//PaletteLoader.LoadPalette(palettefile: "c:\\games\\tnova\\data\\RESGAME.RES", chunkid: 351, 0);

		// Load the hi-res map
		var HiResMap = TNovaMapLoader.LoadTNovaMap(
			sourcearkfile: mapToLoad, 
			HiRes: true, 
			outputfilename: "c:\\temp\\testmap.png");

		var LoResMap = TNovaMapLoader.LoadTNovaMap(
			sourcearkfile: mapToLoad, 
			HiRes: false, 
			outputfilename: "c:\\temp\\testmap_lo.png", 
			excludeX0: 64, 
			excludeX1: 191, 
			excludeY0: 64, 
			excludeY1: 191);

		var TreeMap = TNovaMapLoader.LoadTreeMap(
				sourcearkfile: mapToLoad, 
				outputfilename: "c:\\temp\\testmap_trees.png");
			
		//a compliant loader will pull the planet from the map file.
		TNovaMapLoader.LoadPlanetTextures(
			planetresfile: planetToLoad,
			palettename: palettename,
			planetname: "planet1");

		TNovaMapLoader.LoadSky(
			skyresfile: skyToLoad,
			palettename: palettename,
			skyname: "sky0");			

		TilemapRender.RenderTileMap(
			map : HiResMap, 
			PositionAdjustment: 64 * LoResMap.UnitSize,
			renderSky: false, 
			Root: "/root/Freefall/Planet/HiRes");

		TilemapRender.RenderTileMap(
			map : LoResMap, 
			renderSky: true, 
			Root: "/root/Freefall/Planet/LoRes");

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
