using LiteBus.Queries.Abstractions;
using SB.PortalSolicitudes.Application.Common.Resultados;
using SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Dtos;

namespace SB.PortalSolicitudes.Application.Features.EntidadesGubernamentales.Queries.ObtenerEntidadesGubernamentalesActivas;

public sealed record ObtenerEntidadesGubernamentalesActivasQuery : IQuery<Resultado<IReadOnlyList<EntidadGubernamentalAdminDto>>>;
