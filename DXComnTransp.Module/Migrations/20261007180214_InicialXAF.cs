using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DXComnTransp.Module.Migrations
{
    public partial class InicialXAF : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // XAF - AUDITORÍA
            // ============================================================

            migrationBuilder.CreateTable(
                name: "AuditEFCoreWeakReference",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    TypeName = table.Column<string>(
                        type: "nvarchar(450)",
                        nullable: true),

                    Key = table.Column<string>(
                        type: "nvarchar(450)",
                        nullable: true),

                    DefaultString = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    LastModifiedDate = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_AuditEFCoreWeakReference",
                        x => x.ID);
                });


            // ============================================================
            // XAF - DASHBOARD
            // ============================================================

            migrationBuilder.CreateTable(
                name: "DashboardData",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Content = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Title = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    SynchronizeTitle = table.Column<bool>(
                        type: "bit",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_DashboardData",
                        x => x.ID);
                });


            // ============================================================
            // XAF - FILE DATA
            // ============================================================

            migrationBuilder.CreateTable(
                name: "FileData",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Size = table.Column<int>(
                        type: "int",
                        nullable: false),

                    FileName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Content = table.Column<byte[]>(
                        type: "varbinary(max)",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_FileData",
                        x => x.ID);
                });


            // ============================================================
            // XAF - KPI
            // ============================================================

            migrationBuilder.CreateTable(
                name: "KpiDefinition",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Active = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    TargetObjectTypeFullName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Criteria = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Expression = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    GreenZone = table.Column<float>(
                        type: "real",
                        nullable: false),

                    RedZone = table.Column<float>(
                        type: "real",
                        nullable: false),

                    Compare = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    RangeName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    RangeToCompareName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    MeasurementFrequency = table.Column<int>(
                        type: "int",
                        nullable: false),

                    MeasurementMode = table.Column<int>(
                        type: "int",
                        nullable: false),

                    Direction = table.Column<int>(
                        type: "int",
                        nullable: false),

                    ChangedOn = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    SuppressedSeries = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    EnableCustomizeRepresentation = table.Column<bool>(
                        type: "bit",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_KpiDefinition",
                        x => x.ID);
                });


            migrationBuilder.CreateTable(
                name: "KpiScorecard",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_KpiScorecard",
                        x => x.ID);
                });


            // ============================================================
            // XAF - MODEL DIFFERENCES
            // ============================================================

            migrationBuilder.CreateTable(
                name: "ModelDifferences",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    UserId = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    ContextId = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Version = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ModelDifferences",
                        x => x.ID);
                });


            migrationBuilder.CreateTable(
                name: "ModulesInfo",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    AssemblyFileName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Version = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    IsMain = table.Column<bool>(
                        type: "bit",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ModulesInfo",
                        x => x.Id);
                });


            // ============================================================
            // XAF - SECURITY ROLE
            // ============================================================

            migrationBuilder.CreateTable(
                name: "PermissionPolicyRoleBase",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    IsAdministrative = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    CanEditModel = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    PermissionPolicy = table.Column<int>(
                        type: "int",
                        nullable: false),

                    IsAllowPermissionPriority = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    Discriminator = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PermissionPolicyRoleBase",
                        x => x.ID);
                });


            // ============================================================
            // XAF - SECURITY USER
            // ============================================================

            migrationBuilder.CreateTable(
                name: "PermissionPolicyUser",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    UserName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    IsActive = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    ChangePasswordOnFirstLogon = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    StoredPassword = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Discriminator = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PermissionPolicyUser",
                        x => x.ID);
                });


            // ============================================================
            // XAF - REPORTS
            // ============================================================

            migrationBuilder.CreateTable(
                name: "ReportDataV2",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    DataTypeName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    IsInplaceReport = table.Column<bool>(
                        type: "bit",
                        nullable: false),

                    PredefinedReportTypeName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Content = table.Column<byte[]>(
                        type: "varbinary(max)",
                        nullable: true),

                    DisplayName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    ParametersObjectTypeName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ReportDataV2",
                        x => x.ID);
                });


            // ============================================================
            // XAF - AUDIT DATA
            // ============================================================

            migrationBuilder.CreateTable(
                name: "AuditData",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    ModifiedOn = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    OperationType = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    PropertyName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    OldValue = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    NewValue = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Description = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    AuditedObjectID = table.Column<int>(
                        type: "int",
                        nullable: true),

                    OldObjectID = table.Column<int>(
                        type: "int",
                        nullable: true),

                    NewObjectID = table.Column<int>(
                        type: "int",
                        nullable: true),

                    UserObjectID = table.Column<int>(
                        type: "int",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_AuditData",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_AuditData_AuditEFCoreWeakReference_AuditedObjectID",
                        column: x => x.AuditedObjectID,
                        principalTable: "AuditEFCoreWeakReference",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_AuditData_AuditEFCoreWeakReference_NewObjectID",
                        column: x => x.NewObjectID,
                        principalTable: "AuditEFCoreWeakReference",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_AuditData_AuditEFCoreWeakReference_OldObjectID",
                        column: x => x.OldObjectID,
                        principalTable: "AuditEFCoreWeakReference",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_AuditData_AuditEFCoreWeakReference_UserObjectID",
                        column: x => x.UserObjectID,
                        principalTable: "AuditEFCoreWeakReference",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });


            // ============================================================
            // XAF - KPI INSTANCE
            // ============================================================

            migrationBuilder.CreateTable(
                name: "KpiInstance",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    KpiDefinitionID = table.Column<int>(
                        type: "int",
                        nullable: false),

                    ForceMeasurementDateTime = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: true),

                    Settings = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_KpiInstance",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_KpiInstance_KpiDefinition_KpiDefinitionID",
                        column: x => x.KpiDefinitionID,
                        principalTable: "KpiDefinition",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });


            // ============================================================
            // XAF - MODEL DIFFERENCE ASPECTS
            // ============================================================

            migrationBuilder.CreateTable(
                name: "ModelDifferenceAspects",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Name = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Xml = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    OwnerID = table.Column<int>(
                        type: "int",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ModelDifferenceAspects",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_ModelDifferenceAspects_ModelDifferences_OwnerID",
                        column: x => x.OwnerID,
                        principalTable: "ModelDifferences",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });


            // ============================================================
            // XAF - SECURITY ACTION PERMISSIONS
            // ============================================================

            migrationBuilder.CreateTable(
                name: "PermissionPolicyActionPermissionObject",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    RoleID = table.Column<int>(
                        type: "int",
                        nullable: true),

                    ActionId = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PermissionPolicyActionPermissionObject",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_PermissionPolicyActionPermissionObject_PermissionPolicyRoleBase_RoleID",
                        column: x => x.RoleID,
                        principalTable: "PermissionPolicyRoleBase",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });


            // ============================================================
            // XAF - SECURITY NAVIGATION PERMISSIONS
            // ============================================================

            migrationBuilder.CreateTable(
                name: "PermissionPolicyNavigationPermissionObject",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    RoleID = table.Column<int>(
                        type: "int",
                        nullable: true),

                    ItemPath = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    TargetTypeFullName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    NavigateState = table.Column<int>(
                        type: "int",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PermissionPolicyNavigationPermissionObject",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_PermissionPolicyNavigationPermissionObject_PermissionPolicyRoleBase_RoleID",
                        column: x => x.RoleID,
                        principalTable: "PermissionPolicyRoleBase",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });


            // ============================================================
            // XAF - SECURITY TYPE PERMISSIONS
            // ============================================================

            migrationBuilder.CreateTable(
                name: "PermissionPolicyTypePermissionObject",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    TargetTypeFullName = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    RoleID = table.Column<int>(
                        type: "int",
                        nullable: true),

                    ReadState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    WriteState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    CreateState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    DeleteState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    NavigateState = table.Column<int>(
                        type: "int",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PermissionPolicyTypePermissionObject",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_PermissionPolicyTypePermissionObject_PermissionPolicyRoleBase_RoleID",
                        column: x => x.RoleID,
                        principalTable: "PermissionPolicyRoleBase",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });


            // ============================================================
            // XAF - USER LOGIN INFO
            // ============================================================

            migrationBuilder.CreateTable(
                name: "PermissionPolicyUserLoginInfo",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    LoginProviderName = table.Column<string>(
                        type: "nvarchar(450)",
                        nullable: true),

                    ProviderUserKey = table.Column<string>(
                        type: "nvarchar(450)",
                        nullable: true),

                    UserForeignKey = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PermissionPolicyUserLoginInfo",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_PermissionPolicyUserLoginInfo_PermissionPolicyUser_UserForeignKey",
                        column: x => x.UserForeignKey,
                        principalTable: "PermissionPolicyUser",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });


            // ============================================================
            // XAF - ROLE / USER
            // ============================================================

            migrationBuilder.CreateTable(
                name: "PermissionPolicyRolePermissionPolicyUser",
                columns: table => new
                {
                    RolesID = table.Column<int>(
                        type: "int",
                        nullable: false),

                    UsersID = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PermissionPolicyRolePermissionPolicyUser",
                        x => new { x.RolesID, x.UsersID });

                    table.ForeignKey(
                        name: "FK_PermissionPolicyRolePermissionPolicyUser_PermissionPolicyRoleBase_RolesID",
                        column: x => x.RolesID,
                        principalTable: "PermissionPolicyRoleBase",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_PermissionPolicyRolePermissionPolicyUser_PermissionPolicyUser_UsersID",
                        column: x => x.UsersID,
                        principalTable: "PermissionPolicyUser",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });


            // ============================================================
            // XAF - MEMBER PERMISSIONS
            // ============================================================

            migrationBuilder.CreateTable(
                name: "PermissionPolicyMemberPermissionsObject",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Members = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    Criteria = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    ReadState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    WriteState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    TypePermissionObjectID = table.Column<int>(
                        type: "int",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PermissionPolicyMemberPermissionsObject",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_PermissionPolicyMemberPermissionsObject_PermissionPolicyTypePermissionObject_TypePermissionObjectID",
                        column: x => x.TypePermissionObjectID,
                        principalTable: "PermissionPolicyTypePermissionObject",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });


            // ============================================================
            // XAF - OBJECT PERMISSIONS
            // ============================================================

            migrationBuilder.CreateTable(
                name: "PermissionPolicyObjectPermissionsObject",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    Criteria = table.Column<string>(
                        type: "nvarchar(max)",
                        nullable: true),

                    ReadState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    WriteState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    DeleteState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    NavigateState = table.Column<int>(
                        type: "int",
                        nullable: true),

                    TypePermissionObjectID = table.Column<int>(
                        type: "int",
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PermissionPolicyObjectPermissionsObject",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_PermissionPolicyObjectPermissionsObject_PermissionPolicyTypePermissionObject_TypePermissionObjectID",
                        column: x => x.TypePermissionObjectID,
                        principalTable: "PermissionPolicyTypePermissionObject",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });


            // ============================================================
            // XAF - KPI HISTORY
            // ============================================================

            migrationBuilder.CreateTable(
                name: "KpiHistoryItem",
                columns: table => new
                {
                    ID = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    KpiInstanceID = table.Column<int>(
                        type: "int",
                        nullable: false),

                    RangeStart = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    RangeEnd = table.Column<DateTime>(
                        type: "datetime2",
                        nullable: false),

                    Value = table.Column<float>(
                        type: "real",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_KpiHistoryItem",
                        x => x.ID);

                    table.ForeignKey(
                        name: "FK_KpiHistoryItem_KpiInstance_KpiInstanceID",
                        column: x => x.KpiInstanceID,
                        principalTable: "KpiInstance",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });


            // ============================================================
            // XAF - KPI / SCORECARD
            // ============================================================

            migrationBuilder.CreateTable(
                name: "KpiInstanceKpiScorecard",
                columns: table => new
                {
                    IndicatorsID = table.Column<int>(
                        type: "int",
                        nullable: false),

                    ScorecardsID = table.Column<int>(
                        type: "int",
                        nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_KpiInstanceKpiScorecard",
                        x => new
                        {
                            x.IndicatorsID,
                            x.ScorecardsID
                        });

                    table.ForeignKey(
                        name: "FK_KpiInstanceKpiScorecard_KpiInstance_IndicatorsID",
                        column: x => x.IndicatorsID,
                        principalTable: "KpiInstance",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);

                    table.ForeignKey(
                        name: "FK_KpiInstanceKpiScorecard_KpiScorecard_ScorecardsID",
                        column: x => x.ScorecardsID,
                        principalTable: "KpiScorecard",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });


            // ============================================================
            // XAF - ÍNDICES
            // ============================================================

            migrationBuilder.CreateIndex(
                name: "IX_AuditData_AuditedObjectID",
                table: "AuditData",
                column: "AuditedObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditData_NewObjectID",
                table: "AuditData",
                column: "NewObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditData_OldObjectID",
                table: "AuditData",
                column: "OldObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditData_UserObjectID",
                table: "AuditData",
                column: "UserObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_AuditEFCoreWeakReference_Key_TypeName",
                table: "AuditEFCoreWeakReference",
                columns: new[]
                {
                    "Key",
                    "TypeName"
                });

            migrationBuilder.CreateIndex(
                name: "IX_KpiHistoryItem_KpiInstanceID",
                table: "KpiHistoryItem",
                column: "KpiInstanceID");

            migrationBuilder.CreateIndex(
                name: "IX_KpiInstance_KpiDefinitionID",
                table: "KpiInstance",
                column: "KpiDefinitionID");

            migrationBuilder.CreateIndex(
                name: "IX_KpiInstanceKpiScorecard_ScorecardsID",
                table: "KpiInstanceKpiScorecard",
                column: "ScorecardsID");

            migrationBuilder.CreateIndex(
                name: "IX_ModelDifferenceAspects_OwnerID",
                table: "ModelDifferenceAspects",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyActionPermissionObject_RoleID",
                table: "PermissionPolicyActionPermissionObject",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyMemberPermissionsObject_TypePermissionObjectID",
                table: "PermissionPolicyMemberPermissionsObject",
                column: "TypePermissionObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyNavigationPermissionObject_RoleID",
                table: "PermissionPolicyNavigationPermissionObject",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyObjectPermissionsObject_TypePermissionObjectID",
                table: "PermissionPolicyObjectPermissionsObject",
                column: "TypePermissionObjectID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyRolePermissionPolicyUser_UsersID",
                table: "PermissionPolicyRolePermissionPolicyUser",
                column: "UsersID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyTypePermissionObject_RoleID",
                table: "PermissionPolicyTypePermissionObject",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyUserLoginInfo_LoginProviderName_ProviderUserKey",
                table: "PermissionPolicyUserLoginInfo",
                columns: new[]
                {
                    "LoginProviderName",
                    "ProviderUserKey"
                },
                unique: true,
                filter: "[LoginProviderName] IS NOT NULL AND [ProviderUserKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PermissionPolicyUserLoginInfo_UserForeignKey",
                table: "PermissionPolicyUserLoginInfo",
                column: "UserForeignKey");
        }


        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // SOLO ELIMINAR INFRAESTRUCTURA XAF
            // NUNCA ELIMINAR TABLAS DE COMBTRANSP
            // ============================================================

            migrationBuilder.DropTable(
                name: "AuditData");

            migrationBuilder.DropTable(
                name: "KpiHistoryItem");

            migrationBuilder.DropTable(
                name: "KpiInstanceKpiScorecard");

            migrationBuilder.DropTable(
                name: "ModelDifferenceAspects");

            migrationBuilder.DropTable(
                name: "PermissionPolicyActionPermissionObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyMemberPermissionsObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyNavigationPermissionObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyObjectPermissionsObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyRolePermissionPolicyUser");

            migrationBuilder.DropTable(
                name: "PermissionPolicyUserLoginInfo");

            migrationBuilder.DropTable(
                name: "ReportDataV2");

            migrationBuilder.DropTable(
                name: "DashboardData");

            migrationBuilder.DropTable(
                name: "FileData");

            migrationBuilder.DropTable(
                name: "ModulesInfo");

            migrationBuilder.DropTable(
                name: "AuditEFCoreWeakReference");

            migrationBuilder.DropTable(
                name: "KpiInstance");

            migrationBuilder.DropTable(
                name: "KpiScorecard");

            migrationBuilder.DropTable(
                name: "ModelDifferences");

            migrationBuilder.DropTable(
                name: "PermissionPolicyTypePermissionObject");

            migrationBuilder.DropTable(
                name: "PermissionPolicyUser");

            migrationBuilder.DropTable(
                name: "PermissionPolicyRoleBase");

            migrationBuilder.DropTable(
                name: "KpiDefinition");
        }
    }
}