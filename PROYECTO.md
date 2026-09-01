# PROMPT PARA CODEX — PROYECTO .NET MAUI

Quiero desarrollar desde cero una aplicación móvil Android utilizando **.NET MAUI con C#**, cuyo objetivo sea automatizar la gestión de mensajes SMS a partir de información de clientes obtenida inicialmente desde archivos CSV y posteriormente desde Google Sheets/Google Drive.

## 1. OBJETIVO GENERAL

La aplicación será un **gestor automatizado de comunicaciones de cobranza**.

La aplicación debe permitir:

1. Importar un archivo CSV.
2. Detectar y mapear sus columnas.
3. Validar y normalizar los datos.
4. Permitir múltiples números telefónicos por cliente.
5. Almacenar los datos localmente.
6. Evaluar reglas de negocio relacionadas con deudas y fechas de vencimiento.
7. Determinar automáticamente qué clientes deben recibir un mensaje.
8. Generar mensajes personalizados mediante plantillas.
9. Enviar SMS utilizando las capacidades del propio dispositivo Android, sin utilizar servicios externos de SMS de pago.
10. Registrar el resultado de cada intento de envío.
11. Mantener historial de mensajes.
12. Evitar mensajes duplicados.
13. Permitir reintentos de mensajes fallidos.
14. Preparar la arquitectura para posteriormente sincronizar datos con Google Sheets/Google Drive.

IMPORTANTE:

No quiero comenzar implementando Google Drive. La primera versión debe funcionar completamente con CSV local.

Tampoco quiero utilizar un backend obligatorio para el MVP. La aplicación debe funcionar principalmente de manera local en el dispositivo.

---

# 2. TECNOLOGÍA

Utilizar:

* .NET MAUI
* C#
* Android como plataforma principal
* MVVM
* SQLite para persistencia local
* Dependency Injection
* Servicios separados por responsabilidad
* Async/Await
* Nullable Reference Types
* Configuración mediante interfaces cuando corresponda

Quiero una arquitectura limpia, modular y mantenible.

NO quiero colocar toda la lógica dentro de las páginas XAML.

NO quiero utilizar code-behind para lógica de negocio.

---

# 3. ARQUITECTURA PROPUESTA

Organiza el proyecto aproximadamente de esta manera:

```text
DebtMessageManager/
│
├── Models/
│   ├── Cliente.cs
│   ├── Telefono.cs
│   ├── Mensaje.cs
│   ├── PlantillaMensaje.cs
│   ├── ConfiguracionAutomatizacion.cs
│   └── ReglaEnvio.cs
│
├── Data/
│   ├── AppDatabase.cs
│   ├── DatabaseInitializer.cs
│   └── Repositories/
│       ├── ClienteRepository.cs
│       ├── TelefonoRepository.cs
│       ├── MensajeRepository.cs
│       └── PlantillaRepository.cs
│
├── Services/
│   ├── Csv/
│   │   ├── ICsvService.cs
│   │   └── CsvService.cs
│   │
│   ├── Validation/
│   │   ├── IDataValidationService.cs
│   │   └── DataValidationService.cs
│   │
│   ├── Messaging/
│   │   ├── ISmsService.cs
│   │   └── SmsService.cs
│   │
│   ├── Automation/
│   │   ├── IAutomationService.cs
│   │   └── AutomationService.cs
│   │
│   ├── Templates/
│   │   ├── IMessageTemplateService.cs
│   │   └── MessageTemplateService.cs
│   │
│   └── Import/
│       ├── IImportService.cs
│       └── ImportService.cs
│
├── ViewModels/
│   ├── MainViewModel.cs
│   ├── ImportViewModel.cs
│   ├── ClientesViewModel.cs
│   ├── ClienteDetalleViewModel.cs
│   ├── MensajesViewModel.cs
│   ├── AutomatizacionViewModel.cs
│   └── ConfiguracionViewModel.cs
│
├── Views/
│   ├── MainPage.xaml
│   ├── ImportPage.xaml
│   ├── ClientesPage.xaml
│   ├── ClienteDetallePage.xaml
│   ├── MensajesPage.xaml
│   ├── AutomatizacionPage.xaml
│   └── ConfiguracionPage.xaml
│
├── Helpers/
│   ├── PhoneNumberHelper.cs
│   ├── DateHelper.cs
│   └── CurrencyHelper.cs
│
└── Resources/
```

Puedes modificar esta estructura si consideras que existe una arquitectura mejor, pero explica primero por qué.

---

# 4. MODELO DE DATOS DE ENTRADA

El CSV inicialmente tendrá información similar a:

```csv
ID,NOMBRE,MONTO_DEUDA,TELEFONOS,FECHA_VENCIMIENTO
001,Luciana Huaman,520,"940208029",25/08/2026
002,Juan Perez,210,"951208029",28/08/2026
003,Pedro Lopez,0,"999888777",20/08/2026
004,Ana Torres,350,"999111222 / 988333444",15/08/2026
```

Pero NO asumas que todos los archivos tendrán exactamente estos nombres de columnas.

El sistema debe permitir posteriormente mapear columnas.

Por ejemplo:

```text
Nombre
    ↓
NOMBRE_CLIENTE

Deuda
    ↓
MONTO

Teléfono
    ↓
CELULAR

Fecha de vencimiento
    ↓
FECHA_LIMITE
```

---

# 5. TELÉFONOS

Esta parte es importante.

Un cliente puede tener:

```text
940208029
```

o:

```text
940208029 / 951208029
```

o:

```text
940208029, 951208029
```

o:

```text
940208029;951208029
```

También puede existir:

```text
+51 940208029
```

o:

```text
951 208 029
```

La aplicación debe:

1. Detectar múltiples números.
2. Separarlos.
3. Eliminar espacios innecesarios.
4. Normalizar el formato.
5. Validar que tengan una estructura válida.
6. Evitar duplicados.
7. Asociarlos al cliente correspondiente.

Internamente NO quiero almacenar varios teléfonos como una sola cadena.

Debe existir una relación:

```text
Cliente
   |
   ├── Telefono 1
   ├── Telefono 2
   └── Telefono 3
```

---

# 6. ESTADO DE LA DEUDA

NO quiero depender de una columna ESTADO dentro del CSV.

El estado debe calcularse a partir de los datos.

Ejemplo:

```text
SI MONTO_DEUDA <= 0
    estado = SIN_DEUDA

SI MONTO_DEUDA > 0
Y fecha actual <= fecha vencimiento
    estado = VIGENTE

SI MONTO_DEUDA > 0
Y fecha actual > fecha vencimiento
    estado = VENCIDA
```

Utilizar un enum en C# en lugar de strings libres.

Por ejemplo:

```text
SinDeuda
Vigente
Vencida
```

Puedes agregar otros estados si la arquitectura lo necesita, pero evita complicar innecesariamente el MVP.

---

# 7. ESTADO DE LOS MENSAJES

No mezclar el estado de deuda con el estado del mensaje.

Crear un enum independiente:

```text
Pendiente
Programado
Enviando
Enviado
Error
```

Ejemplo:

```text
Cliente:
Luciana Huaman

Deuda:
S/ 520

EstadoDeuda:
Vencida

SMS:
Enviado
```

Esto debe ser posible.

---

# 8. MOTOR DE AUTOMATIZACIÓN

Crear un servicio independiente:

```csharp
IAutomationService
```

Su responsabilidad será determinar si un cliente debe recibir un mensaje.

Inicialmente implementar estas reglas:

### Regla 1 — Sin deuda

Si:

```text
MONTO_DEUDA <= 0
```

NO enviar ningún mensaje.

---

### Regla 2 — Deuda vigente

Si:

```text
MONTO_DEUDA > 0
```

pero:

```text
FECHA_ACTUAL <= FECHA_VENCIMIENTO
```

No enviar mensaje de cobranza vencida.

---

### Regla 3 — Deuda vencida

Si:

```text
MONTO_DEUDA > 0
```

y:

```text
FECHA_ACTUAL > FECHA_VENCIMIENTO
```

determinar los días de retraso.

Ejemplo:

```text
Fecha vencimiento: 25/08/2026
Fecha actual: 31/08/2026

Días de retraso: 6
```

---

# 9. DÍAS DE GRACIA

La configuración debe permitir:

```text
DiasGracia = 3
```

Ejemplo:

```text
Vencimiento:
25/08/2026

Días de gracia:
3

Fecha a partir de la cual puede enviarse:
29/08/2026
```

No enviar antes de que termine el período de gracia.

---

# 10. RECORDATORIOS

La aplicación debe permitir posteriormente configurar reglas como:

```text
3 días antes del vencimiento
1 día después
7 días después
15 días después
30 días después
```

Pero NO hardcodear estas reglas dentro de las páginas.

Crear una estructura que permita modificar las reglas desde configuración.

Por ejemplo:

```text
ReglaEnvio

Tipo:
AntesVencimiento
DespuesVencimiento

Dias:
3

PlantillaId:
1

Activa:
true
```

---

# 11. EVITAR MENSAJES DUPLICADOS

Este requisito es obligatorio.

Si la aplicación ejecuta la automatización:

```text
09:00
```

y vuelve a ejecutarla:

```text
09:05
```

NO debe volver a enviar el mismo mensaje.

Debe consultar el historial.

Por ejemplo:

```text
ClienteId
ReglaId
Fecha
Estado
```

Si ya existe un envío exitoso correspondiente a esa regla y período, no volver a enviarlo.

---

# 12. HORARIO DE ENVÍO

Crear configuración:

```text
HoraInicio = 08:00
HoraFin = 18:00
```

Si una regla determina que debe enviarse un mensaje fuera del horario:

```text
Estado = Programado
```

No enviarlo inmediatamente.

---

# 13. PLANTILLAS

Crear un sistema de plantillas.

Ejemplo:

```text
Hola {NOMBRE},

Te informamos que tienes una deuda pendiente
de S/ {MONTO}.

Fecha de vencimiento:
{FECHA_VENCIMIENTO}

Por favor, comunícate con nosotros para
regularizar tu situación.

Gracias.
```

Variables mínimas:

```text
{NOMBRE}
{MONTO}
{FECHA_VENCIMIENTO}
{DIAS_RETRASO}
```

El sistema debe reemplazar automáticamente las variables.

Crear un servicio:

```csharp
IMessageTemplateService
```

---

# 14. ENVÍO SMS

Crear:

```csharp
ISmsService
```

y una implementación específica para Android.

La lógica de negocio NO debe llamar directamente a APIs de Android.

Debe existir esta separación:

```text
AutomationService
        ↓
ISmsService
        ↓
AndroidSmsService
        ↓
Android
```

La implementación Android debe encargarse de:

* permisos necesarios
* acceso a las capacidades SMS
* envío
* captura de errores
* resultado del envío

IMPORTANTE:

Investiga y verifica las APIs y permisos actualmente disponibles en .NET MAUI/Android para envío de SMS antes de implementarlo.

No inventes APIs.

Si existen restricciones de Android o de distribución de aplicaciones que impidan algún nivel de automatización, documentarlas claramente y diseñar la aplicación para manejar esa limitación.

No utilizar servicios externos de SMS de pago.

---

# 15. SQLITE

Utilizar SQLite para persistencia local.

La base de datos debería contener como mínimo:

### Clientes

```text
Id
Nombre
MontoDeuda
FechaVencimiento
FechaImportacion
```

### Telefonos

```text
Id
ClienteId
Numero
Activo
```

### Mensajes

```text
Id
ClienteId
TelefonoId
PlantillaId
Contenido
FechaProgramada
FechaEnvio
Estado
Error
```

### Plantillas

```text
Id
Nombre
Contenido
Activa
```

### Reglas

```text
Id
Nombre
Tipo
Dias
PlantillaId
Activa
```

### Configuración

```text
Id
HoraInicio
HoraFin
DiasGracia
AutomatizacionActiva
```

Utilizar relaciones adecuadas y claves foráneas cuando corresponda.

---

# 16. IMPORTACIÓN CSV

La pantalla de importación debería permitir:

```text
[ Seleccionar archivo CSV ]
```

Después:

```text
Archivo seleccionado:
deudas_agosto.csv

Registros encontrados:
250
```

Mostrar una vista previa.

Después detectar columnas.

Ejemplo:

```text
MAPEAR COLUMNAS

Nombre:
[ NOMBRE ▼ ]

Monto:
[ MONTO_DEUDA ▼ ]

Teléfono:
[ TELEFONOS ▼ ]

Fecha vencimiento:
[ FECHA_VENCIMIENTO ▼ ]
```

Validar antes de importar.

Mostrar:

```text
250 registros encontrados

✓ 240 registros válidos
⚠ 7 registros con teléfonos inválidos
⚠ 3 registros sin fecha
```

No importar silenciosamente datos inválidos.

El usuario debe poder revisar los errores.

---

# 17. DASHBOARD

La pantalla principal debe mostrar:

```text
GESTOR DE COBRANZAS

Clientes
250

Sin deuda
85

Vigentes
60

Vencidos
105

Mensajes pendientes
72

Enviados
31

Errores
2
```

Agregar acciones:

```text
[ IMPORTAR CSV ]

[ EJECUTAR AUTOMATIZACIÓN ]

[ VER CLIENTES ]

[ VER MENSAJES ]

[ CONFIGURACIÓN ]
```

---

# 18. LISTADO DE CLIENTES

Mostrar:

```text
Luciana Huaman
S/ 520
Vencida
🔴

Juan Perez
S/ 210
Vigente
🟡

Pedro Lopez
S/ 0
Sin deuda
🟢
```

Filtros:

```text
Todos
Sin deuda
Vigentes
Vencidos
```

---

# 19. DETALLE DEL CLIENTE

Al seleccionar un cliente:

```text
Luciana Huaman

Deuda:
S/ 520

Vencimiento:
25/08/2026

Estado:
VENCIDA

Teléfonos:
940208029
951208029

Días de retraso:
6
```

Historial:

```text
25/08/2026
Recordatorio
Enviado

29/08/2026
Primer aviso
Enviado
```

---

# 20. AUTOMATIZACIÓN

Crear una pantalla:

```text
AUTOMATIZACIÓN

Estado:
🟢 ACTIVADA

Horario:
08:00 - 18:00

Días de gracia:
3

Reglas:

✓ 3 días antes
✓ 1 día después
✓ 7 días después
✓ 15 días después
```

Botones:

```text
[ GUARDAR ]

[ EJECUTAR AHORA ]
```

El botón "Ejecutar ahora" debe evaluar las reglas pero respetar las mismas validaciones de seguridad.

---

# 21. SEGURIDAD CONTRA ENVÍOS INCORRECTOS

Antes de ejecutar una campaña masiva:

Mostrar:

```text
RESUMEN DE ENVÍO

Clientes evaluados: 250

Sin deuda:
85

No corresponde:
60

Mensajes a enviar:
72

Números telefónicos:
78

Errores:
2

¿Deseas continuar?

[ CANCELAR ]

[ INICIAR ENVÍO ]
```

No comenzar a enviar inmediatamente sin confirmación en esta primera versión.

---

# 22. EXPORTACIÓN

Posteriormente permitir exportar resultados a CSV.

Por ejemplo:

```text
ID
NOMBRE
MONTO_DEUDA
TELEFONOS
FECHA_VENCIMIENTO
ESTADO_DEUDA
ESTADO_MENSAJE
ULTIMO_CONTACTO
```

Pero el historial completo debe permanecer en SQLite.

---

# 23. GOOGLE SHEETS / DRIVE

NO implementar todavía.

Pero diseña interfaces para poder agregarlo posteriormente.

Por ejemplo:

```csharp
IDataSourceService
```

Primera implementación:

```text
CsvDataSourceService
```

Futura implementación:

```text
GoogleSheetsDataSourceService
```

La aplicación no debería depender directamente de CSV en toda la lógica.

Arquitectura:

```text
IDataSourceService
       │
       ├── CSV
       │
       └── Google Sheets (futuro)
```

---

# 24. AUTOMATIZACIÓN REAL EN ANDROID

Quiero que analices cuidadosamente qué significa "automático" en Android.

Diferenciar:

### Automatización de lógica

La aplicación determina:

```text
deuda > 0
+
vencimiento superado
+
regla cumplida
+
no se envió anteriormente
=
corresponde enviar
```

### Automatización del envío

La aplicación utiliza el dispositivo para enviar realmente el SMS.

Investigar las restricciones actuales de Android para:

* ejecución en segundo plano
* permisos SMS
* servicios en segundo plano
* WorkManager
* alarmas
* batería
* reinicio del dispositivo

NO asumir que un timer de .NET funcionará mientras la aplicación está cerrada.

Diseñar una solución Android correcta para ejecutar tareas programadas.

---

# 25. DESARROLLO POR FASES

NO implementes todo de golpe.

Quiero que trabajes por fases.

## FASE 1

Crear el proyecto:

```text
.NET MAUI
C#
Android
MVVM
SQLite
DI
```

Crear la estructura de carpetas y clases base.

---

## FASE 2

Implementar:

```text
Modelos
SQLite
Repositories
```

Probar que se puedan guardar y consultar clientes.

---

## FASE 3

Implementar:

```text
Importación CSV
Validación
Normalización de teléfonos
Mapeo de columnas
```

---

## FASE 4

Implementar:

```text
Motor de reglas
Estados de deuda
Días de gracia
Reglas de recordatorio
```

---

## FASE 5

Implementar:

```text
Plantillas
Variables
Generación de mensajes
```

---

## FASE 6

Implementar:

```text
SMS Android
Permisos
Envío
Manejo de errores
```

---

## FASE 7

Implementar:

```text
Automatización
Programación
Horarios
Prevención de duplicados
```

---

## FASE 8

Implementar:

```text
Dashboard
Historial
Filtros
Estadísticas
Exportación
```

---

## FASE 9 — FUTURA

Google Sheets / Google Drive.

---

# 26. REGLAS IMPORTANTES PARA EL DESARROLLO

1. No crear código innecesario.
2. No utilizar servicios externos de SMS.
3. No introducir un backend para el MVP.
4. No hardcodear reglas de negocio en XAML.
5. No mezclar lógica de negocio con UI.
6. Utilizar interfaces para servicios importantes.
7. Utilizar async/await.
8. Manejar excepciones correctamente.
9. Validar datos antes de insertarlos.
10. Evitar duplicados.
11. Mantener historial.
12. No eliminar historial al actualizar un cliente.
13. No asumir que un cliente tiene un solo teléfono.
14. No asumir nombres específicos para las columnas del CSV.
15. No enviar mensajes cuando la deuda sea 0.
16. No enviar mensajes duplicados.
17. No enviar fuera del horario configurado.
18. No enviar antes de cumplir las reglas de vencimiento.
19. Registrar errores de envío.
20. Diseñar pensando en que posteriormente habrá Google Sheets.

---

# 27. PRIMER PASO

Por ahora NO quiero que implementes toda la aplicación.

Primero:

1. Verifica el SDK de .NET instalado y compatible.
2. Indica qué versión de .NET MAUI recomiendas actualmente para el proyecto.
3. Crea el proyecto MAUI.
4. Configura Android.
5. Configura MVVM.
6. Configura Dependency Injection.
7. Configura SQLite.
8. Crea la estructura de carpetas.
9. Crea los modelos iniciales.
10. Crea las interfaces principales.
11. Configura navegación básica.
12. Comprueba que el proyecto compile.
13. Ejecuta una prueba básica en Android.

Después de completar esta primera fase, DETENTE.

No implementes todavía CSV, Google Drive ni SMS.

Quiero revisar primero que la arquitectura base esté correctamente construida.

Cuando termines, explícame:

* qué archivos creaste
* qué responsabilidad tiene cada uno
* qué paquetes NuGet instalaste y por qué
* qué versión de .NET utilizaste
* cómo ejecutar el proyecto
* cómo probarlo en un dispositivo Android
* qué falta para la siguiente fase

No continúes con la siguiente fase hasta recibir mi confirmación.
