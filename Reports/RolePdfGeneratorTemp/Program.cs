using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var outputDir = @"C:\Users\Admin\Desktop\MediTrack\MediTrack\Reports";
Directory.CreateDirectory(outputDir);
var outputFile = Path.Combine(outputDir, "MediTrack_Roles_Mejora.pdf");

Document.Create(container =>
{
    container.Page(page =>
    {
        page.Size(PageSizes.A4);
        page.Margin(28);
        page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

        page.Header().Column(column =>
        {
            column.Spacing(6);
            column.Item().Text("MediTrack - Analisis de Roles y Propuesta de Mejora")
                .FontSize(22).Bold().FontColor(Colors.Blue.Darken2);
            column.Item().Text("Documento de apoyo para revisar la experiencia de administrador, doctor y paciente dentro de la aplicacion.")
                .FontSize(11).FontColor(Colors.Grey.Darken2);
            column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });

        page.Content().Column(column =>
        {
            column.Spacing(14);

            AddSection(column, "1. Vision general", new []
            {
                "MediTrack ya tiene una base funcional clara: acceso por roles, gestion de usuarios, asignaciones, medicacion, mediciones, notas e informes.",
                "La siguiente etapa no pasa por anadir modulos sin control, sino por pulir la experiencia de cada perfil para que cada usuario entienda rapido que puede hacer y como hacerlo.",
                "Este documento resume la utilidad de cada apartado, detecta oportunidades de mejora y propone los siguientes pasos recomendados."
            });

            AddRoleSection(column,
                "2. Rol Administrador",
                "Su funcion principal es mantener el sistema ordenado, controlar el acceso y garantizar que la estructura base permita el trabajo del resto de usuarios.",
                new []
                {
                    "Resumen o dashboard: ofrece una vision rapida del volumen de usuarios, pacientes, doctores y asignaciones activas.",
                    "Usuarios: permite crear cuentas, editar datos basicos, revisar roles y activar o desactivar accesos.",
                    "Asignaciones: conecta a cada paciente con su doctor actual y sostiene la relacion operativa del seguimiento clinico.",
                    "Informes: permite revisar informes generados, comprobar su estado y utilizar la informacion historica como apoyo administrativo o academico."
                },
                new []
                {
                    "Hacer mas evidente el estado de cada usuario con chips visuales de Activo, Inactivo, Doctor, Paciente o Administrador.",
                    "Anadir filtros combinados en usuarios: por rol, por estado y por texto libre en nombre o correo.",
                    "Mostrar historial de reasignaciones doctor-paciente con fechas de inicio y fin para dar trazabilidad administrativa real.",
                    "Incorporar acciones rapidas como restablecer contrasena, duplicar ficha base o exportar un listado filtrado."
                },
                new []
                {
                    "Prioridad alta: terminar de pulir la gestion de usuarios para que la ficha superior sea totalmente estable en cualquier resolucion.",
                    "Prioridad alta: anadir filtros y busqueda global dentro del listado de usuarios.",
                    "Prioridad media: vista de historial de asignaciones y cambios de estado.",
                    "Prioridad media: exportacion administrativa de listados a PDF o Excel."
                });

            AddRoleSection(column,
                "3. Rol Doctor",
                "Su objetivo es revisar pacientes asignados, entender su evolucion clinica y dejar trazabilidad medica util para el paciente y para el propio seguimiento.",
                new []
                {
                    "Resumen o dashboard: resume cuantos pacientes tiene asignados, cual esta activo en contexto y si hay informacion clinica relevante a revisar.",
                    "Pacientes: actua como puerta de entrada al detalle de cada paciente y fija el contexto clinico para el resto de modulos.",
                    "Enfermedades: permite revisar patologias cronicas registradas y actualizar observaciones relacionadas con cada caso.",
                    "Medicacion: permite controlar tratamiento actual, frecuencia, horario y observaciones del plan terapeutico.",
                    "Mediciones: sirve para interpretar evolucion clinica a partir de glucosa, peso, presion arterial y otros indicadores.",
                    "Notas medicas: conserva indicaciones, hallazgos y observaciones compartidas con el paciente cuando procede.",
                    "Informes: resume el estado del paciente en un periodo concreto y facilita la exportacion a PDF."
                },
                new []
                {
                    "Destacar alertas clinicas visuales cuando un paciente tenga mediciones fuera de rango o no tenga seguimiento reciente.",
                    "Permitir comparar periodos para ver tendencia de mejora o empeoramiento sin depender solo de una lista de registros.",
                    "Anadir una linea temporal clinica que mezcle mediciones, notas, cambios de medicacion y reasignaciones del historial.",
                    "Mejorar la seleccion del paciente activo para que siempre quede muy visible en todos los modulos."
                },
                new []
                {
                    "Prioridad alta: mejorar lectura visual de mediciones y graficas con paneles mas claros y estados de alerta.",
                    "Prioridad alta: reforzar el detalle del paciente como vista central del rol doctor.",
                    "Prioridad media: timeline clinico unificado.",
                    "Prioridad media: plantillas de notas e informes para ahorrar tiempo al profesional."
                });

            AddRoleSection(column,
                "4. Rol Paciente",
                "Su funcion es registrar informacion de salud, comprender su estado actual y consultar el seguimiento que realiza el doctor de forma sencilla y motivadora.",
                new []
                {
                    "Dashboard: muestra un resumen de enfermedades, medicacion activa, proximas tomas, mediciones recientes y accesos a informes o notas.",
                    "Mi perfil: permite mantener actualizados los datos personales y de contacto.",
                    "Enfermedades: muestra el listado de patologias cronicas asociadas y sus observaciones principales.",
                    "Medicacion: ayuda a entender el tratamiento, horarios y recordatorios locales.",
                    "Mediciones: permite registrar valores periodicos y ver la evolucion de indicadores clave.",
                    "Notas medicas: da acceso a mensajes o indicaciones visibles emitidas por el doctor.",
                    "Informes: resume el estado del paciente y permite revisar o exportar un documento del periodo elegido."
                },
                new []
                {
                    "Hacer el dashboard mas explicativo y motivador, con mensajes tipo Proxima accion recomendada o Estado general de seguimiento.",
                    "Mostrar validaciones mas pedagogicas al introducir mediciones para evitar dudas sobre unidades o rangos.",
                    "Anadir semaforos sencillos en medicacion y mediciones para que el paciente entienda si todo esta al dia.",
                    "Crear una experiencia de informe mas orientada al paciente, con lenguaje menos tecnico y resumen entendible."
                },
                new []
                {
                    "Prioridad alta: mejorar usabilidad y claridad visual en modulos de medicacion y mediciones.",
                    "Prioridad alta: simplificar los mensajes clinicos para que el paciente entienda el valor de cada apartado.",
                    "Prioridad media: recordatorios mas claros y panel de adherencia al tratamiento.",
                    "Prioridad media: resumen de bienestar o adherencia mensual."
                });

            AddSection(column, "5. Siguientes pasos recomendados", new []
            {
                "Paso 1: estabilizar todos los layouts para que no haya bloques cortados, textos ocultos o controles superpuestos en ninguna resolucion.",
                "Paso 2: definir un lenguaje visual unico para tablas, formularios, tarjetas y mensajes del sistema.",
                "Paso 3: mejorar la experiencia del administrador en usuarios y asignaciones, porque es el punto que mantiene coherente el resto de flujos.",
                "Paso 4: reforzar el detalle clinico del doctor con mejor visualizacion de evolucion y contexto del paciente activo.",
                "Paso 5: simplificar la experiencia del paciente para que entienda mejor su medicacion, sus mediciones y el valor de los informes.",
                "Paso 6: una vez estabilizada la interfaz, preparar la migracion futura desde JSON a MySQL manteniendo contratos de repositorio y servicios."
            });

            AddSection(column, "6. Conclusion", new []
            {
                "La aplicacion ya cubre los flujos esenciales, pero todavia necesita una fase clara de refinamiento de experiencia por rol.",
                "El mayor retorno inmediato vendra de tres focos: consistencia visual, legibilidad de formularios y claridad de navegacion segun el usuario activo.",
                "Con ese pulido, MediTrack pasaria de ser una base funcional prometedora a una demo academico-profesional mucho mas convincente."
            });
        });

        page.Footer().AlignCenter().Text(text =>
        {
            text.Span("Documento generado para planificar mejoras de MediTrack - ");
            text.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).SemiBold();
        });
    });
}).GeneratePdf(outputFile);

Console.WriteLine(outputFile);

static void AddSection(ColumnDescriptor column, string title, string[] paragraphs)
{
    column.Item().Text(title).FontSize(15).Bold().FontColor(Colors.Blue.Darken2);
    foreach (var paragraph in paragraphs)
        column.Item().Text(paragraph).LineHeight(1.35f);
}

static void AddRoleSection(ColumnDescriptor column, string title, string purpose, string[] utilities, string[] improvements, string[] nextSteps)
{
    column.Item().Text(title).FontSize(15).Bold().FontColor(Colors.Blue.Darken2);
    column.Item().Text("Utilidad general: " + purpose).Bold();

    column.Item().Text("Apartados y utilidad especifica").SemiBold().FontColor(Colors.Grey.Darken2);
    foreach (var item in utilities)
        column.Item().Text("- " + item).LineHeight(1.3f);

    column.Item().Text("Como podria mejorar este rol").SemiBold().FontColor(Colors.Grey.Darken2);
    foreach (var item in improvements)
        column.Item().Text("- " + item).LineHeight(1.3f);

    column.Item().Text("Siguientes pasos para este rol").SemiBold().FontColor(Colors.Grey.Darken2);
    foreach (var item in nextSteps)
        column.Item().Text("- " + item).LineHeight(1.3f);
}
