using System.Windows.Forms;

namespace ClipGuard;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var context = new ClipGuardApplicationContext();

        Application.Run(context);
    }
}