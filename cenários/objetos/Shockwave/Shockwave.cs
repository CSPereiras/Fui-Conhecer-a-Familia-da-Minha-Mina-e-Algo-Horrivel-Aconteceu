using Godot;
using System;

public partial class Shockwave : Node2D
{
	public Vector2 TextureSize, viewSize;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		TextureSize = GetNode<Area2D>("Area2D").GetNode<CollisionShape2D>("CollisionShape2D").Shape.GetRect().Size;
		viewSize = viewSize = GetViewport().GetVisibleRect().Size;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void OnArea2DBodyEntered(Node2D body)
	{
		if (body.Name != "Namorado") return;
		((Namorado)body).Dano();
	}
	public bool OutViewport()
	{
		return GlobalPosition.X < -TextureSize.X / 2 || GlobalPosition.X > viewSize.X + TextureSize.X / 2;
	}
}
