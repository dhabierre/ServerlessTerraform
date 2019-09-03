namespace System.IO
{
    using System.Text;
    using System.Threading.Tasks;

    internal static class StreamExtensions
    {
        public async static Task<string> ReadStringAsync(this Stream @this)
        {
            @this.Position = 0;

            using (var reader = new StreamReader(@this, Encoding.UTF8))
            {
                return await reader.ReadToEndAsync();
            }
        }
    }
}