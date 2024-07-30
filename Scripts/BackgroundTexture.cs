using Godot;
using System;

public partial class BackgroundTexture : TextureRect
{
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		SetSize(GetViewportRect().Size);
	}
}
