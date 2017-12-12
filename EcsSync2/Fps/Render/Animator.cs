using ProtoBuf;
using System;
using System.Collections.Generic;

namespace EcsSync2.Fps
{
	interface IAnimatorParamter<TValue>
	{
		string Name { get; set; }

		TValue Value { get; set; }
	}

	[ProtoContract]
	public struct AnimatorFloatParameter : IAnimatorParamter<float>
	{
		[ProtoMember( 1 )]
		public string Name { get; set; }

		[ProtoMember( 2 )]
		public float Value { get; set; }
	}

	[ProtoContract]
	public struct AnimatorIntParameter : IAnimatorParamter<int>
	{
		[ProtoMember( 1 )]
		public string Name { get; set; }

		[ProtoMember( 2 )]
		public int Value { get; set; }
	}

	[ProtoContract]
	public struct AnimatorBoolParameter : IAnimatorParamter<bool>
	{
		[ProtoMember( 1 )]
		public string Name { get; set; }

		[ProtoMember( 2 )]
		public bool Value { get; set; }
	}

	[ProtoContract]
	public class AnimatorSnapshot : ComponentSnapshot
	{
		[ProtoMember( 1 )]
		public uint Revision;

		[ProtoMember( 2 )]
		public string StateName;

		[ProtoMember( 3 )]
		public List<AnimatorBoolParameter> BoolParameters = new List<AnimatorBoolParameter>();

		[ProtoMember( 4 )]
		public List<AnimatorIntParameter> IntParameters = new List<AnimatorIntParameter>();

		[ProtoMember( 5 )]
		public List<AnimatorFloatParameter> FloatParameters = new List<AnimatorFloatParameter>();

		protected override void OnReset()
		{
			StateName = null;
			BoolParameters.Clear();
			IntParameters.Clear();
			FloatParameters.Clear();

			base.OnReset();
		}

		#region Parameter Helpers

		public bool GetBool(string name)
		{
			return GetValue<AnimatorBoolParameter, bool>( BoolParameters, name );
		}

		public void SetBool(string name, bool value)
		{
			SetValue<AnimatorBoolParameter, bool>( BoolParameters, name, value );
		}

		public int GetInt(string name)
		{
			return GetValue<AnimatorIntParameter, int>( IntParameters, name );
		}

		public void SetInt(string name, int value)
		{
			SetValue<AnimatorIntParameter, int>( IntParameters, name, value );
		}

		public float GetFloat(string name)
		{
			return GetValue<AnimatorFloatParameter, float>( FloatParameters, name );
		}

		public void SetFloat(string name, float value)
		{
			SetValue<AnimatorFloatParameter, float>( FloatParameters, name, value );
		}

		static TValue GetValue<TParameter, TValue>(IList<TParameter> parameters, string name)
			where TParameter : IAnimatorParamter<TValue>
			where TValue : struct
		{
			for( int i = 0; i < parameters.Count; i++ )
			{
				if( parameters[i].Name == name )
					return parameters[i].Value;
			}

			return default( TValue );
		}

		static void SetValue<TParameter, TValue>(IList<TParameter> parameters, string name, TValue value)
			where TParameter : IAnimatorParamter<TValue>, new()
			where TValue : struct
		{
			var parameter = new TParameter() { Name = name, Value = value };

			for( int i = 0; i < parameters.Count; i++ )
			{
				if( parameters[i].Name == parameter.Name )
				{
					parameters[i] = parameter;
					return;
				}
			}

			parameters.Add( parameter );
		}

		#endregion
	}

	[ProtoContract]
	public class AnimatorStateChangedEvent : ComponentEvent
	{
		[ProtoMember( 1 )]
		public string State;

		protected override void OnReset()
		{
			State = null;

			base.OnReset();
		}
	}

	public sealed class Animator : Renderer2
	{
		public interface IContext
		{
			void SetState(string name);

			void SetBool(string name, bool value);

			void SetInt(string name, int value);

			void SetFloat(string name, float value);
		}

		IContext m_context;
		uint m_lastRevision;

		protected internal override ComponentSnapshot CreateSnapshot()
		{
			return CreateSnapshot<AnimatorSnapshot>();
		}

		protected override void OnCommandReceived(ComponentCommand command)
		{
			throw new NotSupportedException( command.ToString() );
		}

		protected override ComponentSnapshot OnEventApplied(ComponentEvent @event)
		{
			switch( @event )
			{
				case AnimatorStateChangedEvent e:
					return OnAnimatorStateChangedEvent( e );

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}

		AnimatorSnapshot OnAnimatorStateChangedEvent(AnimatorStateChangedEvent e)
		{
			var s = (AnimatorSnapshot)State.Clone();
			s.Revision++;
			s.StateName = e.State;
			return s;
		}

		protected override void OnUpdate()
		{
			TryUpdateContext();
		}

		void TryUpdateContext(bool force = false)
		{
			if( force || m_lastRevision != TheState.Revision )
			{
				foreach( var p in TheState.BoolParameters )
					m_context.SetBool( p.Name, p.Value );

				foreach( var p in TheState.IntParameters )
					m_context.SetInt( p.Name, p.Value );

				foreach( var p in TheState.FloatParameters )
					m_context.SetFloat( p.Name, p.Value );

				m_context.SetState( TheState.StateName );

				m_lastRevision = TheState.Revision;
			}
		}

		public void ApplyAnimatorStateChangedEvent(string state)
		{
			if( string.IsNullOrEmpty( state ) )
				throw new ArgumentNullException( nameof( state ) );

			var e = CreateEvent<AnimatorStateChangedEvent>();
			e.State = state;

			ApplyEvent( e );
		}

		protected override void OnDestroy()
		{
		}

		protected override void OnFixedUpdate()
		{
		}

		protected override void OnSnapshotRecovered(ComponentSnapshot state)
		{
		}

		protected override void OnStart()
		{
		}

		AnimatorSnapshot TheState => (AnimatorSnapshot)State;

		public IContext Context
		{
			get { return m_context; }
			set
			{
				m_context = value;

				TryUpdateContext( true );
			}
		}
	}
}
