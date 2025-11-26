namespace Server;

[AttributeUsage(AttributeTargets.Class)]
public class CommandAttribute(GameCommand command) : Attribute
{
    public GameCommand Command { get; } = command;
}