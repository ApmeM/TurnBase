using Godot;

public class TimerLabel : Label
{
    public override void _Ready()
    {
        base._Ready();

        this.Text = "";
        this.GetNode<Timer>("MessageTimer").OneShot = true;
        this.GetNode<Timer>("MessageTimer").Connect("timeout", this, nameof(OnMessageTimerTimeout));
    }

    public void ShowMessage(string text, float timeout)
    {
        this.Text = text;
        this.GetNode<Timer>("MessageTimer").WaitTime = timeout;
        this.GetNode<Timer>("MessageTimer").Start();
    }

    private void OnMessageTimerTimeout()
    {
        this.Text = "";
    }
}
