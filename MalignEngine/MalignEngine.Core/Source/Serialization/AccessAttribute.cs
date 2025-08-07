namespace MalignEngine
{
    [Flags]
    public enum AccessPermissions : byte
    {
        None = 0,

        Read = 1 << 0, // 1
        Write = 1 << 1, // 2
        Execute = 1 << 2, // 4

        ReadWrite = Read | Write,
        ReadExecute = Read | Execute,
        WriteExecute = Write | Execute,
        ReadWriteExecute = Read | Write | Execute,
    }

    public static class AccessPermissionsExtensions
    {
        public static string ToUnixPermissions(this AccessPermissions permissions)
        {
            return permissions switch
            {
                AccessPermissions.None => "---",
                AccessPermissions.Read => "r--",
                AccessPermissions.Write => "-w-",
                AccessPermissions.Execute => "--x",
                AccessPermissions.ReadWrite => "rw-",
                AccessPermissions.ReadExecute => "r-x",
                AccessPermissions.WriteExecute => "-wx",
                AccessPermissions.ReadWriteExecute => "rwx",
                _ => throw new ArgumentOutOfRangeException(nameof(permissions), permissions, null)
            };
        }
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct
                | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor)]
    public sealed class AccessAttribute : Attribute
    {
        public readonly Type[] Friends;

        public const AccessPermissions SelfDefaultPermissions = AccessPermissions.ReadWriteExecute;
        public const AccessPermissions FriendDefaultPermissions = AccessPermissions.ReadWriteExecute;
        public const AccessPermissions OtherDefaultPermissions = AccessPermissions.Read;

        /// <summary>
        ///     Access permissions for the type itself, or the type containing the member.
        /// </summary>
        public AccessPermissions Self { get; set; } = SelfDefaultPermissions;
        public AccessPermissions Friend { get; set; } = FriendDefaultPermissions;
        public AccessPermissions Other { get; set; } = OtherDefaultPermissions;

        public AccessAttribute(params Type[] friends)
        {
            Friends = friends;
        }
    }
}