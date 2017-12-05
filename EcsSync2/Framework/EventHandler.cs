using System;

namespace EcsSync2
{
	#region EventHandler

	public sealed class EventHandler : Disposable
	{
		EventDispatcher.EventHandler m_handler;

		internal EventHandler(EventDispatcher dispatcher)
		{
			m_handler = new EventDispatcher.EventHandler( dispatcher );
		}

		protected override void DisposeManaged()
		{
			SafeDispose( ref m_handler );

			base.DisposeManaged();
		}

		public void AddHandler(Action handler)
		{
			var l = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Listener>();
			l.Action = handler;
			m_handler.AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action handler)
		{
			for( int i = 0; i < m_handler.Listeners.Count; i++ )
			{
				var l = (Listener)m_handler.Listeners[i];
				if( l.Action == handler )
				{
					m_handler.RemoveListener( i );
					break;
				}
			}
		}

		public void Invoke()
		{
			var args = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Args>();

			m_handler.Invoke( args );
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

	public sealed class EventHandler<T1> : Disposable
	{
		EventDispatcher.EventHandler m_handler;

		internal EventHandler(EventDispatcher dispatcher)
		{
			m_handler = new EventDispatcher.EventHandler( dispatcher );
		}

		protected override void DisposeManaged()
		{
			SafeDispose( ref m_handler );

			base.DisposeManaged();
		}

		public void AddHandler(Action<T1> handler)
		{
			var l = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Listener>();
			l.Action = handler;
			m_handler.AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action<T1> handler)
		{
			for( int i = 0; i < m_handler.Listeners.Count; i++ )
			{
				var l = (Listener)m_handler.Listeners[i];
				if( l.Action == handler )
				{
					m_handler.RemoveListener( i );
					break;
				}
			}
		}

		public void Invoke(T1 arg1)
		{
			var args = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Args>();
			args.Arg1 = arg1;

			m_handler.Invoke( args );
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

	public sealed class EventHandler<T1, T2> : Disposable
	{
		EventDispatcher.EventHandler m_handler;

		internal EventHandler(EventDispatcher dispatcher)
		{
			m_handler = new EventDispatcher.EventHandler( dispatcher );
		}

		protected override void DisposeManaged()
		{
			SafeDispose( ref m_handler );

			base.DisposeManaged();
		}

		public void AddHandler(Action<T1, T2> handler)
		{
			var l = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Listener>();
			l.Action = handler;
			m_handler.AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action<T1, T2> handler)
		{
			for( int i = 0; i < m_handler.Listeners.Count; i++ )
			{
				var l = (Listener)m_handler.Listeners[i];
				if( l.Action == handler )
				{
					m_handler.RemoveListener( i );
					break;
				}
			}
		}

		public void Invoke(T1 arg1, T2 arg2)
		{
			var args = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Args>();
			args.Arg1 = arg1;
			args.Arg2 = arg2;

			m_handler.Invoke( args );
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

	public sealed class EventHandler<T1, T2, T3> : Disposable
	{
		EventDispatcher.EventHandler m_handler;

		internal EventHandler(EventDispatcher dispatcher)
		{
			m_handler = new EventDispatcher.EventHandler( dispatcher );
		}

		protected override void DisposeManaged()
		{
			SafeDispose( ref m_handler );

			base.DisposeManaged();
		}

		public void AddHandler(Action<T1, T2, T3> handler)
		{
			var l = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Listener>();
			l.Action = handler;
			m_handler.AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action<T1, T2, T3> handler)
		{
			for( int i = 0; i < m_handler.Listeners.Count; i++ )
			{
				var l = (Listener)m_handler.Listeners[i];
				if( l.Action == handler )
				{
					m_handler.RemoveListener( i );
					break;
				}
			}
		}

		public void Invoke(T1 arg1, T2 arg2, T3 arg3)
		{
			var args = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Args>();
			args.Arg1 = arg1;
			args.Arg2 = arg2;
			args.Arg3 = arg3;

			m_handler.Invoke( args );
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

	public sealed class EventHandler<T1, T2, T3, T4> : Disposable
	{
		EventDispatcher.EventHandler m_handler;

		internal EventHandler(EventDispatcher dispatcher)
		{
			m_handler = new EventDispatcher.EventHandler( dispatcher );
		}

		protected override void DisposeManaged()
		{
			SafeDispose( ref m_handler );

			base.DisposeManaged();
		}

		public void AddHandler(Action<T1, T2, T3, T4> handler)
		{
			var l = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Listener>();
			l.Action = handler;
			m_handler.AddListener( l );
			l.Release();
		}

		public void RemoveHandler(Action<T1, T2, T3, T4> handler)
		{
			for( int i = 0; i < m_handler.Listeners.Count; i++ )
			{
				var l = (Listener)m_handler.Listeners[i];
				if( l.Action == handler )
				{
					m_handler.RemoveListener( i );
					break;
				}
			}
		}

		public void Invoke(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			var args = m_handler.Dispatcher.Simulator.ReferencableAllocator.Allocate<Args>();
			args.Arg1 = arg1;
			args.Arg2 = arg2;
			args.Arg3 = arg3;
			args.Arg4 = arg4;

			m_handler.Invoke( args );
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
