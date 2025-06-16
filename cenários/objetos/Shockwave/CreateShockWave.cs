using Godot;
using System;

public partial class CreateShockWave : Node2D
{
	private PackedScene shockwave;
	private Shockwave shockLeft, shockRight;
	private float speed = 800, acc = 2E4f, time = 0;
	// Called when the node enters the scene tree for the first time.
	public CreateShockWave(float x, float y)
	{
		GlobalPosition = new Vector2(x, y);
	}
	public override void _Ready()
	{
		shockwave = GD.Load<PackedScene>("res://cenários/objetos/Shockwave/Shockwave.tscn");
		shockLeft = (Shockwave)shockwave.Instantiate();
		shockRight = (Shockwave)shockwave.Instantiate();
		AddChild(shockLeft); AddChild(shockRight);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		time += (float)delta;
		if (shockLeft != null)
		{
			if (!shockLeft.OutViewport())
			{
				shockLeft.Position = Vector2.Left * speed * time + Vector2.Up * (speed * time * 0 + acc * time * time / 2);
				if (Math.Abs(shockLeft.Position.Y) >= Math.Abs(shockLeft.TextureSize.Y)) shockLeft.Position = new Vector2(shockLeft.Position.X, Math.Clamp(shockLeft.Position.Y, -shockLeft.TextureSize.Y, shockLeft.TextureSize.Y));
			}
			else
			{
			shockLeft.QueueFree();
			shockLeft = null;
			}
		}

		if (shockRight != null)
		{
			if (!shockRight.OutViewport())
			{
				shockRight.Position = Vector2.Right * speed * time + Vector2.Up * (speed * time * 0 + acc * time * time / 2);
				if (Math.Abs(shockRight.Position.Y) >= Math.Abs(shockRight.TextureSize.Y)) shockRight.Position = new Vector2(shockRight.Position.X, Math.Clamp(shockRight.Position.Y, -shockRight.TextureSize.Y, shockRight.TextureSize.Y));
			}
			else
			{
			shockRight.QueueFree();
			shockRight = null;
			}
		}
		
		if (shockLeft == null && shockRight == null) QueueFree();
	}
}
