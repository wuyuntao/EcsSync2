﻿using EcsSync2.Fps;
using MessagePack;

namespace EcsSync2
{
	[Union( 0, typeof( CreateEntityCommand ) )]
	[Union( 1, typeof( RemoveEntityCommand ) )]
	[Union( 2, typeof( PlayerConnectCommand ) )]
	[Union( 3, typeof( MoveCharacterCommand ) )]
	public interface ICommand : IReferencable
	{
	}

	public abstract class SceneCommand : Referencable, ICommand
	{
	}

	public abstract class ComponentCommand : Referencable, ICommand
	{
		[Key( 0 )]
		public uint ComponentId;
	}
}
