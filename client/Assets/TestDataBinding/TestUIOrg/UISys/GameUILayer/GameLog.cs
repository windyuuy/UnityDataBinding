
using lang.libs;

public class Logs
{
	public static Logger UILogger = new Logger().AppendTag("ui");
	public static Logger NetLogger = new Logger().AppendTag("network");
}
