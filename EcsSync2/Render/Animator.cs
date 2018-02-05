using ProtoBuf;
using System;
using System.Collections.Generic;

namespace EcsSync2
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
	[ProtoSubType( typeof( ComponentSnapshot ), 8 )]
	public class AnimatorSnapshot : ComponentSnapshot
	{
		[ProtoMember( 1 )]
		public uint Revision;

		[ProtoMember( 2 )]
		public string StateName;

		[ProtoMember( 3, IsRequired = false )]
		public uint? StateContext;

		[ProtoMember( 4 )]
		public List<AnimatorBoolParameter> BoolParameters = new List<AnimatorBoolParameter>();

		[ProtoMember( 5 )]
		public List<AnimatorIntParameter> IntParameters = new List<AnimatorIntParameter>();

		[ProtoMember( 6 )]
		public List<AnimatorFloatParameter> FloatParameters = new List<AnimatorFloatParameter>();

		public override ComponentSnapshot Clone()
		{
			var s = this.Allocate<AnimatorSnapshot>();
			s.ComponentId = ComponentId;
			s.Revision = Revision;
			s.StateName = StateName;
			s.StateContext = StateContext;
			s.BoolParameters.AddRange( BoolParameters );
			s.IntParameters.AddRange( IntParameters );
			s.FloatParameters.AddRange( FloatParameters );
			return s;
		}

		protected internal override bool IsApproximate(ComponentSnapshot other)
		{
			if( !( other is AnimatorSnapshot s ) )
				return false;

			return
				IsApproximate( ComponentId, s.ComponentId ) &&
				StateName == s.StateName &&
				StateContext == s.StateContext &&
				IsApproximate<AnimatorBoolParameter, bool>( BoolParameters, s.BoolParameters, (b1, b2) => b1 == b2 ) &&
				IsApproximate<AnimatorIntParameter, int>( IntParameters, s.IntParameters, (v1, v2) => IsApproximate( v1, v2 ) ) &&
				IsApproximate<AnimatorFloatParameter, float>( FloatParameters, s.FloatParameters, (v1, v2) => IsApproximate( v1, v2 ) );
		}

		protected override void OnReset()
		{
			ComponentId = 0;
			Revision = 0;
			StateName = null;
			StateContext = null;
			BoolParameters.Clear();
			IntParameters.Clear();
			FloatParameters.Clear();
		}

		#region Parameter Helpers

		bool IsApproximate<TParameter, TValue>(List<TParameter> list1, List<TParameter> list2, Func<TValue, TValue, bool> comparer)
			where TParameter : IAnimatorParamter<TValue>
			where TValue : struct
		{
			if( list1.Count != list2.Count )
				return false;

			for( int i = 0; i < list1.Count; i++ )
			{
				if( list1[i].Name != list2[i].Name || !( comparer( list1[i].Value, list2[i].Value ) ) )
					return false;
			}

			return true;
		}

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
	[ProtoSubType( typeof( ComponentEvent ), 7 )]
	public class AnimatorStateChangedEvent : ComponentEvent
	{
		[ProtoMember( 1 )]
		public string State;

		[ProtoMember( 2, IsRequired = false )]
		public uint? StateContext;

		protected override void OnReset()
		{
			ComponentId = 0;
			State = null;
			StateContext = null;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentEvent ), 8 )]
	public class AnimatorBoolParameterChangedEvent : ComponentEvent
	{
		[ProtoMember( 1 )]
		public string Name { get; set; }

		[ProtoMember( 2 )]
		public bool Value { get; set; }

		protected override void OnReset()
		{
			ComponentId = 0;
			Name = null;
			Value = false;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentEvent ), 9 )]
	public class AnimatorIntParameterChangedEvent : ComponentEvent
	{
		[ProtoMember( 1 )]
		public string Name { get; set; }

		[ProtoMember( 2 )]
		public int Value { get; set; }

		protected override void OnReset()
		{
			ComponentId = 0;
			Name = null;
			Value = 0;
		}
	}

	[ProtoContract]
	[ProtoSubType( typeof( ComponentEvent ), 10 )]
	public class AnimatorFloatParameterChangedEvent : ComponentEvent
	{
		[ProtoMember( 1 )]
		public string Name { get; set; }

		[ProtoMember( 2 )]
		public float Value { get; set; }

		protected override void OnReset()
		{
			ComponentId = 0;
			Name = null;
			Value = 0;
		}
	}

	public class Animator : Renderer
	{
		public interface IContext
		{
			void SetState(string name, uint? context);

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

				case AnimatorBoolParameterChangedEvent e:
					return OnAnimatorBoolParameterChangedEvent( e );

				case AnimatorIntParameterChangedEvent e:
					return OnAnimatorIntParameterChangedEvent( e );

				case AnimatorFloatParameterChangedEvent e:
					return OnAnimatorFloatParameterChangedEvent( e );

				default:
					throw new NotSupportedException( @event.ToString() );
			}
		}

		AnimatorSnapshot OnAnimatorStateChangedEvent(AnimatorStateChangedEvent e)
		{
			var s = (AnimatorSnapshot)State.Clone();
			s.Revision++;
			s.StateName = e.State;
			s.StateContext = e.StateContext;
			return s;
		}

		AnimatorSnapshot OnAnimatorBoolParameterChangedEvent(AnimatorBoolParameterChangedEvent e)
		{
			var s = (AnimatorSnapshot)State.Clone();
			s.Revision++;
			s.SetBool( e.Name, e.Value );
			return s;
		}

		AnimatorSnapshot OnAnimatorIntParameterChangedEvent(AnimatorIntParameterChangedEvent e)
		{
			var s = (AnimatorSnapshot)State.Clone();
			s.Revision++;
			s.SetInt( e.Name, e.Value );
			return s;
		}

		AnimatorSnapshot OnAnimatorFloatParameterChangedEvent(AnimatorFloatParameterChangedEvent e)
		{
			var s = (AnimatorSnapshot)State.Clone();
			s.Revision++;
			s.SetFloat( e.Name, e.Value );
			return s;
		}

		protected override void OnUpdate()
		{
			TryUpdateContext();
		}

		void TryUpdateContext(bool force = false)
		{
			if( m_context != null && ( force || m_lastRevision != TheState.Revision ) )
			{
				foreach( var p in TheState.BoolParameters )
					m_context.SetBool( p.Name, p.Value );

				foreach( var p in TheState.IntParameters )
					m_context.SetInt( p.Name, p.Value );

				foreach( var p in TheState.FloatParameters )
					m_context.SetFloat( p.Name, p.Value );

				m_context.SetState( TheState.StateName, TheState.StateContext );

				m_lastRevision = TheState.Revision;
			}
		}

		public void ApplyAnimatorStateChangedEvent(string state, uint? context = null)
		{
			if( string.IsNullOrEmpty( state ) )
				throw new ArgumentNullException( nameof( state ) );

			var e = CreateEvent<AnimatorStateChangedEvent>();
			e.State = state;
			e.StateContext = context;

			ApplyEvent( e );
		}

		public void ApplyAnimatorBoolParameterChangedEvent(string name, bool value)
		{
			if( string.IsNullOrEmpty( name ) )
				throw new ArgumentNullException( nameof( name ) );

			var e = CreateEvent<AnimatorBoolParameterChangedEvent>();
			e.Name = name;
			e.Value = value;

			ApplyEvent( e );
		}

		public void ApplyAnimatorIntParameterChangedEvent(string name, int value)
		{
			if( string.IsNullOrEmpty( name ) )
				throw new ArgumentNullException( nameof( name ) );

			var e = CreateEvent<AnimatorIntParameterChangedEvent>();
			e.Name = name;
			e.Value = value;

			ApplyEvent( e );
		}

		public void ApplyAnimatorFloatParameterChangedEvent(string name, float value)
		{
			if( string.IsNullOrEmpty( name ) )
				throw new ArgumentNullException( nameof( name ) );

			var e = CreateEvent<AnimatorFloatParameterChangedEvent>();
			e.Name = name;
			e.Value = value;

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

		protected AnimatorSnapshot TheState => (AnimatorSnapshot)State;

		public IContext Context
		{
			get { return m_context; }
			set
			{
				m_context = value;

				if( m_context != null )
					TryUpdateContext( true );
				else
					m_lastRevision = 0;
			}
		}
	}
}
