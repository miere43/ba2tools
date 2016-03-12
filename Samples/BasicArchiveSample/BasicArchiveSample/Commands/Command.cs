namespace BasicArchiveSample.Commands
{
    public class Command
    {
        public readonly CommandType Type;

        public static Command None { get; private set; } = new Command(CommandType.None);

        public static Command Invalid { get; private set; } = new Command(CommandType.Invalid);

        public Command(CommandType type)
        {
            Type = type;
        }
    }
}
