using CorporateAutomation.App;

List<Tarea> tareasEmpresa = new();

int siguienteId = 1;
bool ejecutando = true;

while (ejecutando)
{
    Console.WriteLine();
    Console.WriteLine("=== CORPORATE AUTOMATION ===");
    Console.WriteLine("1. Crear tarea");
    Console.WriteLine("2. Ver tareas");
    Console.WriteLine("3. Buscar tarea por ID");
    Console.WriteLine("4. Ver tareas pendientes");
    Console.WriteLine("5. Completar tarea");
    Console.WriteLine("6. Salir");
    Console.Write("Selecciona una opción: ");

    string opcion = Console.ReadLine() ?? "";

    switch (opcion)
    {
        case "1":
            Tarea nuevaTarea = CrearTarea(siguienteId);

            tareasEmpresa.Add(nuevaTarea);
            siguienteId++;

            Console.WriteLine("Tarea creada correctamente.");
            break;

        case "2":
            MostrarTareas(tareasEmpresa);
            break;

        case "3":
            Console.Write("Escribe el ID de la tarea: ");
            string textoId = Console.ReadLine() ?? "";

            if (int.TryParse(textoId, out int idBuscado))
            {
                Tarea? tareaEncontrada = BuscarTareaPorId(
                    tareasEmpresa,
                    idBuscado
                );

                if (tareaEncontrada is not null)
                {
                    Console.WriteLine(
                        $"{tareaEncontrada.Id} - " +
                        $"{tareaEncontrada.Nombre} - " +
                        $"{tareaEncontrada.Estado} - " +
                        $"{tareaEncontrada.Prioridad}"
                    );
                }
                else
                {
                    Console.WriteLine("No existe una tarea con ese ID.");
                }
            }
            else
            {
                Console.WriteLine("El ID debe ser un número.");
            }

            break;

        case "4":
            int pendientes = ContarTareasPendientes(tareasEmpresa);

            Console.WriteLine($"Tareas pendientes: {pendientes}");
            break;

        case "5":
            Console.Write("Escribe el ID de la tarea que quieres completar: ");
            string textoIdCompletar = Console.ReadLine() ?? "";

            if (int.TryParse(textoIdCompletar, out int idCompletar))
            {
                bool completada = CompletarTarea(
                    tareasEmpresa,
                    idCompletar
                );

                if (completada)
                {
                    Console.WriteLine("Tarea completada correctamente.");
                }
                else
                {
                    Console.WriteLine("No existe una tarea con ese ID.");
                }
            }
            else
            {
                Console.WriteLine("El ID debe ser un número.");
            }

            break;

        case "6":
            ejecutando = false;
            Console.WriteLine("Saliendo del programa...");
            break;

        default:
            Console.WriteLine("Opción incorrecta.");
            break;
    }
}


static Tarea CrearTarea(int id)
{
    Console.Write("Nombre de la tarea: ");
    string nombre = Console.ReadLine() ?? "";

    Console.Write("Prioridad: ");
    string prioridad = Console.ReadLine() ?? "";

    Tarea nuevaTarea = new()
    {
        Id = id,
        Nombre = nombre,
        Estado = "Pendiente",
        Prioridad = prioridad
    };

    return nuevaTarea;
}


static void MostrarTareas(List<Tarea> tareas)
{
    if (tareas.Count == 0)
    {
        Console.WriteLine("No hay tareas.");
        return;
    }

    Console.WriteLine();
    Console.WriteLine("=== TAREAS ===");

    foreach (Tarea tarea in tareas)
    {
        Console.WriteLine(
            $"{tarea.Id} - {tarea.Nombre} - {tarea.Estado} - {tarea.Prioridad}"
        );
    }
}


static Tarea? BuscarTareaPorId(List<Tarea> tareas, int id)
{
    foreach (Tarea tarea in tareas)
    {
        if (tarea.Id == id)
        {
            return tarea;
        }
    }

    return null;
}


static int ContarTareasPendientes(List<Tarea> tareas)
{
    int cantidad = 0;

    foreach (Tarea tarea in tareas)
    {
        if (tarea.Estado == "Pendiente")
        {
            cantidad++;
        }
    }

    return cantidad;
}

static bool CompletarTarea(List<Tarea> tareas, int id)
{
    Tarea? tarea = BuscarTareaPorId(tareas, id);

    if (tarea is null)
    {
        return false;
    }

    tarea.Estado = "Completada";

    return true;
}