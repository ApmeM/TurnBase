using System.Threading.Tasks;

public class ShootUnitAction : IUnitAction
{
    private readonly Unit cannon;
    private Task task;

    public ShootUnitAction(Unit cannon)
    {
        this.cannon = cannon;
    }

    public bool Process(float delta)
    {
        this.task = this.task ?? this.cannon.Shoot();
        return task.IsCompleted;
    }
}
