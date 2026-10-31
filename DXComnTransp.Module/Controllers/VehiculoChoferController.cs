using DevExpress.ExpressApp;
using DXComnTransp.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DXComnTransp.Module.Controllers;

public class VehiculoChoferController : ObjectViewController<DetailView, VehiculoChofer>
{
    protected override void OnActivated()
    {
        base.OnActivated();

        ObjectSpace.Committing += ObjectSpace_Committing;
    }

    private void ObjectSpace_Committing(object sender, EventArgs e)
    {
        if (View.CurrentObject is not VehiculoChofer asignacion)
            return;

        ValidarAsignacion(asignacion);
    }

    private void ValidarAsignacion(VehiculoChofer asignacion)
    {
        // ============================================================
        // VEHÍCULO
        // ============================================================

        if (asignacion.Vehiculo == null || asignacion.IdVehiculo <= 0)
            throw new UserFriendlyException(
                "Debe seleccionar el vehículo."
            );


        // ============================================================
        // CHOFER PRINCIPAL
        // ============================================================

        if (asignacion.Chofer == null || asignacion.IdChofer <= 0)
            throw new UserFriendlyException(
                "Debe seleccionar el chofer principal."
            );


        // ============================================================
        // OBTENER LOS CHOFERES DE ESTA ASIGNACIÓN
        // ============================================================

        var choferes = new List<(long Id, string Nombre)>
        {
            (
                asignacion.IdChofer,
                asignacion.Chofer?.NombreChofer
            )
        };

        if (asignacion.IdChoferSdo.HasValue &&
            asignacion.IdChoferSdo.Value > 0)
        {
            choferes.Add(
                (
                    asignacion.IdChoferSdo.Value,
                    asignacion.ChoferSdo?.NombreChofer
                )
            );
        }

        if (asignacion.IdChoferTro.HasValue &&
            asignacion.IdChoferTro.Value > 0)
        {
            choferes.Add(
                (
                    asignacion.IdChoferTro.Value,
                    asignacion.ChoferTro?.NombreChofer
                )
            );
        }


        // ============================================================
        // VALIDACIÓN 1
        // EL MISMO CHOFER NO PUEDE REPETIRSE
        // EN EL MISMO VEHÍCULO
        // ============================================================

        var repetido = choferes
            .GroupBy(x => x.Id)
            .FirstOrDefault(g => g.Count() > 1);

        if (repetido != null)
        {
            string nombre =
                repetido.First().Nombre;

            throw new UserFriendlyException(
                $"El chofer \"{nombre}\" no puede estar asignado " +
                $"más de una vez al vehículo " +
                $"\"{ObtenerMatricula(asignacion)}\"."
            );
        }


        // ============================================================
        // VALIDACIÓN 2
        // NINGÚN CHOFER PUEDE ESTAR EN OTRO VEHÍCULO
        // ============================================================

        foreach (var chofer in choferes)
        {
            ValidarChoferEnOtroVehiculo(
                asignacion,
                chofer.Id,
                chofer.Nombre
            );
        }
    }


    private void ValidarChoferEnOtroVehiculo(
        VehiculoChofer asignacion,
        long idChofer,
        string nombreChofer)
    {
        var consulta =
            ObjectSpace.GetObjectsQuery<VehiculoChofer>();


        // ============================================================
        // BUSCAR ASIGNACIONES DONDE APAREZCA EL CHOFER
        // ============================================================

        var asignacionExistente = consulta
            .Where(x =>
                (
                    x.IdChofer == idChofer ||
                    x.IdChoferSdo == idChofer ||
                    x.IdChoferTro == idChofer
                )
                &&
                x.IdVehiculo != asignacion.IdVehiculo
                &&
                x.Id != asignacion.Id
            )
            .Select(x => new
            {
                x.Id,
                x.IdVehiculo,
                Matricula = x.Vehiculo.Matricula
            })
            .FirstOrDefault();


        if (asignacionExistente == null)
            return;


        // ============================================================
        // MENSAJE
        // ============================================================

        string matriculaAnterior =
            asignacionExistente.Matricula;

        string matriculaNueva =
            ObtenerMatricula(asignacion);


        throw new UserFriendlyException(
            $"El chofer \"{nombreChofer}\" ya está asignado " +
            $"al vehículo \"{matriculaAnterior}\" y no puede " +
            $"ser asignado al vehículo \"{matriculaNueva}\"."
        );
    }


    private string ObtenerMatricula(VehiculoChofer asignacion)
    {
        if (asignacion.Vehiculo != null &&
            !string.IsNullOrWhiteSpace(
                asignacion.Vehiculo.Matricula))
        {
            return asignacion.Vehiculo.Matricula;
        }

        if (asignacion.IdVehiculo > 0)
        {
            return ObjectSpace
                .GetObjectsQuery<Vehiculo>()
                .Where(x => x.Id == asignacion.IdVehiculo)
                .Select(x => x.Matricula)
                .FirstOrDefault()
                ?? "Sin matrícula";
        }

        return "Sin matrícula";
    }


    protected override void OnDeactivated()
    {
        ObjectSpace.Committing -= ObjectSpace_Committing;

        base.OnDeactivated();
    }
}