namespace Social.Data.Model.Response.Base
{
    /// <summary>
    /// Cursor-free pagination envelope for infinite-scroll lists (skip/take).
    /// HasMore drives the client: keep loading while true, show the
    /// "you're all caught up" state once it flips to false.
    /// </summary>
    public class PagedResponse<T>
    {
        public List<T> Items { get; set; } = new();
        public bool HasMore { get; set; }
    }
}
