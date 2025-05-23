using Godot;
using System;

public partial class Chefe : CharacterBody2D
{
	/*a velocidade e impulso tá menor que a do namorado que fiz. Isso é algo
	que devemos mexer, provavelmente*/
	[Export]
		private float actTime = 1; 
	public const float Speed = 300.0f, Gravity = 980f; 
	public const float JumpVelocity = -400.0f;
	private float health = 0, cooldown = 1, damage = 1; //Cooldown: time between actions, smaller means more aggresive
	private int state = 0, onWall = 0, enable = 1, decideDir = 2; //onWall: 1 left, 0 none, -1 right
	private AnimatedSprite2D spriteChefe;
	private CharacterBody2D player;
	private Marker2D headPos, lPos, rPos;
	private Vector2 viewSize;
	[Signal]
	public delegate void SurfaceEventHandler();
	[Signal]
	public delegate void JumpedEventHandler();
	
	public override void _Ready()
	{
		headPos = GetNode<Marker2D>("Head"); lPos = GetNode<Marker2D>("Left"); rPos = GetNode<Marker2D>("Right");
		viewSize = GetViewport().GetVisibleRect().Size;
		player = GetTree().Root.GetNode<Node2D>("Sala de Jantar").GetNode<CharacterBody2D>("Namorado");
		Velocity = Vector2.One;
		Surface += phases; 
		Jumped += coolJump;
		spriteChefe = GetNode<AnimatedSprite2D>("SpriteChefe");
	}

	public override void _PhysicsProcess(double delta)
	{
		if(!Velocity.IsZeroApprox()){
			if(!IsOnWallBoss() && !IsOnFloor())
				Velocity = new Vector2(Velocity.X, Velocity.Y+Gravity*(float)delta);
		}
		else{
			Velocity = Vector2.Zero;
		}
		if(IsOnWallBoss() || onWall != 0){
			Velocity = Vector2.Zero;
			cooldown = 3.5f;
			/*GD.Print("Wall");
			GD.Print(headPos.GlobalPosition.X < viewSize.X/2);*/
			if(headPos.GlobalPosition.X < viewSize.X/2)
				onWall  = 1;
			else
				onWall = -1;
			decideDir = -onWall;
			if(enable == 1){
				enable = 0;
				EmitSignal(SignalName.Surface);
			}
		}
		if(IsOnFloor()){
			onWall = 0;
			if(!IsOnWallBoss())
				Velocity = Vector2.Zero;
			if(enable == 1){
				enable = 0;
				cooldown = 2;
				/*GD.Print("Floor");*/
				EmitSignal(SignalName.Surface);
			}
		}
		

		MoveAndSlide();
		
	}

	//-------etapa 1
	private void phases(){
		switch(state){
			case 0:
				phase1();
				break;
			case 1:
				phase2();
				break;
			default:
				break;
		}
	}
	
	private async void coolJump(){
		await ToSignal(GetTree().CreateTimer(cooldown), "timeout");
		//GD.Print("Cool: "+cooldown);
		enable = 1;
		/*GD.Print("Enabled");*/
	}
	private void phase1(){
		/*GD.Print("Go");*/
		whereGo();
	}
	private void whereGo(){
		//GD.Print("ON: "+ onWall);
		if(decideDir == 2){
			if(GD.Randi()%2 == 1){
				jumpNow(1);
				decideDir = -1;
			}else{
				jumpNow(-1);
				decideDir = 1;
			}
			return;
		}
			
		switch(onWall){
			case 0:
				jumpNow(decideDir);
				break;
			case 1:
				jumpNow(-1);
				break;
			case -1:
				jumpNow(1);
				break;
		}
	}
	private void jumpNow(int dir){
		//GD.Print(":: "+dir);
		double jumpWindow = viewSize.Y*(float)0.5;
		float whereJump = viewSize.Y-(float)GD.RandRange(jumpWindow, jumpWindow*1.8)-headPos.GlobalPosition.Y;
		//GD.Print("Where: "+whereJump);
		Vector2 Dist;
		if(onWall == 0)
			Dist = new Vector2((1-dir)/2*viewSize.X-headPos.GlobalPosition.X, whereJump);
		else{
			float X = (player.Position.X) -headPos.GlobalPosition.X + (float)GD.RandRange(-viewSize.X*0.08,viewSize.X*0.08);
			X = Math.Clamp(X, -headPos.GlobalPosition.X, viewSize.X-headPos.GlobalPosition.X);
			if(X+lPos.GlobalPosition.X<0) X = (player.Position.X) -headPos.GlobalPosition.X + viewSize.X*0.05f;
			if(X+rPos.GlobalPosition.X>viewSize.X) X = (player.Position.X) -headPos.GlobalPosition.X - viewSize.X*0.05f;
			Dist = new Vector2(X, headPos.GlobalPosition.Y);
		}
		//GD.Print("Dist: "+Dist);
		setVelBoss(actTime, Dist);
		onWall = 0;
		EmitSignal(SignalName.Jumped);
	}
	private void setVelBoss(float time, Vector2 Dist){
		Velocity = new Vector2(Dist.X/time, (Dist.Y/time)-Gravity*time/2);
		//GD.Print(Velocity);
	}
	public bool IsOnWallBoss(){
		if(lPos.GlobalPosition.X <= 0 || rPos.GlobalPosition.X >= viewSize.X) return true;
		return false;
	}
	
	//-------etapa 2
	private void phase2(){
		
		//orientation();
	}
	
	private void orientation(){
		/*if(Namorado.Position.X-Position.X > 0){
			SpriteChefe.FlipH = true;
		}else{
			SpriteChefe.FlipH = false;
		}*/
	}
}
