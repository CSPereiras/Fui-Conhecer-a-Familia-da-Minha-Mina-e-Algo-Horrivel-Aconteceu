using Godot;
using System;

public partial class Chefe : CharacterBody2D
{
	/*a velocidade e impulso tá menor que a do namorado que fiz. Isso é algo
	que devemos mexer, provavelmente*/
	public const float Speed = 300.0f; 
	public const float JumpVelocity = -400.0f;
	private float health = 0, cooldown = 1, damage = 1; //Cooldown: time between actions, smaller means more aggresive
	private int state = 0, onWall = 0; //onWall: -1 left, 0 none, 1 right
	private CharacterBody2D player;

	public override void _Ready(){

	}

	public override void _PhysicsProcess(double delta)
	{
	}
}
