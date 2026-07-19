using Godot;
using System;
using FreeFall;
using System.IO;
public partial class main : Node
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var files = System.IO.Directory.EnumerateFiles("c:\\games\\tnova\\TNF108\\MAPS\\", "*.res");
		foreach (var file in files)
		{
			var output = Path.Combine(path1: "c:\\temp\\tnova", path2: $"{System.IO.Path.GetFileNameWithoutExtension(file)}.png");
			//TNovaMapLoader.BuildTNovaMap(file, output);	
		}
		
		//var enumoptions = new EnumerationOptions();
		//enumoptions.RecurseSubdirectories = true;
		files = System.IO.Directory.EnumerateFiles(path: "c:\\games\\tnova\\data", searchPattern: "*.res");//, enumerationOptions: enumoptions);
		foreach (var file in files)
		{
			//Resloader.EnumerateResFile(file);
		}

		TNovaMapLoader.DumpPlanet("c:\\games\\tnova\\data\\RESPLNT0.RES", "planet0");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
