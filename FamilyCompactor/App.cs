#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Media.Imaging;

#endregion

namespace FamilyCompactor
{
    internal class App : IExternalApplication
    {
        static AddInId addInId = new AddInId(new Guid("0413184f-3308-44d8-ab63-9fa2efae7594"));
        string helpURL = "https://google.com";
        ResourceManager rm = new ResourceManager($"{nameof(FamilyCompactor)}.Resources.strings", typeof(App).Assembly);
        CultureInfo ci;

        public RibbonPanel AutomationPanel(UIControlledApplication application, string tabName = "", RibbonPanel ribbonPanel = null)
        {
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            if (ribbonPanel == null)
            {
                if (string.IsNullOrEmpty(tabName))
                    ribbonPanel = application.CreateRibbonPanel("FamilyCompactor");
                else
                    ribbonPanel = application.CreateRibbonPanel(tabName, "FamilyCompactor");
            }
            AddButton(ribbonPanel,
                    "Compact families",
                    assemblyPath,
                    GetType().Namespace + "." + nameof(Command),
                    rm.GetString("CommandToolTip", ci) + 
                    $"\nv{Assembly.GetExecutingAssembly().GetName().Version}",
                    rm.GetString("CommandLongDescription", ci));
            return ribbonPanel;
        }

        private void AddButton(RibbonPanel ribbonPanel, string buttonName, string path, string linkToCommand, string toolTip, string longDescription)
        {
            PushButtonData buttonData = new PushButtonData(
               buttonName,
               buttonName,
               path,
               linkToCommand);
            ContextualHelp contextualHelp = new ContextualHelp(ContextualHelpType.Url, helpURL);
            buttonData.SetContextualHelp(contextualHelp);
            PushButton button = ribbonPanel.AddItem(buttonData) as PushButton;
            button.ToolTip = toolTip;
            button.LongDescription = longDescription;
            button.AvailabilityClassName = typeof(TrueAvailability).Namespace + "." + nameof(TrueAvailability);
            button.LargeImage = new BitmapImage(new Uri($@"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Icon32.png", UriKind.RelativeOrAbsolute));
            button.Image = new BitmapImage(new Uri($@"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Icon16.png", UriKind.RelativeOrAbsolute));
        }

        public Result OnStartup(UIControlledApplication a)
        {
            string cultureName = "en";
            if (a.ControlledApplication.Language == LanguageType.English_USA ||
                a.ControlledApplication.Language == LanguageType.English_GB ||
                a.ControlledApplication.Language == LanguageType.Russian)
                cultureName = a.ControlledApplication.Language.ToString().Substring(0, 2);
            ci = CultureInfo.CreateSpecificCulture(cultureName);

            RibbonPanel ribbonPanel = AutomationPanel(a);

            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }

    public class TrueAvailability : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication a, CategorySet b) => true;
    }

}
