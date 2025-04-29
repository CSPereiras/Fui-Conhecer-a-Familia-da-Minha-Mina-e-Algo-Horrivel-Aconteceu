using Godot;
using System;

public partial class Namorado : CharacterBody2D
{
	[Export]
	private int Speed { get; set; } = 500;
	private int Direction {get; set; } = 1;
	private bool IsDashing {get; set; } = false;
	
	/*nós*/
	private Timer TimerDash; 
	
	public override void _Ready(){
		TimerDash = GetNode<Timer>("TimerDash");
		TimerDash.Timeout += FimDoDash;
	}
	
	public override void _Process(double delta)
	{
		Anda();
		Dash();
		/*Pula();*/
	}
	
	private void Anda(){
		Vector2 inputDirection = Input.GetVector("left", "right", "down", "up");
		if(inputDirection.X != 0){
			Direction = (int)inputDirection.X;
		}
		Velocity = inputDirection * Speed;
		MoveAndSlide();
	}
	
	private void Dash(){
		if(Input.IsActionJustPressed("dash")){
			IsDashing = true;
			TimerDash.Start();
		}
		
		MovimentoDash();
	}
	
	private void MovimentoDash(){
		if(IsDashing){
			GD.Print("entrou");
			Velocity = new Vector2(100*Direction, 0);
		}
	}
	
	private void FimDoDash(){
		IsDashing = false;
	}
	
	/*private void Pula(){
		if(Input.IsJusPressed()){
			
		}
	}*/
}
