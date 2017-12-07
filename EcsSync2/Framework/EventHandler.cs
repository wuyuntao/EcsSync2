using System;

namespace EcsSync2
{
	#region BaseEventHandler

	public abstract class BaseEventHandler : Disposable
	{
		EventDispatcher.EventHandler m_handler;
		Action<EventDispatcher.EventInvocation> m_onInvoke;

		internal BaseEventHandler(EventDispatcher dispatcher, Action<EventDispatcher.EventInvocation> onInvoke = null)
		{
			m_handler = new EventDispatcher.EventHandler( dispatcher );
			m_onInvoke = onInvoke;
		}

		protected override void DisposeManaged()
		{
			SafeDispose( ref m_handler );

			base.DisposeManaged();
		}

		internal void AddListener(EventDispatcher.EventListener listener)
		{
			m_handler.AddListener( listener );
		}

		internal void RemoveListener(Predicate<EventDispatcher.EventListener> predicate)
		{
			for( int i = 0; i < m_handler.Listeners.Count; i++ )
			{
				var listener = m_handler.Listeners[i];
				if( predicate( listener ) )
				{
					m_handler.RemoveListener( i );
					break;
				}
			}
		}

		internal void Invoke(EventDispatcher.EventArgs args)
		{
			var invocation = m_handler.Invoke( args );

			m_onInvoke?.Invoke( invocation );
		}

		internal TListener CreateListener<TListener>()
			where TListener : EventDispatcher.EventListener, new()
		{
			return m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<TListener>();
		}

		internal TArgs CreateArgs<TArgs>()
			where TArgs : EventDispatcher.EventArgs, new()
		{
			return m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<TArgs>();
		}
	}

	#endregion

	#region EventHandler

	public sealed class EventHandler : BaseEventHandler
	{
		internal EventHandler(EventDispatcher dispatcher, Action<EventDispatcher.EventInvocation> onInvoke = null)
			: base( dispatcher, onInvoke )
		{
		}

		public void AddHandler(Action handler)
		{
			var l = CreateListener<Listener>();
			l.Action = handler;
			AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action handler)
		{
			RemoveListener( l => l is Listener l0 && l0.Action == handler );
		}

		public void Invoke()
		{
			var args = CreateArgs<Args>();

			Invoke( args );
		}

		public static EventHandler operator +(EventHandler handler1, Action handler2)
		{
			handler1.AddHandler( handler2 );
			return handler1;
		}

		public static EventHandler operator -(EventHandler handler1, Action handler2)
		{
			handler1.RemoveHandler( handler2 );
			return handler1;
		}

		#region Args / Listeners

		class Args : EventDispatcher.EventArgs
		{
		}

		class Listener : EventDispatcher.EventListener
		{
			public Action Action;

			public override void Invoke(EventDispatcher.EventArgs args)
			{
				Action();
			}

			protected override void OnReset()
			{
				Action = null;

				base.OnReset();
			}
		}

		#endregion
	}

	#endregion

	#region EventHandler<T1>

	public sealed class EventHandler<T1> : BaseEventHandler
	{
		internal EventHandler(EventDispatcher dispatcher, Action<EventDispatcher.EventInvocation> onInvoke = null)
			: base( dispatcher, onInvoke )
		{
		}

		public void AddHandler(Action<T1> handler)
		{
			var l = CreateListener<Listener>();
			l.Action = handler;
			AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action<T1> handler)
		{
			RemoveListener( l => l is Listener l0 && l0.Action == handler );
		}

		public void Invoke(T1 arg1)
		{
			var args = CreateArgs<Args>();
			args.Arg1 = arg1;

			Invoke( args );
		}

		public static EventHandler<T1> operator +(EventHandler<T1> handler1, Action<T1> handler2)
		{
			handler1.AddHandler( handler2 );
			return handler1;
		}

		public static EventHandler<T1> operator -(EventHandler<T1> handler1, Action<T1> handler2)
		{
			handler1.RemoveHandler( handler2 );
			return handler1;
		}

		#region Args / Listeners

		class Args : EventDispatcher.EventArgs
		{
			public T1 Arg1;

			protected override void OnReset()
			{
				base.OnReset();

				Arg1 = default( T1 );
			}
		}

		class Listener : EventDispatcher.EventListener
		{
			public Action<T1> Action;

			public override void Invoke(EventDispatcher.EventArgs args)
			{
				var args0 = (Args)args;
				Action( args0.Arg1 );
			}

			protected override void OnReset()
			{
				base.OnReset();

				Action = null;
			}
		}

		#endregion
	}

	#endregion

	#region EventHandler<T1, T2>

	public sealed class EventHandler<T1, T2> : BaseEventHandler
	{
		internal EventHandler(EventDispatcher dispatcher, Action<EventDispatcher.EventInvocation> onInvoke = null)
			: base( dispatcher, onInvoke )
		{
		}

		public void AddHandler(Action<T1, T2> handler)
		{
			var l = CreateListener<Listener>();
			l.Action = handler;
			AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action<T1, T2> handler)
		{
			RemoveListener( l => l is Listener l0 && l0.Action == handler );
		}

		public void Invoke(T1 arg1, T2 arg2)
		{
			var args = CreateArgs<Args>();
			args.Arg1 = arg1;
			args.Arg2 = arg2;

			Invoke( args );
		}

		public static EventHandler<T1, T2> operator +(EventHandler<T1, T2> handler1, Action<T1, T2> handler2)
		{
			handler1.AddHandler( handler2 );
			return handler1;
		}

		public static EventHandler<T1, T2> operator -(EventHandler<T1, T2> handler1, Action<T1, T2> handler2)
		{
			handler1.RemoveHandler( handler2 );
			return handler1;
		}

		#region Args / Listener

		class Args : EventDispatcher.EventArgs
		{
			public T1 Arg1;
			public T2 Arg2;

			protected override void OnReset()
			{
				base.OnReset();

				Arg1 = default( T1 );
				Arg2 = default( T2 );
			}
		}

		class Listener : EventDispatcher.EventListener
		{
			public Action<T1, T2> Action;

			public override void Invoke(EventDispatcher.EventArgs args)
			{
				var args0 = (Args)args;
				Action( args0.Arg1, args0.Arg2 );
			}

			protected override void OnReset()
			{
				Action = null;

				base.OnReset();
			}
		}

		#endregion
	}

	#endregion

	#region EventHandler<T1, T2, T3>

	public sealed class EventHandler<T1, T2, T3> : BaseEventHandler
	{
		internal EventHandler(EventDispatcher dispatcher, Action<EventDispatcher.EventInvocation> onInvoke = null)
			: base( dispatcher, onInvoke )
		{
		}

		public void AddHandler(Action<T1, T2, T3> handler)
		{
			var l = CreateListener<Listener>();
			l.Action = handler;
			AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action<T1, T2, T3> handler)
		{
			RemoveListener( l => l is Listener l0 && l0.Action == handler );
		}

		public void Invoke(T1 arg1, T2 arg2, T3 arg3)
		{
			var args = CreateArgs<Args>();
			args.Arg1 = arg1;
			args.Arg2 = arg2;
			args.Arg3 = arg3;

			Invoke( args );
		}

		public static EventHandler<T1, T2, T3> operator +(EventHandler<T1, T2, T3> handler1, Action<T1, T2, T3> handler2)
		{
			handler1.AddHandler( handler2 );
			return handler1;
		}

		public static EventHandler<T1, T2, T3> operator -(EventHandler<T1, T2, T3> handler1, Action<T1, T2, T3> handler2)
		{
			handler1.RemoveHandler( handler2 );
			return handler1;
		}

		#region Args / Listener

		class Args : EventDispatcher.EventArgs
		{
			public T1 Arg1;
			public T2 Arg2;
			public T3 Arg3;

			protected override void OnReset()
			{
				base.OnReset();

				Arg1 = default( T1 );
				Arg2 = default( T2 );
				Arg3 = default( T3 );
			}
		}

		class Listener : EventDispatcher.EventListener
		{
			public Action<T1, T2, T3> Action;

			public override void Invoke(EventDispatcher.EventArgs args)
			{
				var args0 = (Args)args;
				Action( args0.Arg1, args0.Arg2, args0.Arg3 );
			}

			protected override void OnReset()
			{
				Action = null;

				base.OnReset();
			}
		}

		#endregion
	}

	#endregion

	#region EventHandler<T1, T2, T3, T4>

	public sealed class EventHandler<T1, T2, T3, T4> : BaseEventHandler
	{
		internal EventHandler(EventDispatcher dispatcher, Action<EventDispatcher.EventInvocation> onInvoke = null)
			: base( dispatcher, onInvoke )
		{
		}

		public void AddHandler(Action<T1, T2, T3, T4> handler)
		{
			var l = CreateListener<Listener>();
			l.Action = handler;
			AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action<T1, T2, T3, T4> handler)
		{
			RemoveListener( l => l is Listener l0 && l0.Action == handler );
		}

		public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var args = CreateArgs<Args>();
			args.Arg1 = arg1;
			args.Arg2 = arg2;
			args.Arg3 = arg3;
			args.Arg4 = arg4;

			Invoke( args );
		}

		public static EventHandler<T1, T2, T3, T4> operator +(EventHandler<T1, T2, T3, T4> handler1, Action<T1, T2, T3, T4> handler2)
		{
			handler1.AddHandler( handler2 );
			return handler1;
		}

		public static EventHandler<T1, T2, T3, T4> operator -(EventHandler<T1, T2, T3, T4> handler1, Action<T1, T2, T3, T4> handler2)
		{
			handler1.RemoveHandler( handler2 );
			return handler1;
		}

		#region Args / Listener

		class Args : EventDispatcher.EventArgs
		{
			public T1 Arg1;
			public T2 Arg2;
			public T3 Arg3;
			public T4 Arg4;

			protected override void OnReset()
			{
				base.OnReset();

				Arg1 = default( T1 );
				Arg2 = default( T2 );
				Arg3 = default( T3 );
				Arg4 = default( T4 );
			}
		}

		class Listener : EventDispatcher.EventListener
		{
			public Action<T1, T2, T3, T4> Action;

			public override void Invoke(EventDispatcher.EventArgs args)
			{
				var args0 = (Args)args;
				Action( args0.Arg1, args0.Arg2, args0.Arg3, args0.Arg4 );
			}

			protected override void OnReset()
			{
				Action = null;

				base.OnReset();
			}
		}

		#endregion
	}

	#endregion
}
