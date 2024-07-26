using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.User.DeleteUser;

[GenerateOneOf]
public partial class DeleteUserResponse : OneOfBase<Success, NotFound, Forbidden>
{
}