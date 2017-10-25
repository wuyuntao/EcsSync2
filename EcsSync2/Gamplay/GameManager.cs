namespace EcsSync2
{
	public class GameManagerSettings : EntitySettings
	{
	}

	public class GameManager : Entity
	{
		public ProcessController ProcessController { get; private set; }

		internal override void OnInitialize(SceneManager scene, InstanceId id)
		{
			base.OnInitialize( scene, id );

			ProcessController = AddComponent<ProcessController>();
		}
	}
}
