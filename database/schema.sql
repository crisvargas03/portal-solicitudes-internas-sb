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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [Areas] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Areas] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [EstadosSolicitud] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(40) NOT NULL,
        [Orden] int NOT NULL,
        [EsFinal] bit NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_EstadosSolicitud] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [Prioridades] (
        [Id] int NOT NULL IDENTITY,
        [Nivel] int NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Prioridades] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [TiposSolicitud] (
        [Id] int NOT NULL IDENTITY,
        [Descripcion] nvarchar(500) NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_TiposSolicitud] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [Usuarios] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(150) NOT NULL,
        [Email] nvarchar(150) NOT NULL,
        [Rol] int NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Usuarios] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [TransicionesPermitidas] (
        [Id] int NOT NULL IDENTITY,
        [EstadoOrigenId] int NOT NULL,
        [EstadoDestinoId] int NOT NULL,
        [RequiereComentario] bit NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_TransicionesPermitidas] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TransicionesPermitidas_EstadosSolicitud_EstadoDestinoId] FOREIGN KEY ([EstadoDestinoId]) REFERENCES [EstadosSolicitud] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_TransicionesPermitidas_EstadosSolicitud_EstadoOrigenId] FOREIGN KEY ([EstadoOrigenId]) REFERENCES [EstadosSolicitud] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [Solicitudes] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] nvarchar(20) NOT NULL,
        [Titulo] nvarchar(150) NOT NULL,
        [Descripcion] nvarchar(2000) NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaCompromiso] datetime2 NULL,
        [PrioridadId] int NOT NULL,
        [EstadoId] int NOT NULL,
        [AreaId] int NOT NULL,
        [TipoSolicitudId] int NOT NULL,
        [UsuarioSolicitanteId] int NOT NULL,
        [UsuarioAsignadoId] int NULL,
        CONSTRAINT [PK_Solicitudes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Solicitudes_Areas_AreaId] FOREIGN KEY ([AreaId]) REFERENCES [Areas] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_EstadosSolicitud_EstadoId] FOREIGN KEY ([EstadoId]) REFERENCES [EstadosSolicitud] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_Prioridades_PrioridadId] FOREIGN KEY ([PrioridadId]) REFERENCES [Prioridades] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_TiposSolicitud_TipoSolicitudId] FOREIGN KEY ([TipoSolicitudId]) REFERENCES [TiposSolicitud] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_Usuarios_UsuarioAsignadoId] FOREIGN KEY ([UsuarioAsignadoId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitudes_Usuarios_UsuarioSolicitanteId] FOREIGN KEY ([UsuarioSolicitanteId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [TransicionesPermitidasRoles] (
        [Id] int NOT NULL IDENTITY,
        [TransicionPermitidaId] int NOT NULL,
        [Rol] int NOT NULL,
        CONSTRAINT [PK_TransicionesPermitidasRoles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_TransicionesPermitidasRoles_TransicionesPermitidas_TransicionPermitidaId] FOREIGN KEY ([TransicionPermitidaId]) REFERENCES [TransicionesPermitidas] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [Adjuntos] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        [Descripcion] nvarchar(200) NOT NULL,
        [Url] nvarchar(500) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_Adjuntos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Adjuntos_Solicitudes_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitudes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Adjuntos_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [Comentarios] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        [Texto] nvarchar(1000) NOT NULL,
        [EsInterno] bit NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_Comentarios] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comentarios_Solicitudes_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitudes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Comentarios_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [HistorialEstados] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] int NOT NULL,
        [EstadoAnteriorId] int NULL,
        [EstadoNuevoId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        [Comentario] nvarchar(1000) NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_HistorialEstados] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HistorialEstados_EstadosSolicitud_EstadoAnteriorId] FOREIGN KEY ([EstadoAnteriorId]) REFERENCES [EstadosSolicitud] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistorialEstados_EstadosSolicitud_EstadoNuevoId] FOREIGN KEY ([EstadoNuevoId]) REFERENCES [EstadosSolicitud] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistorialEstados_Solicitudes_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitudes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_HistorialEstados_Usuarios_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE TABLE [Notificaciones] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] int NOT NULL,
        [UsuarioDestinoId] int NOT NULL,
        [Canal] int NOT NULL,
        [Asunto] nvarchar(200) NOT NULL,
        [Mensaje] nvarchar(1000) NOT NULL,
        [Estado] int NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_Notificaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Notificaciones_Solicitudes_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitudes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Notificaciones_Usuarios_UsuarioDestinoId] FOREIGN KEY ([UsuarioDestinoId]) REFERENCES [Usuarios] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Nombre') AND [object_id] = OBJECT_ID(N'[Areas]'))
        SET IDENTITY_INSERT [Areas] ON;
    EXEC(N'INSERT INTO [Areas] ([Id], [Activo], [Nombre])
    VALUES (1, CAST(1 AS bit), N''Tecnología de la Información''),
    (2, CAST(1 AS bit), N''Supervisión Bancaria''),
    (3, CAST(1 AS bit), N''Recursos Humanos''),
    (4, CAST(1 AS bit), N''Administración y Finanzas''),
    (5, CAST(1 AS bit), N''Consultoría Jurídica'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Nombre') AND [object_id] = OBJECT_ID(N'[Areas]'))
        SET IDENTITY_INSERT [Areas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'EsFinal', N'Nombre', N'Orden') AND [object_id] = OBJECT_ID(N'[EstadosSolicitud]'))
        SET IDENTITY_INSERT [EstadosSolicitud] ON;
    EXEC(N'INSERT INTO [EstadosSolicitud] ([Id], [Activo], [Codigo], [EsFinal], [Nombre], [Orden])
    VALUES (1, CAST(1 AS bit), N''REGISTRADA'', CAST(0 AS bit), N''Registrada'', 1),
    (2, CAST(1 AS bit), N''EN_ANALISIS'', CAST(0 AS bit), N''En análisis'', 2),
    (3, CAST(1 AS bit), N''EN_PROGRESO'', CAST(0 AS bit), N''En progreso'', 3),
    (4, CAST(1 AS bit), N''EN_ESPERA_SOLICITANTE'', CAST(0 AS bit), N''En espera del solicitante'', 4),
    (5, CAST(1 AS bit), N''RESUELTA'', CAST(0 AS bit), N''Resuelta'', 5),
    (6, CAST(1 AS bit), N''CERRADA'', CAST(1 AS bit), N''Cerrada'', 6)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Codigo', N'EsFinal', N'Nombre', N'Orden') AND [object_id] = OBJECT_ID(N'[EstadosSolicitud]'))
        SET IDENTITY_INSERT [EstadosSolicitud] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Nivel', N'Nombre') AND [object_id] = OBJECT_ID(N'[Prioridades]'))
        SET IDENTITY_INSERT [Prioridades] ON;
    EXEC(N'INSERT INTO [Prioridades] ([Id], [Activo], [Nivel], [Nombre])
    VALUES (1, CAST(1 AS bit), 1, N''Baja''),
    (2, CAST(1 AS bit), 2, N''Media''),
    (3, CAST(1 AS bit), 3, N''Alta''),
    (4, CAST(1 AS bit), 4, N''Crítica'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Nivel', N'Nombre') AND [object_id] = OBJECT_ID(N'[Prioridades]'))
        SET IDENTITY_INSERT [Prioridades] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[TiposSolicitud]'))
        SET IDENTITY_INSERT [TiposSolicitud] ON;
    EXEC(N'INSERT INTO [TiposSolicitud] ([Id], [Activo], [Descripcion], [Nombre])
    VALUES (1, CAST(1 AS bit), N''Fallas de equipos, red o servicios de tecnología.'', N''Soporte técnico''),
    (2, CAST(1 AS bit), N''Altas, bajas y cambios de permisos en aplicaciones internas.'', N''Acceso a sistemas''),
    (3, CAST(1 AS bit), N''Solicitud, reemplazo o traslado de equipos.'', N''Equipo y hardware''),
    (4, CAST(1 AS bit), N''Instalaciones, licencias y nuevas funcionalidades.'', N''Requerimiento de software''),
    (5, CAST(1 AS bit), N''Dudas y orientación que no encajan en otro tipo.'', N''Consulta general'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Descripcion', N'Nombre') AND [object_id] = OBJECT_ID(N'[TiposSolicitud]'))
        SET IDENTITY_INSERT [TiposSolicitud] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'EstadoDestinoId', N'EstadoOrigenId', N'RequiereComentario') AND [object_id] = OBJECT_ID(N'[TransicionesPermitidas]'))
        SET IDENTITY_INSERT [TransicionesPermitidas] ON;
    EXEC(N'INSERT INTO [TransicionesPermitidas] ([Id], [Activo], [EstadoDestinoId], [EstadoOrigenId], [RequiereComentario])
    VALUES (1, CAST(1 AS bit), 2, 1, CAST(0 AS bit)),
    (2, CAST(1 AS bit), 3, 2, CAST(0 AS bit)),
    (3, CAST(1 AS bit), 4, 3, CAST(1 AS bit)),
    (4, CAST(1 AS bit), 3, 4, CAST(0 AS bit)),
    (5, CAST(1 AS bit), 5, 3, CAST(1 AS bit)),
    (6, CAST(1 AS bit), 3, 5, CAST(1 AS bit)),
    (7, CAST(1 AS bit), 6, 5, CAST(0 AS bit)),
    (8, CAST(1 AS bit), 2, 6, CAST(1 AS bit))');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'EstadoDestinoId', N'EstadoOrigenId', N'RequiereComentario') AND [object_id] = OBJECT_ID(N'[TransicionesPermitidas]'))
        SET IDENTITY_INSERT [TransicionesPermitidas] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Rol', N'TransicionPermitidaId') AND [object_id] = OBJECT_ID(N'[TransicionesPermitidasRoles]'))
        SET IDENTITY_INSERT [TransicionesPermitidasRoles] ON;
    EXEC(N'INSERT INTO [TransicionesPermitidasRoles] ([Id], [Rol], [TransicionPermitidaId])
    VALUES (1, 1, 1),
    (2, 2, 1),
    (3, 1, 2),
    (4, 2, 2),
    (5, 1, 3),
    (6, 2, 3),
    (7, 1, 4),
    (8, 2, 4),
    (9, 3, 4),
    (10, 1, 5),
    (11, 2, 5),
    (12, 1, 6),
    (13, 2, 6),
    (14, 1, 7),
    (15, 2, 7),
    (16, 3, 7),
    (17, 1, 8),
    (18, 2, 8)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Rol', N'TransicionPermitidaId') AND [object_id] = OBJECT_ID(N'[TransicionesPermitidasRoles]'))
        SET IDENTITY_INSERT [TransicionesPermitidasRoles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Adjuntos_SolicitudId] ON [Adjuntos] ([SolicitudId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Adjuntos_UsuarioId] ON [Adjuntos] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Areas_Nombre] ON [Areas] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comentarios_SolicitudId_EsInterno] ON [Comentarios] ([SolicitudId], [EsInterno]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Comentarios_UsuarioId] ON [Comentarios] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EstadosSolicitud_Codigo] ON [EstadosSolicitud] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EstadosSolicitud_Orden] ON [EstadosSolicitud] ([Orden]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistorialEstados_EstadoAnteriorId] ON [HistorialEstados] ([EstadoAnteriorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistorialEstados_EstadoNuevoId] ON [HistorialEstados] ([EstadoNuevoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistorialEstados_SolicitudId_EstadoNuevoId] ON [HistorialEstados] ([SolicitudId], [EstadoNuevoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_HistorialEstados_UsuarioId] ON [HistorialEstados] ([UsuarioId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_Estado] ON [Notificaciones] ([Estado]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_SolicitudId] ON [Notificaciones] ([SolicitudId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Notificaciones_UsuarioDestinoId] ON [Notificaciones] ([UsuarioDestinoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Prioridades_Nivel] ON [Prioridades] ([Nivel]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Prioridades_Nombre] ON [Prioridades] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_AreaId] ON [Solicitudes] ([AreaId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Solicitudes_Codigo] ON [Solicitudes] ([Codigo]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_EstadoId] ON [Solicitudes] ([EstadoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_FechaCompromiso] ON [Solicitudes] ([FechaCompromiso]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_FechaCreacion] ON [Solicitudes] ([FechaCreacion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_PrioridadId] ON [Solicitudes] ([PrioridadId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_TipoSolicitudId] ON [Solicitudes] ([TipoSolicitudId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_UsuarioAsignadoId] ON [Solicitudes] ([UsuarioAsignadoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Solicitudes_UsuarioSolicitanteId] ON [Solicitudes] ([UsuarioSolicitanteId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TiposSolicitud_Nombre] ON [TiposSolicitud] ([Nombre]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_TransicionesPermitidas_EstadoDestinoId] ON [TransicionesPermitidas] ([EstadoDestinoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TransicionesPermitidas_EstadoOrigenId_EstadoDestinoId] ON [TransicionesPermitidas] ([EstadoOrigenId], [EstadoDestinoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TransicionesPermitidasRoles_TransicionPermitidaId_Rol] ON [TransicionesPermitidasRoles] ([TransicionPermitidaId], [Rol]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [Usuarios] ([Email]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910043656_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910043656_InitialCreate', N'8.0.11');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910191422_AgregarContadorCodigo'
)
BEGIN
    CREATE TABLE [ContadoresCodigo] (
        [Anio] int NOT NULL,
        [Ultimo] int NOT NULL,
        CONSTRAINT [PK_ContadoresCodigo] PRIMARY KEY ([Anio])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260910191422_AgregarContadorCodigo'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260910191422_AgregarContadorCodigo', N'8.0.11');
END;
GO

COMMIT;
GO

