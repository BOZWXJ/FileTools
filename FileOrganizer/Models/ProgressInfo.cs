namespace FileOrganizer.Models
{
	internal class ProgressInfo
	{
		public int Value { get; private set; }
		public int ValueMax { get; private set; }
		public string Message { get; private set; }
		public ProgressInfo(int value, int valueMax, string message)
		{
			Value = value;
			ValueMax = valueMax;
			Message = message;
		}
	}
}
