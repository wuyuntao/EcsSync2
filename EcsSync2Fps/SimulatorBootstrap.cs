﻿namespace EcsSync2.Fps
{
	public static class SimulatorBootstrap
	{
		public static Simulator StartServer(Simulator.IContext context, int randomSeed, int port)
		{
			var simulator = new Simulator( context, true, false, randomSeed, null );
			InitializeSimulator( simulator );
			simulator.NetworkServer.Start( port );
			return simulator;
		}

		public static Simulator StartClient(Simulator.IContext context, ulong localUserId, string serverAddress, int serverPort)
		{
			var simulator = new Simulator( context, false, true, null, localUserId );
			InitializeSimulator( simulator );
			simulator.NetworkClient.Start( serverAddress, serverPort );
			return simulator;
		}

		public static Simulator StartStandalone(Simulator.IContext context, int randomSeed, ulong localUserId)
		{
			var simulator = new Simulator( context, true, true, randomSeed, localUserId );
			InitializeSimulator( simulator );
			return simulator;
		}

		static void InitializeSimulator(Simulator simulator)
		{
			var scene = simulator.SceneManager.LoadScene<BattleScene>();

			if( simulator.InputManager != null )
			{
				simulator.InputManager.RegisterJoystick( "Move", "Horizontal", "Vertical" );

				simulator.InputManager.RegisterButton( "Skill1", "Fire1" );
				simulator.InputManager.RegisterButton( "Skill2", "Fire2" );
				simulator.InputManager.RegisterButton( "Jump", "Jump" );

				simulator.InputManager.RegisterHandler( f => OnMoveCharacterCommand( f, scene ) );
				simulator.InputManager.RegisterHandler( f => OnJumpCommand( f, scene ) );
				simulator.InputManager.RegisterHandler( f => OnPlayerConnectCommand( f, scene ) );
			}
		}

		static void OnPlayerConnectCommand(CommandFrame frame, BattleScene scene)
		{
			if( scene.LocalPlayer != null && scene.LocalCharacter == null )
			{
				var button = scene.SceneManager.Simulator.InputManager.GetButton( "Jump" );
				if( button.Press )
				{
					var command = frame.AddCommand<PlayerConnectCommand>();
					command.ComponentId = scene.LocalPlayer.ConnectionManager.Id;
				}
			}
		}

		static void OnMoveCharacterCommand(CommandFrame frame, BattleScene scene)
		{
			if( scene.LocalCharacter != null )
			{
				var joystick = scene.SceneManager.Simulator.InputManager.GetJoystick( "Move" );

				var command = frame.AddCommand<MoveCharacterCommand>();
				command.ComponentId = scene.LocalCharacter.MotionController.Id;
				command.InputDirection = joystick.Direction;
				command.InputMagnitude = joystick.Magnitude;
			}
		}

		static void OnJumpCommand(CommandFrame frame, BattleScene scene)
		{
			if( scene.LocalCharacter != null )
			{
				var button = scene.SceneManager.Simulator.InputManager.GetButton( "Jump" );
				if( button.Down )
				{
					var command = frame.AddCommand<JumpCommand>();
					command.ComponentId = scene.LocalCharacter.Jumper.Id;
				}
			}
		}
	}
}
