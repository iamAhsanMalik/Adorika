using Adorika.Application.Common.Wrapper;
using Mediator;

namespace Adorika.Application.Features.Installation.GetInstallationStatus;

public sealed record InstallationStatusQuery : IQuery<Result<InstallationStatusResponse>> { }
