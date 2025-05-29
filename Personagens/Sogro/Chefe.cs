using Godot;
using System;

public partial class Chefe : CharacterBody2D
{
	/*a velocidade e impulso tá menor que a do namorado que fiz. Isso é algo
	que devemos mexer, provavelmente*/
	[Export]
	private float actTime1 = 1, Gravity = G, JumpVelocity = JumpV, health = maxHealth, baseCool = 1;
	public const float Speed = 500.0f, G = 980f, JumpV = -900.0f, maxHealth = 100;
	private float cooldown = 1, damage = 1, trackingFactor = 0.7f; //Cooldown: time between actions, smaller means more aggresive
	private int state = 0, onWall = 0, enable = 1, decideDir = 2; //onWall: 1 left, 0 none, -1 right
	private bool enableGrav = true, checkVelNearZero = false, enableFall = false;
	private AnimatedSprite2D spriteChefe;
	private CharacterBody2D player;
	private Marker2D headPos, lPos, rPos, dPos;
	private Vector2 viewSize;
	[Signal]
	public delegate void PhaseTransEventHandler();
	[Signal]
	public delegate void FallImpactEventHandler();
	[Signal]
	public delegate void JumpedEventHandler();

	public override void _Ready()
	{
		headPos = GetNode<Marker2D>("Head"); lPos = GetNode<Marker2D>("Left"); rPos = GetNode<Marker2D>("Right"); dPos = GetNode<Marker2D>("Down");
		viewSize = GetViewport().GetVisibleRect().Size;
		player = GetTree().Root.GetNode<Node2D>("Sala de Jantar").GetNode<CharacterBody2D>("Namorado");
		Velocity = Vector2.One;
		PhaseTrans += Phases;
		Jumped += CoolJump;
		FallImpact += FallDownImpact;
		spriteChefe = GetNode<AnimatedSprite2D>("SpriteChefe");
	}

	public override void _PhysicsProcess(double delta)
	{
		ChangePhase();
		if (!Velocity.IsZeroApprox())
		{
			if (!IsOnWallBoss() && !IsOnFloor() && enableGrav)
				Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * (float)delta);
		}
		else
		{
			Velocity = Vector2.Zero;
		}
		if ((IsOnWallBoss() || onWall != 0) && state == 0)
		{
			Velocity = Vector2.Zero;
			cooldown = baseCool*3;
			/*GD.Print("Wall");
			GD.Print(headPos.GlobalPosition.X < viewSize.X/2);*/
			if (headPos.GlobalPosition.X < viewSize.X / 2)
				onWall = 1;
			else
				onWall = -1;
			decideDir = -onWall;
			if (enable == 1)
			{
				enable = 0;
				//GD.Print("WallPhase");
				EmitSignal(SignalName.PhaseTrans);
			}

		}

		if (IsOnFloor())
		{
			onWall = 0;
			if (!IsOnWallBoss() && state == 0)
				Velocity = Vector2.Zero;
			if (enable == 1 && state == 0)
			{
				cooldown = baseCool*2;
				enable = 0;
				//GD.Print("FloorPhase");
				EmitSignal(SignalName.PhaseTrans);
			}
		}
		if (enable == 1 && state != 0)
		{
			enable = 0;
			EmitSignal(SignalName.PhaseTrans);
		}
		/*
		if (Input.IsActionJustPressed("soco"))
		{
			health -= 25;
			GD.Print(state);
		}*/
		MoveAndSlide();
		orientation();
	}
	
	//-------etapa 1
	private void Phases()
	{
		switch (state)
		{
			case 0:
				Phase1();
				break;
			case 1:
				phase2();
				break;
			case 2:
				if (checkVelNearZero && ChangedDirectionY(Velocity.Y))
				{
					//GD.Print("Fall Impact");
					Velocity = Vector2.Zero; enableGrav = false;
					EmitSignal(SignalName.FallImpact);
				}
				Phase3();
				break;
			default:
				break;
		}
	}

	private async void CoolJump()
	{
		await ToSignal(GetTree().CreateTimer(cooldown), "timeout");
		//GD.Print("Cool: "+cooldown);
		enable = 1;
		/*GD.Print("Enabled");*/
	}
	private void orientation(){
		/*if(getPlayerPosition()){
			//GD.Print(getPlayerPosition());
			spriteChefe.FlipH = false;
		}else{
			//GD.Print(getPlayerPosition());
			spriteChefe.FlipH = true;
		}*/
		spriteChefe.FlipH = !getPlayerPosition(); 
	}
	private bool getPlayerPosition(){
		return player.Position.X-Position.X > 0;
	}

	private void Phase1()
	{
		if (decideDir == 2)
		{
			if (GD.Randi() % 2 == 1)
			{
				JumpNow(1, onWall);
				decideDir = -1;
			}
			else
			{
				JumpNow(-1, onWall);
				decideDir = 1;
			}
			return;
		}

		switch (onWall)
		{
			case 0:
				JumpNow(decideDir, onWall);
				break;
			case 1:
				JumpNow(-1, onWall);
				break;
			case -1:
				JumpNow(1, onWall);
				break;
		}
	}
	private void JumpNow(int dir, int mode)
	{
		//GD.Print(":: "+dir);
		float whereJump = JumpWindowY();
		//GD.Print("Where: "+whereJump);
		Vector2 Dist;
		if (mode == 0)
		{
			Dist = new Vector2((1 - dir) / 2 * viewSize.X - headPos.GlobalPosition.X, whereJump);
		}
		else
		{
			Dist = new Vector2(JumpToPlayerX(), viewSize.Y-dPos.GlobalPosition.Y);
		}
		//GD.Print("Dist: "+Dist+" Player: "+player.GlobalPosition+ " Boss"+ GlobalPosition);
		SetVelBoss(actTime1, Dist);
		onWall = 0;
		EmitSignal(SignalName.Jumped);
	}

	private float JumpWindowY()
	{
		double jumpWindow = viewSize.Y * (float)0.5;
		float whereJump = viewSize.Y - (float)GD.RandRange(jumpWindow, jumpWindow * 1.8) - headPos.GlobalPosition.Y;
		return whereJump;
	}
	private float JumpToPlayerX()
	{
		float X = player.GlobalPosition.X - GlobalPosition.X+ (float)GD.RandRange(-viewSize.X * 0.01, viewSize.X * 0.01);
		X = Math.Clamp(X, -GlobalPosition.X, viewSize.X - GlobalPosition.X);
		if (X + lPos.GlobalPosition.X < 50) X = player.GlobalPosition.X - GlobalPosition.X + viewSize.X * 0.05f; 
		if (X + rPos.GlobalPosition.X > viewSize.X - 50) X = player.GlobalPosition.X - GlobalPosition.X - viewSize.X * 0.05f; 
		return X;
	}
	private void SetVelBoss(float time, Vector2 Dist)
	{
		Velocity = new Vector2(Dist.X / time, (Dist.Y / time) - Gravity * time/ 2);
		//GD.Print("Boss Vel: "+Velocity+" Boss Pos: "+ GlobalPosition+" Player: "+ player.GlobalPosition);
	}
	public bool IsOnWallBoss()
	{
		if (lPos.GlobalPosition.X <= 0)
		{
			GlobalPosition = new Vector2(-lPos.Position.X, GlobalPosition.Y);
			return true;
		}
		if (rPos.GlobalPosition.X >= viewSize.X)
		{
			GlobalPosition = new Vector2(viewSize.X-rPos.Position.X, GlobalPosition.Y);
			return true;
		}
		return false;
	}

	//-------etapa 2
	bool isRunningOver = false;
	Vector2 direction = new Vector2();
	private void phase2(){
		tryRunOver();
	}
	private void tryRunOver(){
		if(isRunningOver){
			if(spriteChefe.FlipH){
				direction.X = -1;
			}else{
				direction.X = 1;
			}
			isRunningOver = false;
		}
		Velocity = direction * Speed;
		//GD.Print(Velocity);
	}
	private void Phase3()
	{
		if (!checkVelNearZero && IsOnFloor())
		{
			Velocity = new Vector2(0, JumpVelocity);
			checkVelNearZero = true;
		}
		if (!enableFall)
		{
			float VelX = player.GlobalPosition.X - headPos.GlobalPosition.X;
			float AbsVelX = Math.Abs(VelX);
			if (AbsVelX < 10) VelX = 0;
			else if (AbsVelX < Speed) VelX *= Speed / AbsVelX;
			Velocity = new Vector2(VelX*trackingFactor, Velocity.Y);
			//GD.Print("P3: "+Velocity);
		}
		enable = 1;
	}
	private void ChangePhase()
	{
		float perc = (float)(health / maxHealth);
		if (perc >= .8f)
		{
			int phase = 0;
			if (state != phase) enable = 1;
			state = phase;
		}
		else if (perc >= .6f)
		{
			int phase = 1;
			if (state != phase) enable = 1;
			state = phase;
		}
		else if (perc >= .4f)
		{
			int phase = 2;
			if (state != phase) enable = 1;
			state = phase;
		}
		else if (perc >= .2f)
		{
			int phase = 3;
			if (state != phase) enable = 1;
			state = phase;
		}
		else if (perc > 0)
		{
			int phase = 4;
			if (state != phase) enable = 1;
			state = phase;
		}
		else
		{
			int phase = -1;
			if (state != phase) enable = 1;
			state = phase;
		}
	}
	private async void FallDownImpact()
	{
		await ToSignal(GetTree().CreateTimer(baseCool), "timeout");
		enableFall = enableGrav = true;
		Velocity = new Vector2(0, -JumpVelocity);
		Gravity = 3*G;
		//implement shockwave
		await ToSignal(GetTree().CreateTimer(baseCool), "timeout");
		Gravity = G;
		enableFall = checkVelNearZero = false;
	}
	public static bool IsZeroApprox(float val)
	{
		const float EPSILON = 0.00001f;
		if (val <= EPSILON && val >= EPSILON) return true;
		return false;
	}
	public static bool ChangedDirectionY(float val)
	{
		if (val < 0 && val + G / 60 > 0) return true;
		return false;
	}
}
