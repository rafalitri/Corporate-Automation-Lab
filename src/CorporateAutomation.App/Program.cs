using CorporateAutomation.App;

List<Tarea> tareasEmpresa = new();

TareaService servicio = new();

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
                Tarea? tareaEncontrada = servicio.BuscarTareaPorId(
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
            int pendientes = servicio.ContarTareasPendientes(
                tareasEmpresa
            );

            Console.WriteLine($"Tareas pendientes: {pendientes}");
            break;

        case "5":
            Console.Write(
                "Escribe el ID de la tarea que quieres completar: "
            );

            string textoIdCompletar = Console.ReadLine() ?? "";

            if (int.TryParse(
                textoIdCompletar,
                out int idCompletar
            ))
            {
                bool completada = servicio.CompletarTarea(
                    tareasEmpresa,
                    idCompletar
                );

                if (completada)
                {
                    Console.WriteLine(
                        "Tarea completada correctamente."
                    );
                }
                else
                {
                    Console.WriteLine(
                        "No existe una tarea con ese ID."
                    );
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

    PrioridadTarea prioridad;

    while (true)
    {
        Console.WriteLine("Selecciona la prioridad:");
        Console.WriteLine("1. Baja");
        Console.WriteLine("2. Media");
        Console.WriteLine("3. Alta");
        Console.Write("Opción: ");

        string opcionPrioridad = Console.ReadLine() ?? "";

        if (opcionPrioridad == "1")
        {
            prioridad = PrioridadTarea.Baja;
            break;
        }
        else if (opcionPrioridad == "2")
        {
            prioridad = PrioridadTarea.Media;
            break;
        }
        else if (opcionPrioridad == "3")
        {
            prioridad = PrioridadTarea.Alta;
            break;
        }
        else
        {
            Console.WriteLine("Prioridad incorrecta.");
        }
    }

    Tarea nuevaTarea = new()
    {
        Id = id,
        Nombre = nombre,
        Estado = EstadoTarea.Pendiente,
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
            $"{tarea.Id} - {tarea.Nombre} - " +
            $"{tarea.Estado} - {tarea.Prioridad}"
        );
    }
}
