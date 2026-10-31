using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Blazor;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using DevExpress.ExpressApp.SystemModule;
using DXComnTransp.Module.BusinessObjects;
using Microsoft.EntityFrameworkCore;
using DevExpress.ExpressApp.EFCore;
using DevExpress.EntityFrameworkCore.Security;

namespace DXComnTransp.Blazor.Server;

public class DXComnTranspBlazorApplication : BlazorApplication {
    public DXComnTranspBlazorApplication() {
        ApplicationName = "DXComnTransp";
        CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
        DatabaseVersionMismatch += DXComnTranspBlazorApplication_DatabaseVersionMismatch;
    }
    private void DXComnTranspBlazorApplication_DatabaseVersionMismatch(object sender, DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
        e.Updater.Update();
        e.Handled = true;
#else
        string message = "The application cannot connect to the specified database, " +
            "because the database doesn't exist, its version is older " +
            "than that of the application or its schema does not match " +
            "the ORM data model structure. To update the database explicitly, run the application with the " +
            "'--updateDatabase' argument (for example: dotnet run -- --updateDatabase --forceUpdate). " +
            "See also https://www.devexpress.com/kb=T367835.";

        if(e.CompatibilityError != null && e.CompatibilityError.Exception != null) {
            message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
        }
        throw new InvalidOperationException(message);
#endif
    }
}
