using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Win;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Win.Utils;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using Microsoft.EntityFrameworkCore;
using DevExpress.ExpressApp.EFCore;
using DevExpress.EntityFrameworkCore.Security;
using DXComnTransp.Module;
using DXComnTransp.Module.BusinessObjects;
using System.Data.Common;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;

namespace DXComnTransp.Win;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Win.WinApplication._members
public class DXComnTranspWindowsFormsApplication : WinApplication {
    public DXComnTranspWindowsFormsApplication() {
		SplashScreen = new DXSplashScreen(typeof(XafSplashScreen), new DefaultOverlayFormOptions());
        ApplicationName = "DXComnTransp";
        CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
        UseOldTemplates = false;
        DatabaseVersionMismatch += DXComnTranspWindowsFormsApplication_DatabaseVersionMismatch;
        CustomizeLanguagesList += DXComnTranspWindowsFormsApplication_CustomizeLanguagesList;
    }
    private void DXComnTranspWindowsFormsApplication_CustomizeLanguagesList(object sender, CustomizeLanguagesListEventArgs e) {
        string userLanguageName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
        if(userLanguageName != "en-US" && e.Languages.IndexOf(userLanguageName) == -1) {
            e.Languages.Add(userLanguageName);
        }
    }
    private void DXComnTranspWindowsFormsApplication_DatabaseVersionMismatch(object sender, DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
        e.Updater.Update();
        e.Handled = true;
#else
        string message = "The application cannot connect to the specified database, " +
            "because the database doesn't exist, its version is older " +
            "than that of the application or its schema does not match " +
            "the ORM data model structure. To update the database explicitly, run the application with the " +
            "'--updateDatabase' argument. See also https://www.devexpress.com/kb=T367835.";

        if(e.CompatibilityError != null && e.CompatibilityError.Exception != null) {
            message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
        }
        throw new InvalidOperationException(message);
#endif
    }
}
