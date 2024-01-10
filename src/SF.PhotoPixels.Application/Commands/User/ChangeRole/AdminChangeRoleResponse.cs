using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.User.ChangeRole;

[GenerateOneOf]
public partial class AdminChangeRoleResponse : OneOfBase<Success, NotFound, Forbidden>
{
}