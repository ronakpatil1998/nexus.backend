IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425052031_InitialCreate'
)
BEGIN
    CREATE TABLE [Permissions] (
        [Id] int NOT NULL IDENTITY,
        [AppName] nvarchar(max) NOT NULL,
        [AccessLevel] nvarchar(max) NOT NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425052031_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [Rank] int NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425052031_InitialCreate'
)
BEGIN
    CREATE TABLE [RolePermission] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] int NOT NULL,
        [PermissionId] int NOT NULL,
        CONSTRAINT [PK_RolePermission] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RolePermission_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425052031_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] int NOT NULL IDENTITY,
        [FullName] nvarchar(max) NOT NULL,
        [Email] nvarchar(max) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425052031_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Rank') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] ON;
    EXEC(N'INSERT INTO [Roles] ([Id], [Name], [Rank])
    VALUES (1, N''Admin'', 100),
    (2, N''SVP'', 80),
    (3, N''VP'', 60),
    (4, N''PM'', 40),
    (5, N''TL'', 20),
    (6, N''SE'', 10)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Rank') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425052031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RolePermission_RoleId] ON [RolePermission] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425052031_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425052031_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425052031_InitialCreate', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    ALTER TABLE [RolePermission] DROP CONSTRAINT [FK_RolePermission_Roles_RoleId];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    ALTER TABLE [RolePermission] DROP CONSTRAINT [PK_RolePermission];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    DROP INDEX [IX_RolePermission_RoleId] ON [RolePermission];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    DECLARE @var sysname;
    SELECT @var = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[RolePermission]') AND [c].[name] = N'Id');
    IF @var IS NOT NULL EXEC(N'ALTER TABLE [RolePermission] DROP CONSTRAINT [' + @var + '];');
    ALTER TABLE [RolePermission] DROP COLUMN [Id];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    EXEC sp_rename N'[RolePermission]', N'RolePermissions', 'OBJECT';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    ALTER TABLE [RolePermissions] ADD [PermissionName] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    ALTER TABLE [RolePermissions] ADD CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessLevel', N'AppName') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] ON;
    EXEC(N'INSERT INTO [Permissions] ([Id], [AccessLevel], [AppName])
    VALUES (1, N''Full'', N''Nexus Dashboard''),
    (2, N''Admin'', N''Role Manager'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessLevel', N'AppName') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260425121915_UpdateRolePermissionSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260425121915_UpdateRolePermissionSchema', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    EXEC sp_rename N'[Permissions].[AccessLevel]', N'Name', 'COLUMN';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    ALTER TABLE [Permissions] ADD [Description] nvarchar(max) NOT NULL DEFAULT N'';
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    CREATE TABLE [Applications] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(max) NOT NULL,
        [RoutePath] nvarchar(max) NOT NULL,
        [IconName] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Applications] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IconName', N'IsActive', N'Name', N'RoutePath') AND [object_id] = OBJECT_ID(N'[Applications]'))
        SET IDENTITY_INSERT [Applications] ON;
    EXEC(N'INSERT INTO [Applications] ([Id], [IconName], [IsActive], [Name], [RoutePath])
    VALUES (1, N''Users'', CAST(1 AS bit), N''Identity Vault'', N''/admin/users''),
    (2, N''LayoutDashboard'', CAST(1 AS bit), N''Dashboard'', N''/dashboard'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IconName', N'IsActive', N'Name', N'RoutePath') AND [object_id] = OBJECT_ID(N'[Applications]'))
        SET IDENTITY_INSERT [Applications] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    EXEC(N'UPDATE [Permissions] SET [AppName] = N''Identity Vault'', [Description] = N''Full access to manage corporate agents'', [Name] = N''USER_MANAGEMENT_FULL''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    EXEC(N'UPDATE [Permissions] SET [AppName] = N''Dashboard'', [Description] = N''Permission to view the main analytics dashboard'', [Name] = N''DASHBOARD_VIEW_BASIC''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'RoleId', N'PermissionName') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
        SET IDENTITY_INSERT [RolePermissions] ON;
    EXEC(N'INSERT INTO [RolePermissions] ([PermissionId], [RoleId], [PermissionName])
    VALUES (1, 1, N''IDENTITY_VAULT_FULL_ACCESS''),
    (2, 1, N''DASHBOARD_VIEW'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'PermissionId', N'RoleId', N'PermissionName') AND [object_id] = OBJECT_ID(N'[RolePermissions]'))
        SET IDENTITY_INSERT [RolePermissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    ALTER TABLE [RolePermissions] ADD CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427014351_UpdateSeedData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260427014351_UpdateSeedData', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    ALTER TABLE [Permissions] ADD [CanDelete] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    ALTER TABLE [Permissions] ADD [CanEdit] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    ALTER TABLE [Permissions] ADD [CanRead] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    ALTER TABLE [Permissions] ADD [CanWrite] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    CREATE TABLE [UserPermissionOverrides] (
        [Id] int NOT NULL IDENTITY,
        [UserId] int NOT NULL,
        [PermissionId] int NOT NULL,
        CONSTRAINT [PK_UserPermissionOverrides] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserPermissionOverrides_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [Permissions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserPermissionOverrides_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    EXEC(N'UPDATE [Permissions] SET [CanDelete] = CAST(0 AS bit), [CanEdit] = CAST(0 AS bit), [CanRead] = CAST(0 AS bit), [CanWrite] = CAST(0 AS bit)
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    EXEC(N'UPDATE [Permissions] SET [CanDelete] = CAST(0 AS bit), [CanEdit] = CAST(0 AS bit), [CanRead] = CAST(0 AS bit), [CanWrite] = CAST(0 AS bit)
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    CREATE INDEX [IX_UserPermissionOverrides_PermissionId] ON [UserPermissionOverrides] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    CREATE INDEX [IX_UserPermissionOverrides_UserId] ON [UserPermissionOverrides] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260427084335_AddUserOverridesAndMatrixFlags'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260427084335_AddUserOverridesAndMatrixFlags', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501162433_AddUserIdToPermissions'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501162433_AddUserIdToPermissions'
)
BEGIN
    EXEC(N'DELETE FROM [Permissions]
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501162433_AddUserIdToPermissions'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Permissions]') AND [c].[name] = N'AppName');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Permissions] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Permissions] ALTER COLUMN [AppName] nvarchar(450) NOT NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501162433_AddUserIdToPermissions'
)
BEGIN
    ALTER TABLE [Permissions] ADD [UserId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501162433_AddUserIdToPermissions'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_UserId_AppName] ON [Permissions] ([UserId], [AppName]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260501162433_AddUserIdToPermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260501162433_AddUserIdToPermissions', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502042410_AddRoleIdToPermissions'
)
BEGIN
    DROP INDEX [IX_Permissions_UserId_AppName] ON [Permissions];
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502042410_AddRoleIdToPermissions'
)
BEGIN
    EXEC(N'DELETE FROM [RolePermissions]
    WHERE [PermissionId] = 1 AND [RoleId] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502042410_AddRoleIdToPermissions'
)
BEGIN
    EXEC(N'DELETE FROM [RolePermissions]
    WHERE [PermissionId] = 2 AND [RoleId] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502042410_AddRoleIdToPermissions'
)
BEGIN
    ALTER TABLE [Permissions] ADD [RoleId] int NOT NULL DEFAULT 0;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502042410_AddRoleIdToPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AppName', N'CanDelete', N'CanEdit', N'CanRead', N'CanWrite', N'Description', N'Name', N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] ON;
    EXEC(N'INSERT INTO [Permissions] ([Id], [AppName], [CanDelete], [CanEdit], [CanRead], [CanWrite], [Description], [Name], [RoleId], [UserId])
    VALUES (1, N''Identity Vault'', CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), CAST(1 AS bit), N'''', N''ADMIN_VAULT_MASTER'', 1, 0),
    (2, N''Dashboard'', CAST(0 AS bit), CAST(0 AS bit), CAST(1 AS bit), CAST(0 AS bit), N'''', N''ADMIN_DASHBOARD_MASTER'', 1, 0)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AppName', N'CanDelete', N'CanEdit', N'CanRead', N'CanWrite', N'Description', N'Name', N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[Permissions]'))
        SET IDENTITY_INSERT [Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502042410_AddRoleIdToPermissions'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Permissions_RoleId_AppName] ON [Permissions] ([RoleId], [AppName]) WHERE [RoleId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502042410_AddRoleIdToPermissions'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Permissions_UserId_AppName] ON [Permissions] ([UserId], [AppName]) WHERE [RoleId] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502042410_AddRoleIdToPermissions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502042410_AddRoleIdToPermissions', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502051638_FinalizePermissionIsolation'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Permissions]') AND [c].[name] = N'RoleId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Permissions] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [Permissions] ALTER COLUMN [RoleId] int NULL;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502051638_FinalizePermissionIsolation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502051638_FinalizePermissionIsolation', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502051801_PermissionIsolation'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502051801_PermissionIsolation', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502052204_CompletePermissionSync'
)
BEGIN
    EXEC(N'UPDATE [Permissions] SET [Description] = N''Full access for Admin Role''
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502052204_CompletePermissionSync'
)
BEGIN
    EXEC(N'UPDATE [Permissions] SET [Description] = N''Read-only access for Admin Role''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502052204_CompletePermissionSync'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502052204_CompletePermissionSync', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502074251_AddIsInternalToApplication'
)
BEGIN
    ALTER TABLE [Applications] ADD [IsInternal] bit NOT NULL DEFAULT CAST(0 AS bit);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502074251_AddIsInternalToApplication'
)
BEGIN
    EXEC(N'UPDATE [Applications] SET [IsInternal] = CAST(1 AS bit)
    WHERE [Id] = 1;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502074251_AddIsInternalToApplication'
)
BEGIN
    EXEC(N'UPDATE [Applications] SET [IconName] = N''Package'', [IsInternal] = CAST(1 AS bit), [Name] = N''App Registry'', [RoutePath] = N''/admin/apps''
    WHERE [Id] = 2;
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502074251_AddIsInternalToApplication'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IconName', N'IsActive', N'IsInternal', N'Name', N'RoutePath') AND [object_id] = OBJECT_ID(N'[Applications]'))
        SET IDENTITY_INSERT [Applications] ON;
    EXEC(N'INSERT INTO [Applications] ([Id], [IconName], [IsActive], [IsInternal], [Name], [RoutePath])
    VALUES (3, N''Shield'', CAST(1 AS bit), CAST(1 AS bit), N''Security Matrix'', N''/admin/security''),
    (4, N''GitGraph'', CAST(1 AS bit), CAST(1 AS bit), N''Access Control'', N''/admin/roles''),
    (5, N''Permission'', CAST(1 AS bit), CAST(1 AS bit), N''Role Permission Matrix'', N''/admin/role-permissions'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'IconName', N'IsActive', N'IsInternal', N'Name', N'RoutePath') AND [object_id] = OBJECT_ID(N'[Applications]'))
        SET IDENTITY_INSERT [Applications] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502074251_AddIsInternalToApplication'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502074251_AddIsInternalToApplication', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502074834_AddIsInternalToApplicationData'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502074834_AddIsInternalToApplicationData', N'9.0.15');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260502074930_UpdateApplicationSeeds'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260502074930_UpdateApplicationSeeds', N'9.0.15');
END;

COMMIT;
GO

