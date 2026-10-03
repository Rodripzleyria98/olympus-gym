using Olympus.Api.Models;

namespace Olympus.Api.Data;

public static class MembresiaSeedData
{
    public static readonly PlanMembresia[] Planes =
    [
        new PlanMembresia
        {
            Id = 1,
            Nombre = "Pase Libre",
            Descripcion = "Acceso total e ilimitado a la sala de musculación, máquinas y todas las instalaciones durante todo el mes.",
            Precio = 28000.00m,
            DuracionDias = 30,
            ClasesIncluidas = null,
            IsActivo = true
        },
        new PlanMembresia
        {
            Id = 2,
            Nombre = "Pase 3 veces por semana (12 clases)",
            Descripcion = "Incluye 12 clases o asistencias al mes para entrenar 3 días por semana según tu disponibilidad.",
            Precio = 26000.00m,
            DuracionDias = 30,
            ClasesIncluidas = 12,
            IsActivo = true
        },
        new PlanMembresia
        {
            Id = 3,
            Nombre = "Pase 2 veces por semana (8 clases)",
            Descripcion = "Incluye 8 clases o asistencias al mes para entrenar 2 días por semana.",
            Precio = 22000.00m,
            DuracionDias = 30,
            ClasesIncluidas = 8,
            IsActivo = true
        }
    ];
}