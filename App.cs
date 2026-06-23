using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace RevitVisibilityDiagnostic_R24
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "MTC Tools";
            string panelName = "Diagnostics";

            // Create a custom ribbon tab
            try
            {
                application.CreateRibbonTab(tabName);
            }
            catch (Exception)
            {
                // Tab might already exist if other plugins use the same tab name
            }

            // Create a ribbon panel
            RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            // Get dll assembly path
            string assemblyPath = Assembly.GetExecutingAssembly().Location;

            // Create push button for the command
            PushButtonData buttonData = new PushButtonData(
                "cmdVisibilityDiagnostic",
                "Visibility\nDiagnostic",
                assemblyPath,
                "RevitVisibilityDiagnostic_R24.Command");

            buttonData.ToolTip = "Diagnoses why an element is not visible in the current view.";

            // Add button to the panel
            PushButton button = panel.AddItem(buttonData) as PushButton;

            // (Optional) Set an icon for the button if you have one.
            // button.LargeImage = new BitmapImage(new Uri("pack://application:,,,/VisibilityDiagnostic;component/Resources/icon.png"));

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }
}
