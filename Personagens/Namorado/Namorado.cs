using Godot;
using System;

public partial class Namorado : CharacterBody2D
{
	/*constantes*/
	private const int GravNum = 1500;
	private const int Impulso = -2000; 
	private const int Velo = 500; 
	
	/*variaveis*/
	[Export]
	private int Gravity = GravNum;
	private int Speed { get; set; } = Velo;
	private int Direction {get; set; } = 1;
	private bool IsJumping {get; set; } = false;
	private bool BackupOnFloor {get; set; } = true;
	private bool IsDashing {get; set; } = false;
	
	/*nós*/
	private Timer TimerDash; 
	private Timer TimerPulo; 
	
	public override void _Ready(){
		TimerDash = GetNode<Timer>("TimerDash");
		TimerDash.Timeout += FimDoDash;
		TimerPulo = GetNode<Timer>("TimerPulo");
		TimerPulo.Timeout += FimDoPulo;
	}
	
	public override void _Process(double delta)
	{
		Gravidade();
		Anda();
		Dash();
		Pula();
		DebugaPulo();
	}
	
	private void Gravidade(){
		Velocity = new Vector2(0, Gravity);
		MoveAndSlide();
	}
	
	private void Anda(){
		Vector2 inputDirection = Input.GetVector("left", "right", "down", "up");
		if(!IsDashing){
			if(inputDirection.X != 0){
				Direction = (int)inputDirection.X;
			}
			Velocity = inputDirection * Speed;
			MoveAndSlide();
		}
	}
	
	private void Pula(){
		if(Input.IsActionJustPressed("pulo") && !IsJumping){
			Gravity = Impulso;
			TimerPulo.Start();
		}
		
		AdministraPulo();
	}
	
	private bool Backup = false;
	private void DebugaPulo(){
		if(Backup != IsJumping){
			Backup = IsJumping;
		}
	}
	
	private void FimDoPulo(){
		if(!IsDashing){
			Gravity = GravNum;
		}
	}
	
	private void AdministraPulo(){
		if(IsOnFloor() != BackupOnFloor){
			BackupOnFloor = IsOnFloor();
			IsJumping = !IsJumping;
		}
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
			Velocity = new Vector2(1000*Direction, 0);
			Gravity = 0;
			MoveAndSlide();
		}
	}
	
	private void FimDoDash(){
		IsDashing = false;
		Gravity = GravNum;
	}
}
