using Godot;
using System;

public partial class Chefe : CharacterBody2D
{
	/*a velocidade e impulso tá menor que a do namorado que fiz. Isso é algo
	que devemos mexer, provavelmente*/
	public const float Speed = 300.0f, Gravity = 980f; 
	public const float JumpVelocity = -400.0f;
	private float health = 0, cooldown = 1, damage = 1, actTime = 1; //Cooldown: time between actions, smaller means more aggresive
	private int state = 0, onWall = 0, enable = 1, decideDir = 2; //onWall: 1 left, 0 none, -1 right
	private CharacterBody2D player;
	private Marker2D headPos;
	private Vector2 viewSize;
	private Timer timerGetBoyfriend;
	[Signal]
	public delegate void SurfaceEventHandler();
	[Signal]
	public delegate void JumpedEventHandler();
	
	public override void _Ready()
	{
		headPos = GetNode<Marker2D>("Head");
		viewSize = GetViewport().GetVisibleRect().Size;
		player = GetTree().Root.GetNode<Node2D>("Sala de Jantar").GetNode<CharacterBody2D>("Namorado");
		Velocity = Vector2.One;
		Surface += phases; Jumped += coolJump;
		timerGetBoyfriend = GetNode<Timer>("TimerPegaNamorado");
		timerGetBoyfriend.Timeout += getBoyfriend;
	}

	public override void _PhysicsProcess(double delta)
	{
		if(Velocity.X == 0) Velocity = Vector2.Zero;
		if(!Velocity.IsZeroApprox()){
			if(!IsOnWall() && !IsOnFloor())
				Velocity = new Vector2(Velocity.X, Velocity.Y+Gravity*(float)delta);
		}
		else{
			Velocity = Vector2.Zero;
		}
		if(IsOnWall() && enable == 1){
			Velocity = Vector2.Zero;
			enable = 0;
			cooldown = 3.5f;
			GD.Print("Wall");
			GD.Print(headPos.GlobalPosition.X < viewSize.X/2);
			if(headPos.GlobalPosition.X < viewSize.X/2)
				onWall  = 1;
			else
				onWall = -1;
			decideDir = -onWall;
			EmitSignal(SignalName.Surface);
		}
		if(IsOnFloor()){
			onWall = 0;
			Velocity = Vector2.Zero;
			if(enable == 1){
				enable = 0;
				cooldown = 2;
				GD.Print("Floor");
				EmitSignal(SignalName.Surface);
			}
		}
		MoveAndSlide();
		
	}

	private void phases(){
		switch(state){
			case 0:
				phase1();
				break;
			default:
				break;
		}
	}
	
	private async void coolJump(){
		await ToSignal(GetTree().CreateTimer(cooldown), "timeout");
		//GD.Print("Cool: "+cooldown);
		enable = 1;
		GD.Print("Enabled");
	}
	private void phase1(){
		GD.Print("Go");
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
		float whereJump = viewSize.Y-(float)GD.RandRange(jumpWindow*0.5, jumpWindow*1.5)-headPos.GlobalPosition.Y;
		//GD.Print("Where: "+whereJump);
		Vector2 Dist;
		if(onWall == 0)
			Dist = new Vector2((1-dir)/2*viewSize.X-headPos.GlobalPosition.X, whereJump);
		else{
			float X = (player.Position.X) -headPos.GlobalPosition.X + (float)GD.RandRange(-viewSize.X*0.1,viewSize.X*0.1);
			X = Math.Clamp(X, -headPos.GlobalPosition.X, viewSize.X-headPos.GlobalPosition.X);
			Dist = new Vector2(X, player.Position.Y-headPos.GlobalPosition.Y);
		}
		GD.Print("Dist: "+Dist);
		setVelBoss(1, Dist);
		EmitSignal(SignalName.Jumped);
	}
	private void setVelBoss(float time, Vector2 Dist){
		if(onWall == 0){
			Velocity = new Vector2(Dist.X/time, -time*Gravity);
		}else{
			Velocity = new Vector2(Dist.X/time, (Dist.Y/time)-Gravity*(float)Math.Pow(time,2)/2);
		}
	}
	
	public void dontGetBoyfriend(){
		SetCollisionMaskValue(2, false);
	}
	
	private void getBoyfriend(){
		SetCollisionMaskValue(2, true);
	}
}
