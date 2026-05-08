-- MediTrack - Creación de base de datos local
-- Ejecutar este script con un usuario administrador de MySQL, por ejemplo root.

CREATE DATABASE IF NOT EXISTS meditrak_db
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE meditrak_db;

-- Las tablas de MediTrack no se crean manualmente aquí.
-- La aplicación ejecuta DatabaseInitializer al arrancar y crea/actualiza
-- automáticamente las tablas necesarias si no existen.
