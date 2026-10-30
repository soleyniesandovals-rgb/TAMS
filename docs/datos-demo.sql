/*
    Datos mínimos para probar el módulo de calificaciones (Práctica 3).

    Requisitos previos:
      - Las migraciones de Core y de negocio ya están aplicadas.
      - El usuario docente ya existe en core.Usuarios.

    Antes de ejecutar, cambia @CorreoDocente por el correo del docente.
    El script es idempotente: puede ejecutarse varias veces sin duplicar los datos.
    No contiene credenciales ni contraseñas.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @CorreoDocente nvarchar(320) = N'CAMBIAR_POR_CORREO';
DECLARE @DocenteId int = (
    SELECT TOP (1) Id
    FROM core.Usuarios
    WHERE Correo = @CorreoDocente
    ORDER BY Id
);

IF @DocenteId IS NULL
BEGIN
    THROW 50001, N'No existe un usuario en core.Usuarios con el correo indicado. Cambia @CorreoDocente antes de ejecutar el script.', 1;
END

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Ahora datetime2 = SYSUTCDATETIME();

    -- 1 Centro. No existe una relación directa con cursos en el esquema actual.
    IF NOT EXISTS (SELECT 1 FROM negocio.Centros WHERE Nombre = N'Centro Demo TAMS')
    BEGIN
        INSERT INTO negocio.Centros
            (Nombre, TipoCentro, TiposTecnicosHabilitados, FechaCreacion)
        VALUES
            (N'Centro Demo TAMS', N'Tecnica', N'DesarrolloAdministracionAplicacionesInformaticas', @Ahora);
    END

    -- 1 año escolar activo. Si ya hay uno, se reutiliza para respetar el índice único filtrado.
    DECLARE @AnioEscolarId int = (
        SELECT TOP (1) Id
        FROM negocio.AniosEscolares
        WHERE EsActivo = 1
        ORDER BY Id
    );

    IF @AnioEscolarId IS NULL
    BEGIN
        SET @AnioEscolarId = (
            SELECT TOP (1) Id
            FROM negocio.AniosEscolares
            WHERE Nombre = N'2026-2027'
            ORDER BY Id
        );

        IF @AnioEscolarId IS NULL
        BEGIN
            INSERT INTO negocio.AniosEscolares
                (Nombre, FechaInicio, FechaFin, EsActivo, FechaCreacion)
            VALUES
                (N'2026-2027', '2026-08-01', '2027-06-30', 1, @Ahora);

            SET @AnioEscolarId = CAST(SCOPE_IDENTITY() AS int);
        END
        ELSE
        BEGIN
            UPDATE negocio.AniosEscolares
            SET EsActivo = 1
            WHERE Id = @AnioEscolarId;
        END
    END

    -- 1 curso dentro del año escolar activo.
    DECLARE @CursoId int = (
        SELECT TOP (1) Id
        FROM negocio.Cursos
        WHERE AnioEscolarId = @AnioEscolarId
          AND Grado = N'1er Grado'
          AND Seccion = N'A'
        ORDER BY Id
    );

    IF @CursoId IS NULL
    BEGIN
        INSERT INTO negocio.Cursos
            (Grado, Seccion, EsTecnico, TipoTecnico, AnioEscolarId, FechaCreacion)
        VALUES
            (N'1er Grado', N'A', 1, N'DesarrolloAdministracionAplicacionesInformaticas', @AnioEscolarId, @Ahora);

        SET @CursoId = CAST(SCOPE_IDENTITY() AS int);
    END

    -- Materia no técnica: es la que se asigna al docente para probar el flujo principal.
    DECLARE @MateriaNoTecnicaId int = (
        SELECT TOP (1) Id
        FROM negocio.Materias
        WHERE Nombre = N'Matemática'
          AND EsTecnica = 0
        ORDER BY Id
    );

    IF @MateriaNoTecnicaId IS NULL
    BEGIN
        INSERT INTO negocio.Materias
            (Nombre, EsTecnica, TipoTecnico, CantidadRA, FechaCreacion)
        VALUES
            (N'Matemática', 0, NULL, NULL, @Ahora);

        SET @MateriaNoTecnicaId = CAST(SCOPE_IDENTITY() AS int);
    END

    -- Materia técnica: permite probar también las calificaciones por RA.
    IF NOT EXISTS (
        SELECT 1
        FROM negocio.Materias
        WHERE Nombre = N'Programación'
          AND EsTecnica = 1
    )
    BEGIN
        INSERT INTO negocio.Materias
            (Nombre, EsTecnica, TipoTecnico, CantidadRA, FechaCreacion)
        VALUES
            (N'Programación', 1, N'DesarrolloAdministracionAplicacionesInformaticas', 3, @Ahora);
    END

    -- 2 estudiantes en el curso.
    IF NOT EXISTS (
        SELECT 1
        FROM negocio.Estudiantes
        WHERE CursoId = @CursoId
          AND Nombre = N'Ana'
          AND Apellido = N'Pérez'
    )
    BEGIN
        INSERT INTO negocio.Estudiantes
            (Nombre, Apellido, CursoId, FechaCreacion)
        VALUES
            (N'Ana', N'Pérez', @CursoId, @Ahora);
    END

    IF NOT EXISTS (
        SELECT 1
        FROM negocio.Estudiantes
        WHERE CursoId = @CursoId
          AND Nombre = N'Luis'
          AND Apellido = N'Gómez'
    )
    BEGIN
        INSERT INTO negocio.Estudiantes
            (Nombre, Apellido, CursoId, FechaCreacion)
        VALUES
            (N'Luis', N'Gómez', @CursoId, @Ahora);
    END

    -- 1 asignación del docente a la materia no técnica en el curso.
    -- El índice único (MateriaId, CursoId) impide que otro docente tenga la misma asignación.
    IF NOT EXISTS (
        SELECT 1
        FROM negocio.AsignacionesDocentes
        WHERE MateriaId = @MateriaNoTecnicaId
          AND CursoId = @CursoId
    )
    BEGIN
        INSERT INTO negocio.AsignacionesDocentes
            (UsuarioId, MateriaId, CursoId, HorasSemanales, FechaCreacion)
        VALUES
            (@DocenteId, @MateriaNoTecnicaId, @CursoId, 4, @Ahora);
    END
    ELSE IF EXISTS (
        SELECT 1
        FROM negocio.AsignacionesDocentes
        WHERE MateriaId = @MateriaNoTecnicaId
          AND CursoId = @CursoId
          AND UsuarioId <> @DocenteId
    )
    BEGIN
        THROW 50002, N'Ya existe una asignación de esa materia y curso para otro docente. Revisa los datos antes de continuar.', 1;
    END

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END

    THROW;
END CATCH
