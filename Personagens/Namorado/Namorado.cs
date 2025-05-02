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
	private int Lifes {get; set;} = 5;
	private int Gravity { get; set; } = GravNum;
	private int Speed { get; set; } = Velo;
	private int Direction { get; set; } = 1;
	private bool IsImmune { get; set; } = false;
	private bool IsJumping { get; set; } = false;
	private bool BackupOnFloor { get; set; } = true;
	private bool IsDashing { get; set; } = false;
	private bool IsPunching { get; set; } = false;
	
	/*nós*/
	private Timer TimerImunidade;
	private Timer TimerDash; 
	private Timer TimerPulo;  
	private Area2D Soco;
	private Timer TimerSoco;
	private AnimatedSprite2D Sprite;
	
	public override void _Ready(){
		/*timer que cuida da imunidade*/
		TimerImunidade = GetNode<Timer>("TimerImunidade");
		TimerImunidade.Timeout += FimDaImunidade;
		/*timer que cuida do dash*/
		TimerDash = GetNode<Timer>("TimerDash");
		TimerDash.Timeout += FimDoDash;
		/*timer que cuida do pulo*/
		TimerPulo = GetNode<Timer>("TimerPulo");
		TimerPulo.Timeout += FimDoPulo;
		/*area2D e timer que cuida do soco*/
		Soco = GetNode<Area2D>("Soco");
		Soco.BodyEntered += SocoColisao; 
		Soco.Monitoring = false;
		TimerSoco = GetNode<Timer>("TimerSoco");
		TimerSoco.Timeout += FimDoSoco;
		/*sprite animado*/
		Sprite = GetNode<AnimatedSprite2D>("SpriteNamorado");
		GD.Print("Vidas restantes: " + Lifes);
	}
	
	public override void _Process(double delta)
	{
		Gravidade();
		Movimenta();
		KinematicCollision2D Colisao = MoveAndCollide(Velocity * (float)delta);
		if(Colisao != null){
			AdministraColisao(Colisao.GetCollider() as Node2D);
		}
		Bate();
	}
	
	private void Gravidade(){
		Velocity = new Vector2(0, Gravity);
		MoveAndSlide();
	}
	
	private void Movimenta(){
		Vector2 inputDirection = Input.GetVector("left", "right", "down", "up");
		if(!IsDashing){
			if(inputDirection.X != 0){
				Direction = (int)inputDirection.X;
			}
			Velocity = inputDirection * Speed;
			/*MoveAndSlide();*/
		}
		
		Dash();
		Pula();
	}
	
	private void AdministraColisao(Node2D Colisor){
		if(Colisor.Name.Equals("Chefe") && !IsImmune){
			IsImmune = true;
			Lifes--;
			GD.Print("Vidas restantes: " + Lifes);
			VerificaVidas();
			TimerImunidade.Start();
		}
	} 
	
	private void VerificaVidas(){
		if(Lifes <= 0){
			QueueFree();
		}
	}
	
	private void FimDaImunidade(){
		IsImmune = false;
	}
	
	private void Pula(){
		if(Input.IsActionJustPressed("pulo") && !IsJumping){
			Gravity = Impulso;
			TimerPulo.Start();
		}
		
		AdministraPulo();
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
		if(Input.IsActionJustPressed("dash") && !IsDashing && !IsPunching){
			IsDashing = true;
			TimerDash.Start();
		}
		
		MovimentoDash();
	}
	
	private void MovimentoDash(){
		if(IsDashing){
			Velocity = new Vector2(1000*Direction, 0);
			Gravity = 0;
		}
	}
	
	private void FimDoDash(){
		IsDashing = false;
		Gravity = GravNum;
	}
	
	private void Bate(){
		if(Input.IsActionJustPressed("soco") && !IsPunching && !IsDashing){
			IsPunching = true;
			Sprite.Frame = 1;
			Soco.Monitoring = true;
			TimerSoco.Start();
		}
	}
	
	private void FimDoSoco(){
		IsPunching = false;
		Sprite.Frame = 0;
		Soco.Monitoring = false;
	}
	
	private void SocoColisao(Node2D bodyx){
		GD.Print("Namorado acaba de executar um soco!");
	}
}
