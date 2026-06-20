# Mobile RTS — Humans vs Orcs (guía para Claude)

Juego móvil (iOS/Android) en Unity 6, C#. RTS con gestión de aldea + batallas por
escuadrón, estilo **Pixel Tribe / Top Troops / Battle Legion**. La fuente de verdad del
diseño son los dos PDFs del usuario ("Ciclo de Juego" y "Sistema de Crafteo de Guerreros").
Este archivo resume las **reglas de diseño** y las **convenciones técnicas**. Si una decisión
contradice estas reglas, seguir las reglas (o preguntar).

## Arquitectura de dos escenas

1. **Escena Aldea (PlayScene)** — gestión: construir, recolectar, farmear, procesar, cocinar,
   craftear guerreros. NO hay combate aquí (el modo "defensa de oleadas" original está descartado).
2. **Escena Batalla (BattleScene)** — escena separada, grid **14×5**, ejército vs ejército.

`BaseGameManager` es compartido por ambas escenas; `GameManager` (aldea) y `BattleGameManager`
(batalla) lo extienden.

---

## Reglas de diseño (canónicas, de los PDFs)

### Aldeanos y asignación (REGLA CENTRAL)
- Los aldeanos **deben ser asignados a un edificio/tarea** para que el edificio haga su trabajo.
  Nada produce solo: hay que asignar un aldeano (construir, plantar, cosechar, talar, minar, cocinar…).
- Roles: agricultor, leñador, constructor, herrero, minero, cocinero, pescador.

### Población
- **Castillo**: edificio inicial, da **+4 de población** base.
- **Casa**: da **+1** de población. Una casa **sin ocupantes** puede **generar un aldeano** (tarda X tiempo).
  (⚠️ regla: la casa genera aldeano sólo si está desocupada, no producción infinita.)
- Podés generar tantos aldeanos/guerreros como **viviendas disponibles** permitan.
- **Sólo X guerreros van a la batalla** (guerrero = guerrero, mago, arquero). Es un límite aparte del de población.

### Recursos
- Naturales: **Madera, Piedra, Hierro, Oro, Carbón, Fruta** (bayas, manzanas).
- Cultivos: **Trigo, Maíz** (y verduras a futuro).
- Procesados: **Harina** (de trigo), **Lingotes** (hierro + carbón), **Fardos** (de trigo), Huevos, Leche, Carne, Pescado, Queso.

### Producción agrícola
- **Huerto**: produce **Trigo** o **Maíz** (un cultivo por huerto). MVP: sólo trigo y maíz.
  Ciclo: el aldeano **planta** y **cosecha cuando está listo**.
- **Molino** (MVP): convierte **Trigo → Harina**.
- **Empacadora de Fardos**: Trigo → Fardos (para alimentar/reproducir vacas).
- **Gallinero**: Huevos. **Ganadería**: vacas → Leche, Carne, reproducción con fardos. **Pescadería**: peces.
- **Mina**: Hierro, Carbón, Oro. **Fundición**: Hierro + Carbón → Lingotes.

### Cocina y recetas
- **Cocina** (necesita aldeano cocinero). MVP: **Pan = Harina → restaura poco HP**.
- Recetas completas (post-MVP): Pastel de Bayas (Harina+Huevo+Baya → HP+maná), Huevos Revueltos
  (→ fuerza), Carne Asada (Carne+Carbón → recuperación alta), Flan de Leche (→ agilidad),
  Pizza Campesina (→ ataque+defensa), etc. Las comidas dan **bonus de stats / recuperan HP**.

### Crafteo de guerreros
- Edificios: **Sala de Entrenamiento** (cuerpo a cuerpo, MVP), **Torre de Arquería**, **Academia de Magia**.
- Receta: **Aldeano + Comida (opcional) + Materiales (opcional, ej. lingotes/cuero/piedra de loot)** →
  guerrero con stats determinados por la combinación.
- **Herrería**: mejora armas/armaduras con lingotes. **Armería** (MVP): crea/almacena una espada.

### Stats base de unidades de combate
- **Agilidad** (evasión/esquiva), **Fuerza** (daño físico melee — sólo físicas),
  **Poder Mágico** (daño mágico — sólo magos), **Defensa** (reduce daño), **Velocidad** (frecuencia de ataque).

### Batalla (escena separada, estilo Top Troops)
- Grid **14×5**. Lado jugador (cols 0–6) vs enemigo (cols 7–13).
- **Coste de entrada** para iniciar la batalla (moneda distinta) — pendiente de implementar.
- Se puede **editar el ejército antes** de la batalla (menú lateral: arrastrar unidades al campo).
- Duración máxima **1:30**. Victoria = derrotar a todos; si se acaba el tiempo, gana el equipo con **más HP total**.
- Cada batalla se califica **0–3 estrellas** (tiempo, bajas, eficiencia). Al ganar → premios/botín.
- Heridas y muerte: los guerreros pueden morir; recuperación acelerada con comidas especiales (pan, empanadas, queso).

### Mapa / progresión
- Cada mapa tiene X niveles (peleas). **Avanzás de nivel ganando con ≥1 estrella**.
- Para desbloquear el siguiente mapa hay que **acumular X estrellas**. Cada X estrellas por mapa → premio.

### Cementerio
- Soldados muertos van al cementerio (escena aparte). Revivir cuesta X elementos. Tiene límite + lista de espera.
- Reciclar devuelve parte del equipo + un recurso extra (ej. hueso).

### Unidades confirmadas (visión)
Guerrero, Mago, Arquero, Asesino, Tanque, Lancero, Curandero, Hechicero, Nigromante.
Empezar con 9–12 unidades al lanzamiento.

---

## Convenciones técnicas (establecidas en este proyecto)

- **Managers como clases planas** propiedad de `BaseGameManager` (no MonoBehaviour singletons):
  `ResourceManager`, `PopulationManager`. Evita la fragilidad de resolver singletons entre escenas.
  Patrón: dato + evento `OnChanged`; la población actual se **deriva** (contar unidades vivas), no se cuenta a mano.
- **Recursos**: `ResourceType` enum (`Gold, Wood, Food, Iron, Carbon, Wheat, Corn, Flour`).
  Acceso vía `BaseGameManager`: `GetResource/AddResource/HasResource/SpendResource`, o `ResourceBank`
  (nombrado así para no chocar con `UnityEngine.Resources`).
- **UI en runtime, sin fricción de Editor**: construir paneles/botones por código (ver `BuildMenu`,
  `BuildAssignButton`, `ResourceDataUI` que autocrea labels, `ProductionBar`). El usuario prefiere
  evitar pasos manuales en el Inspector; cuando algo necesita un asset, **generarlo en YAML** (prefab/.asset + .meta).
- **Edificios** extienden `StructureUnit`. `AfterConstructionUpdate()` corre por frame tras construir;
  `OnConstructionFinished()` al terminar. Footprint de pathfinding configurable (`WalkabilityWidth/Height`).
- **Asignación de trabajadores**: interfaz `IWorkerAssignable` (cimiento=construir, huerto=plantar,
  árbol=cortar). Botón flotante genérico (`BuildAssignButton`) debajo de lo seleccionado → asigna el
  obrero más cercano. La construcción **no arranca hasta** que un obrero es asignado.
- **Construcción**: colocar cimiento (estado base, cobra recursos al colocar) → asignar obrero → se construye.
  Catálogo global desde el botón **"Construir"** (`BuildCatalogSO` en `Resources/BuildCatalog.asset`).
- Campos privados con prefijo `m_`. `Time.timeScale` se resetea en transiciones de escena.

## Flujo de trabajo

- Comunicación en **español**. Commits por feature (mensaje termina con `Co-Authored-By: Claude...`).
  Trabajo en rama `feature/panel-units`; pushear sólo cuando el usuario lo pide.
- Al tocar mecánicas, **respetar las reglas de diseño de arriba**. Si la implementación actual las
  contradice (p.ej. huerto que produce sin ciclo plantar/cosechar, o casa que genera de más),
  alinearla con el documento.

## Estado actual (features implementadas)
- Escena de batalla (grid, drag&drop, combate, timer, estrellas, victoria/derrota) — Feature 9.
- `ResourceManager` multi-recurso — Feature 1.
- Población + Casa + UI de población — Feature 2.
- Farming (Huerto Trigo/Maíz, obrero asignado, panel de recursos) — Feature 3.
- Build UI (botón Construir, catálogo, info, cimiento, asignar trabajador genérico) — en curso.

## Pendiente (no implementado aún)
Molino, Cocina+Recetas, Stats de unidades en combate, Crafteo de guerreros, Mago, Save/Load,
Mapa de progresión, Cementerio, coste de entrada a batalla, edición del ejército antes de la batalla.
