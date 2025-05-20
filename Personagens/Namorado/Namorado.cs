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
	private int Lifes {get; set;} = 5; //vidas
	private int Gravity { get; set; } = GravNum; //gravidade
	private int Speed { get; set; } = Velo; //velocidade
	private int Direction { get; set; } = 1; //direção (para onde o jogador "aponta")
	private bool IsImmune { get; set; } = false; //se o jogador tá imune
	private bool IsJumping { get; set; } = false; //se o jogador está pulando
	private bool BackupOnFloor { get; set; } = true; /* variavel auxiliar para 
	ajudar a ver se o jogador está no chão*/
	private bool IsDashing { get; set; } = false; //se o jogador tá fazendo o dash
	private bool IsPunching { get; set; } = false; //se o jogador está socando
	
	/*nós*/
	private CollisionShape2D ColisaoDireita; /*colisão normal*/
	private CollisionShape2D ColisaoSocoDireita; /*colisão normal*/
	private CollisionShape2D ColisaoEsquerda; /*colisão invertida*/
	private CollisionShape2D ColisaoSocoEsquerda; /*colisão invertida*/
	private Timer TimerImunidade;
	private Timer TimerDash; 
	private Timer TimerPulo;  
	private Area2D Soco;
	private Timer TimerSoco;
	private AnimatedSprite2D Sprite;
	
	public override void _Ready(){
		/*colisões*/
		ColisaoDireita = GetNode<CollisionShape2D>("ColisaoNamorado1");
		ColisaoEsquerda = GetNode<CollisionShape2D>("ColisaoNamorado2");
		ColisaoEsquerda.Disabled = true;
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
		ColisaoSocoDireita = GetNode<CollisionShape2D>("Soco/ColisaoSoco1");
		ColisaoSocoEsquerda = GetNode<CollisionShape2D>("Soco/ColisaoSoco2");
		ColisaoSocoEsquerda.Disabled = true;
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
	
	/*Faz o tratamento da gravidade*/
	private void Gravidade(){
		Velocity = new Vector2(0, Gravity);
		MoveAndSlide();
	}
	
	/*Gerencia todo o movimento, o que inclui fazer o movimento básico de andar
	pros lados e chamar as funções auxiliares que cuidam do pulo, dash e 
	orientação do sprite*/
	private void Movimenta(){
		Vector2 inputDirection = Input.GetVector("left", "right", "down", "up");
		if(!IsDashing){
			if(inputDirection.X != 0){
				Direction = (int)inputDirection.X;
			}
			Velocity = inputDirection * Speed;
			/*MoveAndSlide();*/
		}
		
		AdministraSprite();
		Dash();
		Pula();
	}
	
	/*Gerencia a orientação do sprite*/
	private void AdministraSprite(){
		if(Direction == 1){
			Sprite.FlipH = false;
			ColisaoDireita.Disabled = false;
			ColisaoSocoDireita.Disabled = false;
			ColisaoEsquerda.Disabled = true;
			ColisaoSocoEsquerda.Disabled = true;
		}else{
			Sprite.FlipH = true;
			ColisaoDireita.Disabled = true;
			ColisaoSocoDireita.Disabled = true;
			ColisaoEsquerda.Disabled = false;
			ColisaoSocoEsquerda.Disabled = false;
		}
	}
	
	/*Gerencia a colisão do jogador com o chefe, retirando a vida dele e mexendo
	em outras coisas necessárias*/
	private void AdministraColisao(Node2D Colisor){
		if(Colisor.Name.Equals("Chefe") && !IsImmune){
			IsImmune = true;
			Lifes--;
			GD.Print("Vidas restantes: " + Lifes);
			VerificaVidas();
			SetCollisionMaskValue(3, false);
			TimerImunidade.Start();
		}
	} 
	
	/*Verifica quantas vidas tem e, caso seja zero, o jogador morre*/
	private void VerificaVidas(){
		if(Lifes <= 0){
			QueueFree();
		}
	}
	
	/*Recebe o sinal do timeout do timer da imunidade e faz o tratamento*/
	private void FimDaImunidade(){
		SetCollisionMaskValue(3, true);
		IsImmune = false;
	}
	
	/*Mecânica do pulo*/
	private void Pula(){
		if(Input.IsActionJustPressed("pulo") && !IsJumping){
			Gravity = Impulso;
			TimerPulo.Start();
		}
		
		AdministraPulo();
	}
	
	/*Recebe o sinal do timeout do timer do pulo e faz o tratamento*/
	private void FimDoPulo(){
		if(!IsDashing){
			Gravity = GravNum;
		}
	}
	
	/*Retorna a possibilidade do jogador pular*/
	private void AdministraPulo(){
		if(IsOnFloor() != BackupOnFloor){
			BackupOnFloor = IsOnFloor();
			IsJumping = !IsJumping;
		}
	}
	
	/*Mecânica do dash*/
	private void Dash(){
		if(Input.IsActionJustPressed("dash") && !IsDashing && !IsPunching){
			IsDashing = true;
			TimerDash.Start();
		}
		
		MovimentoDash();
	}
	
	/*Gerencia o movimento do dahs*/
	private void MovimentoDash(){
		if(IsDashing){
			Velocity = new Vector2(1000*Direction, 0);
			Gravity = 0;
		}
	}
	
	/*Recebe o sinal do timeout do timer do dash e faz o tratamento*/
	private void FimDoDash(){
		IsDashing = false;
		Gravity = GravNum;
	}
	
	/*Mecânica do soco*/
	private void Bate(){
		if(Input.IsActionJustPressed("soco") && !IsPunching && !IsDashing){
			IsPunching = true;
			Sprite.Frame = 1;
			Soco.Monitoring = true;
			TimerSoco.Start();
		}
	}
	
	/*Recebe o sinal do timeout do timer do soco e faz o tratamento*/
	private void FimDoSoco(){
		IsPunching = false;
		Sprite.Frame = 0;
		Soco.Monitoring = false;
	}
	
	/*Recebe o sinal da colisão do soco e faz o tratamento disso*/
	private void SocoColisao(Node2D body){
		GD.Print("Namorado acaba de executar um soco no "+body.Name+"!");
	}
}
