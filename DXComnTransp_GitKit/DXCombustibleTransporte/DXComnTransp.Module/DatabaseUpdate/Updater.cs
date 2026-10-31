using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Base.Security;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.EF;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DXComnTransp.Module.BusinessObjects;
using DevExpress.Persistent.BaseImpl.EFCore.AuditTrail;

namespace DXComnTransp.Module.DatabaseUpdate;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Updating.ModuleUpdater
public class Updater : ModuleUpdater {
    public Updater(IObjectSpace objectSpace, Version currentDBVersion) :
        base(objectSpace, currentDBVersion) {
    }
    public override void UpdateDatabaseAfterUpdateSchema() {
        base.UpdateDatabaseAfterUpdateSchema();

        CreateDefaultRole();
        AsegurarAdministradorInicial();
        DesactivarUsuariosConPasswordVacia();

        AsegurarTipoDocumentoVehiculo("Inspección técnica", true);
        AsegurarTipoDocumentoVehiculo("Licencia operativa", true);
        AsegurarTipoDocumentoVehiculo("Contrato", true);
        AsegurarTipoDocumentoVehiculo("Foto del vehículo", false);
        ObjectSpace.CommitChanges();
    }

    private void AsegurarAdministradorInicial() {
        string initialPassword = Environment.GetEnvironmentVariable("DXCOMNTRANSP_INITIAL_ADMIN_PASSWORD");
        if(string.IsNullOrWhiteSpace(initialPassword)) {
            return;
        }
        if(initialPassword.Length < 12) {
            throw new InvalidOperationException(
                "La variable de entorno 'DXCOMNTRANSP_INITIAL_ADMIN_PASSWORD' debe tener al menos 12 caracteres.");
        }

        ApplicationUser userAdmin = ObjectSpace.FirstOrDefault<ApplicationUser>(u => u.UserName == "Admin");
        if(userAdmin == null) {
            userAdmin = ObjectSpace.CreateObject<ApplicationUser>();
            userAdmin.UserName = "Admin";
            userAdmin.SetPassword(initialPassword);
            userAdmin.ChangePasswordOnFirstLogon = true;
            ObjectSpace.CommitChanges();
            ((ISecurityUserWithLoginInfo)userAdmin).CreateUserLoginInfo(
                SecurityDefaults.PasswordAuthentication, ObjectSpace.GetKeyValueAsString(userAdmin));
        }
        else if(TienePasswordVacia(userAdmin)) {
            userAdmin.SetPassword(initialPassword);
            userAdmin.ChangePasswordOnFirstLogon = true;
        }
        userAdmin.IsActive = true;

        PermissionPolicyRole adminRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(r => r.Name == "Administrators");
        if(adminRole == null) {
            adminRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            adminRole.Name = "Administrators";
        }
        adminRole.IsAdministrative = true;
        if(!userAdmin.Roles.Contains(adminRole)) {
            userAdmin.Roles.Add(adminRole);
        }
        ObjectSpace.CommitChanges();
    }

    private void DesactivarUsuariosConPasswordVacia() {
        foreach(ApplicationUser user in ObjectSpace.GetObjects<ApplicationUser>()) {
            if(user.IsActive && TienePasswordVacia(user)) {
                user.IsActive = false;
            }
        }
    }

    private static bool TienePasswordVacia(ApplicationUser user) {
        return string.IsNullOrEmpty(user.StoredPassword)
            || ((IAuthenticationStandardUser)user).ComparePassword("");
    }

    private void AsegurarTipoDocumentoVehiculo(string nombre, bool requiereVencimiento)
    {
        TipoDocumentoVehiculo tipo = ObjectSpace.FirstOrDefault<TipoDocumentoVehiculo>(
            t => t.Nombre == nombre);
        if (tipo == null)
        {
            tipo = ObjectSpace.CreateObject<TipoDocumentoVehiculo>();
            tipo.Nombre = nombre;
            tipo.RequiereVencimiento = requiereVencimiento;
            tipo.Activo = true;
        }
    }
    public override void UpdateDatabaseBeforeUpdateSchema() {
        base.UpdateDatabaseBeforeUpdateSchema();
    }
    private PermissionPolicyRole CreateDefaultRole() {
        PermissionPolicyRole defaultRole = ObjectSpace.FirstOrDefault<PermissionPolicyRole>(role => role.Name == "Default");
        if(defaultRole == null) {
            defaultRole = ObjectSpace.CreateObject<PermissionPolicyRole>();
            defaultRole.Name = "Default";

            defaultRole.AddObjectPermissionFromLambda<ApplicationUser>(SecurityOperations.Read, cm => cm.ID == (int)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddNavigationPermission(@"Application/NavigationItems/Items/Default/Items/MyDetails", SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "ChangePasswordOnFirstLogon", cm => cm.ID == (int)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddMemberPermissionFromLambda<ApplicationUser>(SecurityOperations.Write, "StoredPassword", cm => cm.ID == (int)CurrentUserIdOperator.CurrentUserId(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<PermissionPolicyRole>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.ReadWriteAccess, SecurityPermissionState.Allow);
			defaultRole.AddTypePermissionsRecursively<ModelDifference>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermissionsRecursively<ModelDifferenceAspect>(SecurityOperations.Create, SecurityPermissionState.Allow);
            defaultRole.AddTypePermission<AuditDataItemPersistent>(SecurityOperations.Read, SecurityPermissionState.Deny);
            defaultRole.AddObjectPermissionFromLambda<AuditDataItemPersistent>(SecurityOperations.Read, a => a.UserObject.Key == CurrentUserIdOperator.CurrentUserId().ToString(), SecurityPermissionState.Allow);
            defaultRole.AddTypePermission<AuditEFCoreWeakReference>(SecurityOperations.Read, SecurityPermissionState.Allow);
        }
        return defaultRole;
    }
}
