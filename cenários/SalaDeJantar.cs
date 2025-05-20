using Godot;
using System;

public partial class SalaDeJantar : Node2D
{
	private Namorado Namorado;
	private Chefe Chefe;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready(){
		Namorado = GetNode<Namorado>("Namorado");
		Chefe = GetNode<Chefe>("Chefe");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
