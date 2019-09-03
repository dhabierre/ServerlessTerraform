namespace Microsoft.Azure.ServiceBus
{
    using System.Text;

    internal static class MessageExtensions
    {
        public static string ReadBodyAsString(this Message @this)
        {
            if (@this == null)
            {
                return null;
            }

            return Encoding.UTF8.GetString(@this.Body);
        }
    }
}