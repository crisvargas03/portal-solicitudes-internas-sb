using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadGubernamentalPorId;

public sealed record ObtenerEntidadGubernamentalPorIdQuery(int Id) : IQuery<Resultado<EntidadGubernamentalAdminDto>>;
