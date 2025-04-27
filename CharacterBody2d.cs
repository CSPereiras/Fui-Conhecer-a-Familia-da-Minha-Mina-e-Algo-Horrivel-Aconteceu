using Godot;
using System;

public partial class CharacterBody2d : CharacterBody2D
{
	[Export]
	public int Speed { get; set; } = 10;
	
	public override void _Process(double delta)
	{
		GetInput();
	}
	
	public void GetInput(){
		Vector2 inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Velocity = inputDirection * Speed;
	}
}
