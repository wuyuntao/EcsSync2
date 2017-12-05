﻿using System;
using System.Collections.Generic;

namespace EcsSync2
{
	public class EventDispatcher : SimulatorComponent
	{
		List<EventHandler> m_dirtyHandlers = new List<EventHandler>();
		List<EventInvocation> m_invocations = new List<EventInvocation>();
		//SortedList<uint, EventInvocationFrame> m_syncInvocationFrames = new SortedList<uint, EventInvocationFrame>();
		//SortedList<uint, EventInvocationFrame> m_reconcilationInvocationFrames = new SortedList<uint, EventInvocationFrame>();
		//SortedList<uint, EventInvocationFrame> m_predictionInvocationFrames = new SortedList<uint, EventInvocationFrame>();

		public EventDispatcher(Simulator simulator)
			: base( simulator )
		{
		}

		#region EventHandler

		internal void AddDirtyEventHandler(EventHandler handler)
		{
			m_dirtyHandlers.Add( handler );
		}

		internal void AddEventInvocation(EventInvocation invocation)
		{
			invocation.Retain();
			m_invocations.Add( invocation );

			//var context = Simulator.TickScheduler.CurrentContext.Value;
			//switch( context.Type )
			//{
			//	case TickScheduler.TickContextType.Sync:
			//		AddEventInvocation( m_syncInvocationFrames, context, invocation );
			//		break;

			//	case TickScheduler.TickContextType.Reconcilation:
			//		AddEventInvocation( m_reconcilationInvocationFrames, context, invocation );
			//		break;

			//	case TickScheduler.TickContextType.Prediction:
			//		AddEventInvocation( m_predictionInvocationFrames, context, invocation );
			//		break;

			//	default:
			//		throw new NotSupportedException( context.Type.ToString() );
			//}
		}

		//void AddEventInvocation(SortedList<uint, EventInvocationFrame> frames, TickScheduler.TickContext context, EventInvocation invocation)
		//{
		//	if( !frames.TryGetValue( context.Time, out EventInvocationFrame frame ) )
		//	{
		//		frame = Simulator.ReferencableAllocator.Allocate<EventInvocationFrame>();
		//		frame.Context = context;
		//		frames.Add( context.Time, frame );
		//	}

		//	invocation.Retain();
		//	frame.Invocations.Add( invocation );
		//}

		internal void Dispatch()
		{
			for( int i = 0; i < m_invocations.Count; i++ )
				m_invocations[i].Invoke();
			m_invocations.Clear();

			for( int i = 0; i < m_dirtyHandlers.Count; i++ )
				m_dirtyHandlers[i].ApplyChanges();
			m_dirtyHandlers.Clear();
		}

		#endregion

		#region EventArgs

		internal abstract class EventArgs : Referencable
		{
		}

		#endregion

		#region EventListener

		internal abstract class EventListener : Referencable
		{
			public abstract void Invoke(EventArgs args);
		}

		#endregion

		#region EventInvocation

		internal class EventInvocation : Referencable
		{
			public EventListener Listener;

			public EventArgs Args;

			public void Invoke()
			{
				Listener.Invoke( Args );
			}

			protected override void OnReset()
			{
				base.OnReset();

				Listener.Release();
				Listener = null;

				Args.Release();
				Args = null;
			}
		}

		#endregion

		#region EventInvocationFrame

		//internal class EventInvocationFrame : Referencable
		//{
		//	public TickScheduler.TickContext Context;
		//	public List<EventInvocation> Invocations = new List<EventInvocation>();

		//	protected override void OnReset()
		//	{
		//		Context = default( TickScheduler.TickContext );

		//		foreach( var invocation in Invocations )
		//			invocation.Release();
		//		Invocations.Clear();

		//		base.OnReset();
		//	}
		//}

		#endregion

		#region EventHandler

		internal class EventHandler : Disposable
		{
			EventDispatcher m_dispatcher;
			List<EventListener> m_listeners = new List<EventListener>();
			bool m_isDirty;

			public EventHandler(EventDispatcher dispatcher)
			{
				m_dispatcher = dispatcher;
			}

			protected override void DisposeManaged()
			{
				foreach( var l in m_listeners )
					l.Release();
				m_listeners.Clear();

				base.DisposeManaged();
			}

			public void AddListener(EventListener listener)
			{
				listener.Retain();
				m_listeners.Add( listener );

				IsDirty = true;
			}

			public void RemoveListener(int index)
			{
				m_listeners[index].Release();
				m_listeners[index] = null;

				IsDirty = true;
			}

			public void ApplyChanges()
			{
				m_listeners.RemoveAll( l => l == null );

				IsDirty = false;
			}

			public void Invoke(EventArgs args)
			{
				for( int i = 0; i < m_listeners.Count; i++ )
				{
					var listener = m_listeners[i];
					if( listener != null )
					{
						var invocation = m_dispatcher.Simulator.ReferencableAllocator.Allocate<EventInvocation>();
						invocation.Listener = listener;
						invocation.Args = args;

						m_dispatcher.AddEventInvocation( invocation );
					}
				}
			}

			public EventDispatcher Dispatcher => m_dispatcher;

			public IList<EventListener> Listeners => m_listeners;

			bool IsDirty
			{
				set
				{
					if( value && !m_isDirty )
						m_dispatcher.AddDirtyEventHandler( this );

					m_isDirty = value;
				}
			}
		}

		#endregion
	}
}
