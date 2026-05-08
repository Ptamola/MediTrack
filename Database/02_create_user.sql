-- MediTrack - Creación de usuario MySQL para la aplicación
-- Ejecutar este script con un usuario administrador de MySQL, por ejemplo root.

CREATE USER IF NOT EXISTS 'meditrak_app'@'localhost'
IDENTIFIED BY 'MeditrakDemo123!';

GRANT SELECT, INSERT, UPDATE, DELETE, CREATE, ALTER, INDEX, REFERENCES
ON meditrak_db.*
TO 'meditrak_app'@'localhost';

FLUSH PRIVILEGES;